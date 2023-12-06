using System.Collections.Generic;
using System.Linq;
using MEC;
using PokerMode.Traits;
using UI;
using UnityEngine;

namespace PokerMode
{
    public class RoundController : MonoBehaviour
    {
        [SerializeField] private bool _DEBUG_ControlAI;
        
        public Round CurrentRound { get; private set; }
        
        public IEnumerator<float> PlayRound(Table table, PokerHud pokerHud, CreditVisuals potVisuals, RiverVisuals riverVisuals, Player startingPlayer, Round roundType, bool fullPlay, System.Action<bool> allowCheats)
        {
            // Start round in backend
            int priorRiverCount = table.River.Count;
            table.StartRound(roundType);
            CurrentRound = roundType;
            
            // Show round start message
            if (roundType != Round.Preflop)
            {
                yield return Timing.WaitForSeconds(1f);
                foreach (var player in table.Players.Where(x => !x.Folded)) player.Visuals.ClearAction();
            }
            
            // Animate draw to river
            for (int i = priorRiverCount; i < table.River.Count; i++)
            {
                Timing.RunCoroutine(table.River[i].Visuals.DeckToRiver_Anim(table.Deck.Visuals, riverVisuals, i));
                yield return Timing.WaitForSeconds(.15f);
            }
            yield return Timing.WaitForSeconds(.1f);
            
            // Evaluate human player hand value
            if (fullPlay)
            {
                var humanPlayer = table.Players.Single(x => x.IsHuman);
                var newBestHand = HandEvaluator.GetBestHand(table, humanPlayer);
                pokerHud.UpdateBestHand(newBestHand);
            }
            
            // If all but one remaining players are all-in, skip round
            if (table.Players.Count(x => !x.Folded && x.Credits != 0) < 2)
            {
                table.EndRound();
                yield break;
            }
            
            // Loop through turns during round
            var activePlayer = (startingPlayer.Folded || startingPlayer.Credits == 0) ? table.NextPlayer(startingPlayer, skipFolded: true, skipAllIn: true) : startingPlayer;
            while (true)
            {
                // Action desc
                int bet = 0;
                bool fold = false;
                
                // Call requirements
                int maxBet = table.Players.Where(x => !x.Folded).Select(x => x.ActiveBet).Max();
                int callValue = Mathf.Max(0, maxBet - activePlayer.ActiveBet); // Additional bet needed to match the maximum active bet
                bool callReq = callValue > 0;
                int minRaise = callValue + table.MinRaise;
                int maxRaise = activePlayer.Credits;
                const int PREFLOP_RAISE_LIMIT = 4;
                if (roundType == Round.Preflop) maxRaise = Mathf.Min(maxRaise, maxBet + table.BigBlind * PREFLOP_RAISE_LIMIT);
                minRaise = Mathf.Min(minRaise, maxRaise);
                
#if UNITY_EDITOR
                if (activePlayer.IsHuman || _DEBUG_ControlAI)
#else
                if (activePlayer.IsHuman)
#endif
                {
                    var minBet = callReq ? callValue : minRaise;
                    if (!callReq) minRaise = Mathf.Min(minRaise + table.BigBlind, maxRaise);
                    
                    // Enter human turn UI state
                    bool inputReady = false;
                    allowCheats?.Invoke(true);
                    pokerHud.EnterPlayMode_HumanTurn(callReq, activePlayer.CHEAT_MustRaise, minBet, minRaise, maxRaise, inputAction: (i, b) =>
                    {
                        bet = i;
                        fold = b;
                        allowCheats?.Invoke(false);
                        inputReady = true;
                    });
                    activePlayer.CHEAT_MustRaise = false;
                    
                    // Wait for human input
                    yield return Timing.WaitUntilTrue(() => inputReady);
                    pokerHud.EnterPlayMode_Waiting();
                }
                else
                {
                    // Enter ai turn UI state
                    if (fullPlay) activePlayer.Visuals.TurnIndicator?.Activate();
                    
                    if (roundType != Round.Preflop) // Winrate already calculated for starting hands
                        activePlayer.EstimatedWinrate = fullPlay ? AIEvaluator.EstimateWinrate(table, activePlayer) : Random.Range(.2f, .8f);
                    AIEvaluator.ChooseAction(table, activePlayer, callValue, minRaise, out bet, out fold, fullPlay);
                    if (!fullPlay) bet = Mathf.Min(bet, table.BigBlind);
                    bet = Mathf.Min(bet, maxRaise);
                    
                    // Simulate AI "thinking"
                    yield return Timing.WaitForSeconds(.5f);
                    activePlayer.Visuals.ReactionController.OnTakeAction(table, activePlayer, callValue, bet, fold);
                    
                    if (activePlayer.CHEAT_MustRaise && bet == callValue) bet = minRaise;
                    activePlayer.CHEAT_MustRaise = false;
                }
                
                // Take action!
                if (fold)
                {
                    activePlayer.Fold();
                    var localActivePlayer = activePlayer;
                    activePlayer.Visuals.ShowAction(PlayerDisplay.Action.Fold, activePlayer.ActiveBet, "DropCards", () =>
                        {
                            Service.AudioController.Play("Fold", localActivePlayer.Visuals.CreditVisuals.transform.position);
                            if (!localActivePlayer.IsHuman) localActivePlayer.Visuals.HandVisuals.Fold_Anim();
                        });
                }
                else if (bet > 0)
                {
                    PlayerDisplay.Action action;
                    if (bet == callValue) action = PlayerDisplay.Action.Call;
                    else if (callValue > 0)
                    {
                        action = PlayerDisplay.Action.Raise;
                        activePlayer.Trait?.OnRaiseOrBet(activePlayer, table);
                    }
                    else
                    {
                        action = PlayerDisplay.Action.Bet;
                        activePlayer.Trait?.OnRaiseOrBet(activePlayer, table);
                    }
                    var effectiveBet = activePlayer.PlaceBet(table, bet);
                    var effectiveRaise = effectiveBet - maxBet;
                    if (effectiveRaise > table.MinRaise) table.MinRaise = effectiveRaise;
                    activePlayer.Visuals.ShowBet(action, activePlayer.ActiveBet, activePlayer.Credits, potVisuals, table.Pot, gameObject);
                }
                else
                {
                    activePlayer.Check();
                    var localActivePlayer = activePlayer;
                    activePlayer.Visuals.ShowAction(PlayerDisplay.Action.Check, activePlayer.ActiveBet, "Knock"
                        , () => Service.AudioController.Play("Check", localActivePlayer.Visuals.CreditVisuals.transform.position, randomizer: 3));
                }
                
                // Stop "active turn" visuals
                activePlayer.Visuals.TurnIndicator?.Deactivate();
                yield return Timing.WaitForSeconds(.8f);
                
                // Exit loop for default winner
                if (table.Players.Count(x => !x.Folded) < 2) break;
                
                // Exit loop because all remaining players have checked/called
                maxBet = table.Players.Where(x => !x.Folded).Select(x => x.ActiveBet).Max();
                if (table.Players.All(x => x.Acted && x.ActiveBet == maxBet || x.Folded || x.Credits == 0)) break;
                
                // Continue Loop: Get next non-folded-or-all-in player
                activePlayer = table.NextPlayer(activePlayer, skipFolded: true, skipAllIn: true);
            }
            
            // Evaluate side pots
            table.CheckForSidePots(out var betChangeTarget);
            if (betChangeTarget != null)
            {
                betChangeTarget.Visuals.ChangeActionCredits(betChangeTarget.ActiveBet);
                betChangeTarget.Visuals.CreditVisuals.Show(betChangeTarget.Credits);
                yield return Timing.WaitForSeconds(1f); // Give the player time to see and understand the bet being changed
            }
            
            // Reset per-round data (i.e. player.ActiveBet)
            table.EndRound();
        }
    }
}