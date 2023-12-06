using System;
using System.Linq;
using PokerMode.Dialogue;
using UnityEngine;
using Utilities;
using static PokerMode.ExpressionVisuals;
using Random = UnityEngine.Random;

namespace PokerMode.Visuals.Reactions
{
    public class ReactionController : MonoBehaviour
    {
        private const float HUGE_WIN = 30f;
        private const float LARGE_WIN = 10f;
        private const float MEDIUM_WIN = 2f;
        private const float SMALL_WIN = 1f;
        
        private const float LARGE_POT = 20f;
        
        [Header("Character References")] 
        [SerializeField] private ExpressionVisuals _ExpressionVisuals;
        [Header("Generic References")] 
        [SerializeField] private DialogueAnchor _DialogueAnchor;
        
        public DialogueAnchor DialogueAnchor => _DialogueAnchor;
        
        private static DialogueLookup _dialogueLookup;
        private string _archetype;
        
        private void Awake()
        {
            if (_dialogueLookup == null)
            {
                var englishDialogue = Resources.Load<TextAsset>("Dialogue/EnglishDialogue");
                _dialogueLookup = JsonUtility.FromJson<DialogueLookup>(englishDialogue.text);
            }
        }
        
        public void Initialize(string archetype) => _archetype = archetype;
        
        public void Clear()
        {
            _DialogueAnchor?.HideDialogue();
            _ExpressionVisuals?.MarkDisengaged(false);
            _ExpressionVisuals?.ShowExpression(Expression.Neutral);
        }
        
        public void OnHumanJoined(bool showDialog, bool tournamentRules)
        {
            if (showDialog) _DialogueAnchor?.ShowDialogue(tournamentRules ? "So glad you\ncould make it" : "Welcome! Want to\nplay a hand?");
            _ExpressionVisuals?.ShowExpression(Expression.Happy, duration: Random.Range(1.8f, 2.4f), delay: .5f);
        }
        
        public void OnBuyIn()
        {
            _DialogueAnchor?.ShowDialogue("I'll buy back in", duration: 1f);
        }

        public void OnCleanedOut()
        {
            _DialogueAnchor?.ShowDialogue("I'm all out!\nGood luck.", duration: 2f);
        }
        
        public void OnObjectiveWin(string objective)
        {
            _DialogueAnchor?.ShowDialogue($"I'm clean out, well\nplayed. The {objective} is\nall yours");
        }
        
        public void OnDrawCards(Table table, float estimatedWinrate)
        {
            var normalizedWinrate = estimatedWinrate * table.Players.Count(x => !x.NotPlaying);
            
            if (_ExpressionVisuals == null) return;
            _ExpressionVisuals.MarkDisengaged(false);
            if (normalizedWinrate > 1.6f) _ExpressionVisuals.ExpressionToShifty(Expression.Happy);
            else if (normalizedWinrate > 1.2f) _ExpressionVisuals.ExpressionToShifty(Expression.Happy, probability: .5f);
            else if (normalizedWinrate < .5f) _ExpressionVisuals.ExpressionToShifty(Expression.Sad);
            else if (normalizedWinrate < .8f) _ExpressionVisuals.ExpressionToShifty(Expression.Sad, probability: .5f);
            else _ExpressionVisuals.ShowExpression(Expression.Neutral);
        }
        
        public void OnTakeAction(Table table, Player target, int callValue, int bet, bool fold)
        {
            const float CHATTER_PROBABILITY = .35f;
            const float DURATION = 1f;
            
            if (fold)
            {
                if (table.Pot > LARGE_POT * table.BigBlind) ShowDialogue("LargeFold", DURATION);
                else ShowDialogue("SmallFold", DURATION, CHATTER_PROBABILITY);
                _ExpressionVisuals?.MarkDisengaged(true);
                _ExpressionVisuals?.ShowExpression(Expression.Neutral);
            }
            else
            {
                if (bet == 0) ShowDialogue("Check", DURATION, CHATTER_PROBABILITY);
                else if (callValue == bet) ShowDialogue("Call", DURATION, CHATTER_PROBABILITY);
                else if (target.Credits > 0) ShowDialogue("Raise", DURATION);
                else ShowDialogue("AllIn", DURATION);
            }
        }
        
        public void OnHandEnd(Table table, bool showdown, Player target, Player winner)
        {
            if (target.IsHuman && !target.Folded)
            {
                Service.AudioController.Play(target.HandResult is Player.Result.Tied or Player.Result.Won ? "PlayerWin" : "PlayerLoss");
            }
            
            if (_ExpressionVisuals == null) return;
            if (!target.NotPlaying)
            {
                if (target.HandResult is Player.Result.Tied or Player.Result.SidePot) return;
                
                var targetCreditDelta = target.Credits - target.PrevCredits;
                var winnerCreditDelta = winner.Credits - winner.PrevCredits;
                var targetHandValue = target.HandValue.Value;
                var winnerHandValue = winner.HandValue.Value;
                if (showdown)
                {
                    if (target.HandResult == Player.Result.Won) Win(winnerCreditDelta, table.BigBlind);
                    else if (!target.Folded) LoseShowdown(-targetCreditDelta, table.BigBlind, targetHandValue, winnerHandValue);
                    else if (targetHandValue > winnerHandValue) BadFold();
                    else if (targetHandValue < winnerHandValue) GoodFold(winnerCreditDelta, table.BigBlind);
                }
                else
                {
                    if (target.HandResult == Player.Result.Won)
                    {
                        if (target.EstimatedWinrate < .5f && targetHandValue < HandEvaluator.HandType.TwoPair && winnerCreditDelta > MEDIUM_WIN) 
                            WinFromBluffing(winnerCreditDelta);
                        else Win(winnerCreditDelta, table.BigBlind);
                    }
                    else if (targetHandValue > winnerHandValue) BadFold();
                    else FoldedObserver(table, target, winner, .5f);
                }
            }
            else FoldedObserver(table, target, winner, .5f);
        }
        
        private void FoldedObserver(Table table, Player target, Player toObserve, float? probability = null)
        {
            if (toObserve.IsHuman) return;
            
            bool foundSelf = false;
            bool lookRight = false; // "Right" is in viewer-space, as are the "shifty" expressions
            foreach (var player in table.Players)
            {
                if (player == target) foundSelf = true;
                if (player == toObserve) lookRight = foundSelf;
            }
            _ExpressionVisuals.ShowExpression(lookRight ? Expression.Shifty_Right : Expression.Shifty_Left, probability: probability);
        }
        
        private void Win(int winValue, int bigBlind)
        {
            if (winValue > HUGE_WIN * bigBlind) ShowDialogue("HugeWin");
            else if (winValue > LARGE_WIN * bigBlind) ShowDialogue("LargeWin");
            else if (winValue > MEDIUM_WIN * bigBlind) ShowDialogue("MediumWin");
            else ShowDialogue("SmallWin", replacementValues: ("(WINVALUE)", winValue.ToString()));
            
            if (winValue > SMALL_WIN * bigBlind) _ExpressionVisuals.ShowExpression(Expression.Happy);
        }
        
        private void LoseShowdown(int loseValue, int bigBlind, HandEvaluator.HandType targetHandValue, HandEvaluator.HandType winningHandValue)
        {
            if (loseValue < -LARGE_WIN * bigBlind) ShowDialogue("LargeLoss", replacementValues: ("(WINHAND)", winningHandValue.PrettyPrint()));
            else if (loseValue < -MEDIUM_WIN * bigBlind) ShowDialogue("MediumLoss");
            else ShowDialogue("SmallLoss", probability: .5f);
            
            if (loseValue < -SMALL_WIN * bigBlind) _ExpressionVisuals.ShowExpression(Expression.Sad);
        }
        
        private void WinFromBluffing(int winValue)
        {
            ShowDialogue("WinFromBluff");
            _ExpressionVisuals.ShowExpression(Expression.Happy);
        }
        
        private void BadFold()
        {
            ShowDialogue("BadFold", probability: .35f);
            _ExpressionVisuals.ShowExpression(Expression.Sad);
        }
        
        private void GoodFold(int winValue, int bigBlind)
        {
            if (winValue > MEDIUM_WIN * bigBlind) ShowDialogue("GoodFold", probability: .4f);
        }
        
        private void ShowDialogue(string actionName, float? duration = null, float? probability = null, params (string id, string value)[] replacementValues)
        {
            _DialogueAnchor?.ShowDialogue(_dialogueLookup.GetDialogue(actionName, _archetype, replacementValues), duration, probability);
        }
    }
}