using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MEC;
using PokerMode.Traits;
using UnityEngine;
using Utilities;

namespace PokerMode
{
    public class Table
    {
        public IEnumerable<Player> Players => _players;
        public Player HumanPlayer { get; private set; }
        public Deck Deck { get; }
        public List<Card> River { get; }
        public CreditVisuals PotVisuals { get; }
        public int SmallBlind { get; }
        public int BigBlind { get; }
        public bool TournamentRules { get; }
        public float CallBoost { get; }
        public int MinRaise { get; set; }

        public int Pot => _mainPot + _sidePots.Sum(x => x.value);
        
        private List<Player> _players = new();
        private int _mainPot;
        private List<(int value, List<Player> participants)> _sidePots = new();
        private Player _dealer; // Player with the dealer button
        private bool _simulated;
        
        public void AddToPot(int credits) => _mainPot += credits;
        
        public Table(DeckVisuals deckVisuals, List<PlayerVisuals> aiPlayerVisuals, CreditVisuals potVisuals, int buyIn, int smallBlind, int bigBlind, bool tournamentRules = false, float callBoost = 0f)
        {
            _simulated = false;
            Deck = new Deck(deckVisuals);
            River = new();
            PotVisuals = potVisuals;
            SmallBlind = smallBlind;
            BigBlind = bigBlind;
            TournamentRules = tournamentRules;
            CallBoost = callBoost;
            
            foreach (var playerVisuals in aiPlayerVisuals)
                _players.Add(new Player(playerVisuals, credits: buyIn));
            
#if UNITY_EDITOR
            if (Service.GameController.DEBUG_CustomStartingCash)
                foreach (var player in _players)
                    player.Credits = player.Visuals.DEBUG_CustomStartingCash;
#endif
        }
        
        public Table(Table toSimulate, Player fixedPlayer)
        {
            _simulated = true;
            Deck = new Deck(toSimulate.Deck, toSimulate.Players.Where(x => x != fixedPlayer && !x.Folded).SelectMany(x => x.Hand));
            River = new();
            foreach (var card in toSimulate.River) River.Add(new Card(card));
            
            foreach (var player in toSimulate.Players.Where(x => x != fixedPlayer && !x.Folded))
                _players.Add(new Player(player));
        }
        
        public void AddHumanPlayer(Player player)
        {
            _players.Add(player);
            HumanPlayer = player;
        }
        
        public Player NextPlayer(Player player, bool skipFolded = false, bool skipAllIn = false)
        {
            var index = _players.IndexOf(player);
            Debug.Assert(index != -1, $"Player {player} not found in Table");
            do
            {
                index = (index + 1) % _players.Count;
                player = _players[index];
            } while (player.NotPlaying || skipFolded && player.Folded || skipAllIn && player.Credits == 0); // Keep going if this player is out of the game or (if appropriate) has folded
            return player;
        }
        
        public IEnumerable<Player> RemoveBrokePlayers()
        {
            var removedPlayers = new List<Player>();
            foreach (var player in Players)
                if (!player.IsHuman && player.Credits == 0)
                {
                    removedPlayers.Add(player);
                    player.SetNotPlaying();
                }
            
            return removedPlayers;
        }
        
        public IEnumerable<Player> UseBuyIns(int buyInCredits)
        {
            var affectedPlayers = Players.Where(x => !x.IsHuman && x.Credits == 0).ToList();
            foreach (var player in affectedPlayers) player.Credits = buyInCredits;
            return affectedPlayers;
        }
        
        public void RestoreDeck(int fixedRiverCount = 0)
        {
            // Take cards back from players and river
            foreach (var player in Players)
            {
                foreach (var card in player.Hand) Deck.Add(card);
                player.Discard();
            }
            for (int i = River.Count - 1; i >= fixedRiverCount; i--) // For simulation, some river cards may be fixed
            {
                Deck.Add(River[i]);
                River.RemoveAt(i);
            }
        }
        
        public void StartHand(out IEnumerable<Card> shuffleOrder, out Player dealer, out Player smallBlind, out Player bigBlind)
        {
            Debug.Assert(!_simulated, "Attempting to call StartHand() with simulated Table");
            
            // Shuffle deck
            Deck.ResetCards();
            Deck.Shuffle();
            shuffleOrder = Deck.ToList(); // Store deck state prior to drawing as "shuffle order"
            
            // Pick dealer
            _dealer = dealer = _dealer == null || !_players.Contains(_dealer) ? _players[0] : NextPlayer(_dealer);
            var alwaysDealer = _players.FirstOrDefault(x => !x.NotPlaying && x.Trait?.GetType() == typeof(Trait_Croop));
            if (!TournamentRules && alwaysDealer != null) _dealer = dealer = alwaysDealer;
            smallBlind = NextPlayer(dealer);
            bigBlind = NextPlayer(smallBlind);
            
            // Draw cards (starting with small blind)
            var toDraw = smallBlind;
            do {
                toDraw.Draw(Deck);
                toDraw = NextPlayer(toDraw);
            } while (toDraw != smallBlind);
            
            // Set player state
            foreach (var player in Players) player.StartHand();
            smallBlind.PlaceBet(this, SmallBlind, isBlind: true);
            bigBlind.PlaceBet(this, BigBlind, isBlind: true);
        }
        
        public void EndHand()
        {
            GetPlacements(this, players: Players.Where(x => !x.NotPlaying), out var placements);
            
            // Assign winnings
            // Main pot always goes to first place
            var mainPotWinners = placements.Where(x => x.placement == 1).ToList();
            int winnings = _mainPot / mainPotWinners.Count;
            foreach (var winner in mainPotWinners)
            {
                winner.player.HandResult = mainPotWinners.Count > 1 ? Player.Result.Tied : Player.Result.Won;
                winner.player.Credits += winnings;
            }
            // Side pots have per-pot evaluation of winnings
            foreach (var sidePot in _sidePots)
            {
                var bestPlacement = placements.Where(x => sidePot.participants.Contains(x.player)).Select(x => x.placement).Min();
                var sidePotWinners = placements.Where(x => sidePot.participants.Contains(x.player)).Where(x => x.placement == bestPlacement).ToList();
                int sidePotWinnings = sidePot.value / sidePotWinners.Count;
                foreach (var winner in sidePotWinners)
                {
                    if (winner.player.HandResult == default) winner.player.HandResult = Player.Result.SidePot;
                    winner.player.Credits += sidePotWinnings;
                }
            }
            
            _mainPot = 0;
            _sidePots.Clear();
        }
        
        public void StartRound(Round roundType)
        {
            MinRaise = BigBlind;
            
            int riverDraw = roundType switch
            {
                Round.Preflop => 0,
                Round.Flop => 3,
                Round.Turn => 1,
                Round.River => 1,
            };
            Deck.Deal(riverDraw, River);
            
            if (roundType == Round.Flop)
            {
                // Call OnFlop on traits
                foreach (var player in _players.Where(x => x.Trait != null))
                    player.Trait.OnFlop(player, this);
            }
        }
        
        public void CheckForSidePots(out Player betChangeTarget)
        {
            betChangeTarget = null;
            
            var maxActiveBet = Players.Where(x => !x.Folded).Max(x => x.ActiveBet);
            var unevenBets = Players.Any(x => !x.Folded && x.ActiveBet != maxActiveBet);
            if (unevenBets)
            {
                // Uneven bets means at least one player is all-in
                var activePlayers = Players.Where(x => !x.Folded).OrderBy(x => x.ActiveBet).ToList();
                if (activePlayers.Count == 2)
                {
                    // In the case of 2 players, simply return the excess funds
                    var allInPlayer = activePlayers.First();
                    var otherPlayer = activePlayers.Last();
                    var diff = maxActiveBet - allInPlayer.ActiveBet;
                    otherPlayer.Credits += diff;
                    otherPlayer.ActiveBet -= diff;
                    _mainPot -= diff;
                    
                    betChangeTarget = otherPlayer;
                }
                else
                {
                    // Else, assign side pots
                    // Algorithmic goal: create side pots for each stratum of bettors
                    //   - The highest bettor(s) gets a pot for all the extra betting defined as maxBet - nextLowerBet
                    //   - Then the group including the next lower bettor(s) gets a pot for nextLowerBet - nextEvenLowerBet, etc.
                    for (int i = activePlayers.Count - 1; i > 0; i--)
                    {
                        var perPlayerDiff = activePlayers[i].ActiveBet - activePlayers[i - 1].ActiveBet;
                        if (perPlayerDiff != 0)
                        {
                            int participants = activePlayers.Count - i;
                            int sidePotValue = perPlayerDiff * participants;
                            _sidePots.Add((sidePotValue, activePlayers.GetRange(i, participants)));
                            _mainPot -= sidePotValue;
                        }
                    }
                }
            }
        }
        
        public void EndRound()
        {
            foreach (var player in _players) player.EndRound();
        }
        
        /// <returns>Returns 1/winnerCount for a win, 0 for a loss</returns>
        public float SimulateHand(Player fixedPlayer)
        {
            Debug.Assert(_simulated, "Attempting to call SimulateHand() with non-simulated Table");
            
            Deck.Shuffle();
            
            // Deal out hands and river
            foreach (var player in _players) player.Draw(Deck);
            Deck.Deal(5 - River.Count, River);
            
            GetPlacements(this, Players.Concat(fixedPlayer.ToEnumerable()), out var placements, markHandValues: false);
            
            if (placements.Any(x => x.player == fixedPlayer && x.placement == 1)) return 1f / placements.Count(x => x.placement == 1);
            else return 0f;
        }
        
        public static void GetPlacements(Table table, IEnumerable<Player> players, out List<(Player player, int placement)> placements, bool markHandValues = true)
        {
            // Calculate player hand values
            var candidates = players.Select(x =>
            {
                var handValue = HandEvaluator.GetBestHand(table, x, out var effectiveHand, out var value);
                if (markHandValues) x.HandValue = handValue;
                return (player: x, hand: handValue, effectiveHand: effectiveHand, value: value);
            }).Where(x => !x.player.Folded).ToList(); // Intentionally storing hand values even for folded players
            
            // Sort hand values ordinally (type first, kickers last, etc.)
            candidates.Sort((lhs, rhs) =>
            {
                var count = Mathf.Min(lhs.value.Count, rhs.value.Count);
                for (int i = 0; i < count; i++)
                    if (lhs.value[i] != rhs.value[i]) 
                        return rhs.value[i] - lhs.value[i]; // Descending value
                return 0; // No differences found
            }); //Debug.Log(handValues.ToVerboseString(x => $"{x.player.Name} {x.hand} {x.effectiveHand.ToVerboseString()}"));
            
            // Assign placements to each player
            placements = new();
            int placement = 1;
            List<int> placementValue = null;
            foreach (var element in candidates)
            {
                if (placementValue != null && !element.value.SequenceEqual(placementValue)) placement++;
                placements.Add((element.player, placement));
                placementValue = element.value;
            }
        }
    }
}