using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cinemachine;
using DG.Tweening;
using MEC;
using NavigationMode;
using UI;
using UnityEngine;
using UnityEngine.Serialization;
using Utilities;
using Random = UnityEngine.Random;

namespace PokerMode
{
    public class TableController : MonoBehaviour, ISaveable
    {
        [SerializeField] private CinemachineVirtualCamera _VirtualCamera;
        [Header("Settings")]
        [SerializeField] private int _BuyIn = 500;
        [SerializeField] private int _SmallBlind = 5;
        [SerializeField] private int _BigBlind = 10;
        [SerializeField] [Range(0, 1)] private float _CallBoost = 0f;
        [SerializeField] private bool _TournamentRules;
        [Header("Game Pieces")]
        [SerializeField] private DeckVisuals _MainDeck;
        [SerializeField] private RiverVisuals _RiverVisuals;
        [SerializeField] private CreditVisuals _PotVisuals;
        [SerializeField] private PlayerVisuals _HumanVisuals;
        [SerializeField] private List<PlayerVisuals> _AIPlayerVisuals;
        [Header("Subsystem")]
        [SerializeField] private HandController _HandController;
        
        public Table Table => _humanTable;
        public HandController HandController => _HandController;
        public int BuyIn => _BuyIn;
        public bool ObjectiveAchieved { get; private set; }

        public int CurrentHumanCash => _humanTable?.Players.FirstOrDefault(x => x.IsHuman)?.Credits ?? 0;
        
        private bool _resetting;
        private Table _autoTable;
        private Table _humanTable;
        
        void OnEnable(){} // Allow enablement
        
        public void Save(int version, BinaryWriter writer, bool changingScenes = false)
        {
            bool hasData = _humanTable != null;
            writer.Write(hasData);
            if (!hasData) return;
            
            var cashOnHand = _humanTable.Players.Where(x => !x.IsHuman).Select(x => x.Credits).ToList();
            
            writer.Write(ObjectiveAchieved);
            writer.WriteList(cashOnHand);
        }
        
        public void Load(int version, BinaryReader reader)
        {
            bool hasData = reader.ReadBoolean();
            if (!hasData) return;
            
            // Read data
            ObjectiveAchieved = reader.ReadBoolean();
            var cashOnHand = new List<int>();
            reader.ReadList(cashOnHand);
            
            // Create table
            _humanTable = new Table(_MainDeck, _AIPlayerVisuals, _PotVisuals, _BuyIn, _SmallBlind, _BigBlind);
            int playerIndex = 0;
            foreach (var player in _humanTable.Players)
            {
                if (playerIndex >= cashOnHand.Count) break;
                player.Credits = cashOnHand[playerIndex];
                playerIndex++;
            }
            _humanTable.AddHumanPlayer(new Player(_HumanVisuals, credits: 0));
        }
        
        public void Join(PokerHud pokerHud, Func<bool> canBuyIn, Action buyIn, Action<int> cashOut, Action<int> gainXP, Action gainGumption, Action<bool> allowCheats, Action wonTournament)
        {
            _VirtualCamera.enabled = enabled = true;
            
            if (_humanTable == null)
            {
                _humanTable = new Table(_MainDeck, _AIPlayerVisuals, _PotVisuals, _BuyIn, _SmallBlind, _BigBlind, _TournamentRules, _CallBoost);
                _humanTable.AddHumanPlayer(new Player(_HumanVisuals, credits: 0));
            }
            
            // Kill existing game loop
            Timing.KillCoroutines(gameObject);
            Timing.RunCoroutine(PlayGame(pokerHud, fullPlay: true, _humanTable, priorTable: _autoTable, canBuyIn, buyIn, cashOut, gainXP, gainGumption, allowCheats, wonTournament), gameObject);
        }
        
        public void Leave()
        {
            _VirtualCamera.enabled = enabled = false;
            
            if (_TournamentRules) return; // No auto-play without humans in the tournament
            
            // No need to kill game, we assume it exited properly
            _autoTable = new Table(_MainDeck, _AIPlayerVisuals, _PotVisuals, 500, 5, 10);
            Timing.RunCoroutine(PlayGame(pokerHud: null, fullPlay: false, table: _autoTable, priorTable: _humanTable), gameObject);
        }
        
#if UNITY_EDITOR
        public void OnValidate()
        {
            if (_VirtualCamera != null) _VirtualCamera.enabled = enabled;
        }
#endif
        
        private IEnumerator<float> PlayGame(PokerHud pokerHud, bool fullPlay, Table table, Table priorTable
            , Func<bool> canBuyIn = null, Action buyIn = null, Action<int> cashOut = null
            , Action<int> gainXP = null, Action gainGumption = null, Action<bool> allowCheats = null, Action wonTournament = null)
        {
            // RESET VISUAL STATE
            foreach (var player in table.Players) player.Visuals.CreditVisuals.Show(player.Credits);
            if (!fullPlay) _HumanVisuals.CreditVisuals.Show(0);
            _PotVisuals.Show(table.Pot);
            if (priorTable != null)
            {
                foreach (var player in table.Players)
                {
                    player.Visuals.ResetState(player.NotPlaying);
                    player.Visuals.KillAnimations();
                }
                
                priorTable.RestoreDeck();
                yield return Timing.WaitUntilDone(RestoreDeckVisuals());
            }
            
            // INIT HUD
            if (fullPlay)
            {
                var allPlayerVisuals = _AIPlayerVisuals.Concat(_HumanVisuals.ToEnumerable());
                pokerHud.Show(allPlayerVisuals, _PotVisuals, _HumanVisuals.HandVisuals, _RiverVisuals);
                
                // Welcome the human
                var dialogPlayer = _AIPlayerVisuals[1];
                foreach (var aiPlayer in _AIPlayerVisuals)
                    aiPlayer.ReactionController.OnHumanJoined(showDialog: aiPlayer == dialogPlayer, _TournamentRules);
            }
            
            // GAME LOOP
            yield return Timing.WaitForOneFrame;
            while (true)
            {
                bool exit = false;
                if (fullPlay)
                {
                    Service.GameController.SaveGame();

                    if (_TournamentRules)
                    {
                        var removedPlayers = table.RemoveBrokePlayers();
                        foreach (var player in removedPlayers)
                        {
                            player.Visuals.ReactionController.OnCleanedOut();
                            yield return Timing.WaitForSeconds(1f);
                            yield return Timing.WaitForOneFrame;
                        }
                    }
                    
                    // Wait for human input
                    bool inputReady = false;
                    var humanPlayer = table.HumanPlayer;
                    bool hasCredits = humanPlayer.Credits > 0;
                    bool canPlay = hasCredits || canBuyIn();
                    if (_TournamentRules && table.Players.Count(x => !x.IsHuman && !x.NotPlaying) < 1)
                    {
                        canPlay = false;
                        wonTournament();
                    }
                    Action localBuyIn = () =>
                    {
                        buyIn();
                        humanPlayer.Credits = _BuyIn;
                        humanPlayer.Visuals.CreditVisuals.Show(humanPlayer.Credits);
                        foreach (var traitPlayer in table.Players.Where(x => !string.IsNullOrEmpty(x.Visuals.Trait)))
                            Service.SimpleTutorial.TriggerBeat($"TRAIT_{traitPlayer.Visuals.Trait.ToUpper()}");
                        Service.AudioController.Play("BuyIn", humanPlayer.Visuals.CreditVisuals.transform.position);
                    };
                    pokerHud.EnterDefaultMode(hasCredits, canPlay, localBuyIn, _BuyIn, e =>
                    {
                        exit = e;
                        inputReady = true;
                    });
                    yield return Timing.WaitUntilTrue(() => inputReady);
                }
                
                // Reset playerVisuals
                foreach (var player in table.Players) player.Visuals.ResetState(player.NotPlaying);
                
                // Human leaves table
                if (exit)
                {
                    // Cash out the human player's credits
                    var humanPlayer = table.HumanPlayer;
                    if (humanPlayer.Credits > 0)
                    {
                        cashOut(humanPlayer.Credits);
                        Service.AudioController.Play("Bid", humanPlayer.Visuals.CreditVisuals.transform.position, randomizer: 3);
                        humanPlayer.Credits = 0;
                        humanPlayer.Visuals.CreditVisuals.Show(0);
                    }
                    
                    // Exit poker mode
                    pokerHud.Hide();
                    Service.GameController.SetMode(GameController.GameMode.Navigation);
                    yield break;
                }
                else if (fullPlay) pokerHud.EnterPlayMode_Waiting();
                
                // AI Players buy-in if needed
                if (!_TournamentRules)
                {
                    var buyInPlayers = table.UseBuyIns(_BuyIn);
                    if (fullPlay)
                        foreach (var player in buyInPlayers)
                        {
                            player.Visuals.ReactionController.OnBuyIn();
                            yield return Timing.WaitForSeconds(1f);
                            Service.AudioController.Play("Bid", player.Visuals.CreditVisuals.transform.position, randomizer: 3);
                            player.Visuals.CreditVisuals.Show(player.Credits);
                        }
                }
                
                // Restore board state (including returning all cards to deck)
                foreach (var player in table.Players.Where(x => !x.IsHuman)) player.Visuals.ExitShowdown_Anim();
                table.RestoreDeck();
                yield return Timing.WaitUntilDone(RestoreDeckVisuals());
                
                // Check if enough players remain for auto-mode
                if (!fullPlay && table.Players.Count(x => !x.NotPlaying) < 2) yield break;
                
                // *** PLAY HAND ***
                yield return Timing.WaitForSeconds(.2f);
                var handHandle = Timing.RunCoroutine(_HandController.PlayHand(table, pokerHud, _PotVisuals, _RiverVisuals, fullPlay, allowCheats), gameObject);
                yield return Timing.WaitUntilDone(handHandle);
                
                // AI Showdown animations
                bool showdown = table.Players.Count(x => !x.Folded) > 1;
                foreach (var player in table.Players.Where(x => !x.Folded))
                {
                    player.Visuals.EnterShowdown_Anim(player.HandResult is Player.Result.Won or Player.Result.Tied, "ShowCards",
                        () => player.Visuals.HandVisuals.Showdown_Anim());
                }
                if (fullPlay) Service.AudioController.Play("Showdown");
                
                // Reveal hands in ui
                yield return Timing.WaitForSeconds(.4f);
                foreach (var player in table.Players.Where(x => !x.IsHuman && !x.Folded)) // TODO: Make handVisuals general so a human one would just ignore this
                {
                    player.Visuals.HandVisuals.Revealed = true;
                    player.Visuals.HandVisuals.HandValue = player.HandValue;
                }
                
                if (showdown) yield return Timing.WaitForSeconds(.6f); // Add anticipation to "who won" for a showdown
                
                // Show winner
                var firstWinner = table.Players.First(x => !x.NotPlaying && x.HandResult is Player.Result.Won or Player.Result.Tied);
                foreach (var player in table.Players.Where(x => !x.NotPlaying))
                {
                    if (player.HandResult != default)
                    {
                        // Gain xp for winnings
                        int winnings = player.Credits - player.PrevCredits;
                        var resultAsAction = player.HandResult switch
                        {
                            Player.Result.Won => PlayerDisplay.Action.Win,
                            Player.Result.Tied => PlayerDisplay.Action.Tie,
                            Player.Result.SidePot => PlayerDisplay.Action.SidePot,
                        };
                        player.Visuals.ShowAction(resultAsAction, winnings);
                        if (player.IsHuman)
                        {
                            if (winnings > 0) gainXP(winnings);
                            else gainXP(Mathf.CeilToInt(Mathf.Abs(player.Credits - player.PrevCredits) * .25f));
                            if (player.HandResult == Player.Result.Won) pokerHud.ShowWinMessage();
                        }
                    }
                    else if (player.IsHuman)
                    {
                        // Gain 1/4 exp for losses
                        gainXP(Mathf.CeilToInt(Mathf.Abs(player.Credits - player.PrevCredits) * .25f));
                    }
                    player.Visuals.CreditVisuals.Show(player.Credits);
                    player.Visuals.ReactionController.OnHandEnd(table, showdown, player, firstWinner);
                }
                
                // Trigger tutorial beats
                if (fullPlay)
                {
                    if (table.HumanPlayer.HandResult != default) Service.SimpleTutorial.TriggerBeat("GRATS_EXP");
                    else Service.SimpleTutorial.TriggerBeat("INTRO_EXP");
                }
                
                // Gain gumption
                if (fullPlay)
                {
                    if (!table.HumanPlayer.Folded) gainGumption();
                }
                _PotVisuals.Show(0);
            }
        }
        
        private IEnumerator<float> RestoreDeckVisuals()
        {
            if (_resetting)
            {
                yield return Timing.WaitUntilFalse(() => _resetting);
                yield break;
            }
            
            _resetting = true;
            
            // Wait for card animations to finish
            yield return Timing.WaitUntilFalse(() => _MainDeck.Animating || _MainDeck.AllCards.Any(x => x.Animating));
            
            // Restore hand cards to deck
            foreach (var player in _AIPlayerVisuals.Concat(_HumanVisuals.ToEnumerable()))
            foreach (var card in player.HandVisuals.Cards.ToList())
            {
                Timing.RunCoroutine(card.HandToDeck_Anim(player.HandVisuals, _MainDeck));
                yield return Timing.WaitForSeconds(.1f);
            }
            // Restore river cards to deck
            foreach (var card in _RiverVisuals.Cards.ToList())
            {
                Timing.RunCoroutine(card.RiverToDeck_Anim(_RiverVisuals, _MainDeck));
                yield return Timing.WaitForSeconds(.1f);
            }
            
            _resetting = false;
        }
    }
}
