using System.Collections.Generic;
using PokerMode.Traits;
using UnityEngine;
using static PokerMode.AIEvaluator;

namespace PokerMode
{
    public class Player
    {
        public const int HAND_SIZE = 2;
        
        // Static
        public PlayerVisuals Visuals { get; set; }
        public string Name { get; }
        public bool IsHuman { get; }
        public Trait Trait { get; }
        
        // Poker State
        public IEnumerable<Card> Hand => _hand;
        public int Credits { get; set; }
        public bool Folded => _folded || _notPlaying;
        public bool NotPlaying => _notPlaying;
        
        // Per-Hand
        public HandEvaluator.HandType? HandValue { get; set; } // Only updated at end of hand when calculating winner
        public enum Result { None, Won, Tied, SidePot }
        public Result HandResult { get; set; }
        public int PrevCredits { get; private set; } // Credits from start of hand
        
        // Per-Round
        public int ActiveBet { get; set; }
        public bool Acted { get; private set; }
        public float EstimatedWinrate { get; set; }
        public bool CHEAT_MustRaise { get; set; }
        
        private List<Card> _hand = new();
        private bool _folded;
        private bool _notPlaying;
        
        public Player(PlayerVisuals visuals, int credits)
        {
            Visuals = visuals;
            Name = visuals.Name;
            IsHuman = visuals.IsHuman;
            Credits = credits;
            if (!string.IsNullOrEmpty(visuals.Trait)) Trait = Trait.CreateInstance(visuals.Trait);
        }
        
        public Player(Player toSimulate)
        {
            Name = toSimulate.Name;
            IsHuman = toSimulate.IsHuman;
        }
        
        // *** CARDS ***
        public void Draw(Deck deck)
        {
            deck.Deal(HAND_SIZE, _hand);
            _folded = false;
        }
        public void Discard() => _hand.Clear();
        public void SetCustomHand(IEnumerable<Card> customHand)
        {
            foreach (var card in customHand) _hand.Add(card);
        }
        
        // *** LIFECYCLE ***
        public void StartHand()
        {
            HandValue = null;
            HandResult = default;
            PrevCredits = Credits;
        }
        
        public void EndRound()
        {
            ActiveBet = 0;
            Acted = false;
            CHEAT_MustRaise = false;
        }
        
        // *** ACTIONS ***
        public int PlaceBet(Table table, int bet, bool isBlind = false)
        {
            int effectiveBet = Mathf.Min(bet, Credits);
            Credits -= effectiveBet;
            ActiveBet += effectiveBet;
            table.AddToPot(effectiveBet);
            
            if (!isBlind) Acted = true;
            return effectiveBet;
        }
        public void Check() => Acted = true;
        public void Fold() => _folded = true;
        public void SetNotPlaying() => _notPlaying = true;
    }
}