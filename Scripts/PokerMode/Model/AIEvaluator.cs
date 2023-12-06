using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using PokerMode.Traits;
using UnityEngine;
using Utilities;
using Random = UnityEngine.Random;

namespace PokerMode
{
    public static class AIEvaluator
    {
        const int ROUND = 5;
        
        public enum Strategy { Cautious, Balanced, Bluffer }
        private enum ActionName { Fold, Check, Call, Raise }

        public static float EstimateWinrate(Table table, Player player)
        {
            return EstimatedWinrate(table, player);
        }
        
        public static void ChooseAction(Table table, Player player, int callValue, int minRaise, out int bet, out bool fold, bool fullPlay)
        {
            bet = 0;
            fold = false;
            
            // Calculate hand winrate
            bool startingHand = table.River.Count == 0;
            var estimatedWinrate = player.EstimatedWinrate;
            string description = $"{player.Name}'s turn w/ {player.Hand.ToVerboseString(x => x.ToString())}. Estimated winrate: {estimatedWinrate}.";
            
            // Calculate rate of return (if callValue isn't 0)
            bool call;
            float callRoll = -1f;
            if (callValue > 0)
            {
                int effectivePot = table.Pot;
                if (startingHand) effectivePot += 2 * table.BigBlind; // Juice first round call rates
                float potOdds = callValue / (float)(callValue + effectivePot);
                float rateOfReturn = estimatedWinrate / potOdds;
                
                // Using rate of return, evaluate call rate
                if (startingHand && player.Hand.Select(x => x.Rank).Distinct().Count() == 1) call = true; // For pocket pairs, always call starting hand
                else call = RollOnCurve(out callRoll, AICurves.Instance.CallRate_Per_RoR, input: rateOfReturn, table.CallBoost);
                description += $" Pot odds: {potOdds}. Effective return: {rateOfReturn}. Call-rate {AICurves.Instance.CallRate_Per_RoR.Evaluate(rateOfReturn) + table.CallBoost}\n";
            }
            else
            {
                call = true; // Always call if it's free to do so
                description += " No call required.";
            }
            
            ActionName actionName;
            if (!call)
            {
                // Fold
                actionName = ActionName.Fold;
                fold = true;
                
                description += $" Based on aggressiveness rolls of {callRoll}, {player.Name} {actionName.ToString()}s";
            }
            else
            {
                // Roll again to determine call or raise
                var raise = RollOnCurve(out var raiseRoll, AICurves.Instance.RaiseRate_Per_Winrate, input: estimatedWinrate);
                if (!raise)
                {
                    // Call (or check if call value is 0)
                    actionName = callValue > 0 ? ActionName.Call : ActionName.Check;
                    if (callValue > 0) bet = callValue;
                }
                else
                {
                    // Raise
                    actionName = ActionName.Raise;
                    int randomRaise;
                    if (startingHand) randomRaise = RandomRaise(table.BigBlind, lowerMult: 1f, upperMult: 4f);
                    else
                    {
                        var lowerMult = AICurves.Instance.LowestRaise_Per_Winrate.Evaluate(estimatedWinrate);
                        var upperMult = AICurves.Instance.HighestRaise_Per_Winrate.Evaluate(estimatedWinrate);
                        randomRaise = RandomRaise(table.BigBlind, lowerMult, upperMult);
                    }
                    if (callValue + randomRaise < minRaise) bet = minRaise;
                    else bet = callValue + randomRaise;
                }
                
                description += $" Based on aggressiveness rolls of {(callRoll == -1 ? "N/a" : callRoll.ToString())} & {raiseRoll}, {player.Name} {actionName.ToString()}s";
                if (raise) description += $". Raise bounds: {AICurves.Instance.LowestRaise_Per_Winrate.Evaluate(estimatedWinrate)}" +
                                          $" to {AICurves.Instance.HighestRaise_Per_Winrate.Evaluate(estimatedWinrate)}";
            }
            
            if (fullPlay) Debug.Log(description);
        }
        
        private static float EstimatedWinrate(Table table, Player player)
        {
            #if UNITY_EDITOR
            if (Service.GameController.DEBUG_StackDeck) return .5f;
            #endif
            
            // Create simulation instances of the deck, river, and other players
            var simulatedTable = new Table(toSimulate: table, fixedPlayer: player);
            
            // Run a monte carlo simulation of the overall winrate
            const int ITERATIONS = 100;
            var wins = 0f;
            for (int i = 0; i < ITERATIONS; i++)
            {
                // Simulate hand and accumulate win total
                if (player.Trait != null) wins += player.Trait.CustomSimulateHand(simulatedTable, player);
                else wins += simulatedTable.SimulateHand(player);
                simulatedTable.RestoreDeck(table.River.Count);
            }
            return wins / ITERATIONS;
        }
        
        private static bool RollOnCurve(out float roll, AnimationCurve curve, float input, float boost = 0f)
        {
            roll = Random.Range(0f, 1f);
            return 1f - roll < curve.Evaluate(input) + boost;
        }
        
        private static int RandomRaise(float bigBlind, float lowerMult, float upperMult)
        {
            var raiseMult = DOVirtual.EasedValue(lowerMult, upperMult, Random.Range(0f, 1f), Ease.InQuad);
            return Mathf.RoundToInt(bigBlind * raiseMult / ROUND) * ROUND;
        }
        
        private static bool D100(float percentLikelihood) => Random.Range(0f, 1f) <= percentLikelihood;
    }
}