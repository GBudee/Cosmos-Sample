using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utilities;

namespace PokerMode
{
    public static class HandEvaluator
    {
        public enum HandType { HighCard, Pair, TwoPair, ThreeOfAKind, Straight, Flush, FullHouse, FourOfAKind, StraightFlush, FiveOfAKind }
        
        public static string PrettyPrint(this HandType value)
        {
            string article = value switch
            {
                HandType.Pair or HandType.Straight or HandType.Flush or HandType.StraightFlush => "a ",
                _ => ""
            };
            return article + value.ToString().AddSpacesToCamelCase();
        }
        
        public static HandType GetBestHand(Table table, Player player) => GetBestHand(table, player, out var unused1, out var unused2);
        
        public static HandType GetBestHand(Table table, Player player, out List<Card> effectiveHand, out List<int> value)
        {
            var availableCards = player.Hand.Concat(table.River).ToList();
            effectiveHand = new();
            value = new();
            SortByRank(availableCards);
            GetBestHand(availableCards, ref effectiveHand, ref value);
            if (player.Trait != null) player.Trait.CustomBestHand(availableCards, ref effectiveHand, ref value);
            return (HandType) value.First();
        }
        
        private static void GetBestHand(List<Card> sortedCards, ref List<Card> effectiveHand, ref List<int> value)
        {
            Debug.Assert(sortedCards.Count > 0, "GetBestHand() called with an empty hand"); 
            
            if (GetFiveOfAKind(sortedCards, ref effectiveHand, ref value)) return;
            if (GetStraightFlush(sortedCards, ref effectiveHand, ref value)) return;
            if (GetFourOfAKind(sortedCards, ref effectiveHand, ref value)) return;
            if (GetFullHouse(sortedCards, ref effectiveHand, ref value)) return;
            if (GetFlush(sortedCards, ref effectiveHand, ref value)) return;
            if (GetStraight(sortedCards, ref effectiveHand, ref value)) return;
            if (GetThreeOfAKind(sortedCards, ref effectiveHand, ref value)) return;
            if (GetTwoPair(sortedCards, ref effectiveHand, ref value)) return;
            if (GetPair(sortedCards, ref effectiveHand, ref value)) return;
            GetHighCard(sortedCards, ref effectiveHand, ref value);
        }
        
        // *** HAND TYPE EVALUATORS ***
        private static bool GetFiveOfAKind(List<Card> sortedCards, ref List<Card> effectiveHand, ref List<int> value)
        {
            return Get_N_OfAKind(HandType.FiveOfAKind, 5, sortedCards, ref effectiveHand, ref value);
        }
        
        private static bool GetStraightFlush(List<Card> sortedCards, ref List<Card> effectiveHand, ref List<int> value)
        {
            if (sortedCards.Count < 5) return false;
            
            // Find straight flush
            bool acePresent = sortedCards[0].Rank == 14;
            var minStraightLength = acePresent ? 4 : 5;
            for (int topIndex = 0; topIndex <= sortedCards.Count - minStraightLength; topIndex++)
            {
                var prevCard = sortedCards[topIndex]; // Add possible top card
                effectiveHand.Add(prevCard);
                
                bool aceBottom = prevCard.Rank == 5 && acePresent; // Straight requires ace bottom
                var straightLength = aceBottom ? 4 : 5;
                for (int i = topIndex + 1; i < sortedCards.Count; i++) // Try to form a straight flush
                {
                    if (effectiveHand.Count + sortedCards.Count - i < straightLength) break; // Not enough cards left to form a straight
                    if (sortedCards[i].Rank == prevCard.Rank - 1 && sortedCards[i].Suit == prevCard.Suit) // Straight flush test
                    {
                        effectiveHand.Add(sortedCards[i]);
                        if (effectiveHand.Count == straightLength)
                        {
                            if (aceBottom) effectiveHand.Add(sortedCards[0]);
                            break;
                        }
                        else prevCard = sortedCards[i];
                    }
                }
                if (effectiveHand.Count == 5) break;
                else effectiveHand.Clear();
            }
            
            if (effectiveHand.Count == 5)
            {
                // Assign hand value
                value.Add((int) HandType.StraightFlush);
                value.Add(effectiveHand[0].Rank);
                return true;
            }
            else return false;
        }
        
        private static bool GetFourOfAKind(List<Card> sortedCards, ref List<Card> effectiveHand, ref List<int> value)
        {
            return Get_N_OfAKind(HandType.FourOfAKind, 4, sortedCards, ref effectiveHand, ref value);
        }
        
        private static bool GetFullHouse(List<Card> sortedCards, ref List<Card> effectiveHand, ref List<int> value)
        {
            // Find 3 of a kind
            if (!Get_N_OfAKind(3, sortedCards, ref effectiveHand)) return false;
            
            // Then, find an additional pair
            var remainingCards = sortedCards.Except(effectiveHand).ToList();
            var pairHand = new List<Card>();
            if (Get_N_OfAKind(2, remainingCards, ref pairHand))
            {
                // Assign hand value
                value.Add((int) HandType.FullHouse);
                value.Add(effectiveHand[0].Rank);
                value.Add(pairHand[0].Rank);
                effectiveHand.AddRange(pairHand);
                return true;
            }
            effectiveHand.Clear();
            return false;
        }

        public static bool GetFlush(List<Card> sortedCards, ref List<Card> effectiveHand, ref List<int> value, int count = 5)
        {
            if (sortedCards.Count < count) return false;
            
            // Flush detection
            int spades = 0, diamonds = 0, clubs = 0, hearts = 0;
            foreach (var card in sortedCards)
            {
                if (card.Suit == Suit.Rockets) spades++;
                else if (card.Suit == Suit.Stars) diamonds++;
                else if (card.Suit == Suit.Moons) clubs++;
                else hearts++;
            }
            var flushSuit = spades >= count ? Suit.Rockets : diamonds >= count ? Suit.Stars : clubs >= count ? Suit.Moons : hearts >= count ? Suit.Planets : (Suit?)null;
            
            if (flushSuit != null) // Flush exists, construct hand
            {
                foreach (var card in sortedCards)
                {
                    if (card.Suit == flushSuit) effectiveHand.Add(card);
                    if (effectiveHand.Count == count) break;
                }
                
                // Assign hand value
                value.Add((int) HandType.Flush);
                foreach (var card in effectiveHand) value.Add(card.Rank); // Hand is already sorted, so this is in order
                return true;
            }
            return false;
        }

        public static bool GetStraight(List<Card> sortedCards, ref List<Card> effectiveHand, ref List<int> value, int count = 5)
        {
            if (sortedCards.Count < count) return false;
            
            // Find straight, starting at each possible top card
            bool acePresent = sortedCards[0].Rank == 14;
            var minStraightLength = acePresent ? count - 1 : count;
            for (int topIndex = 0; topIndex <= sortedCards.Count - minStraightLength; topIndex++)
            {
                var prevCard = sortedCards[topIndex]; // Add possible top card
                effectiveHand.Add(prevCard);
                
                bool aceBottom = prevCard.Rank == count && acePresent; // Straight requires ace bottom
                var straightLength = aceBottom ? count - 1 : count;
                for (int i = topIndex + 1; i < sortedCards.Count; i++) // Try to form a straight
                {
                    if (effectiveHand.Count + sortedCards.Count - i < straightLength) break; // Not enough cards left to form a straight
                    if (sortedCards[i].Rank == prevCard.Rank - 1) // Straight test
                    {
                        effectiveHand.Add(sortedCards[i]);
                        if (effectiveHand.Count == straightLength)
                        {
                            if (aceBottom) effectiveHand.Add(sortedCards[0]);
                            break;
                        }
                        else prevCard = sortedCards[i];
                    }
                }
                if (effectiveHand.Count == count) break;
                else effectiveHand.Clear();
            }
            
            if (effectiveHand.Count == count)
            {
                // Assign hand value
                value.Add((int) HandType.Straight);
                value.Add(effectiveHand[0].Rank);
                return true;
            }
            else return false;
        }
        
        private static bool GetThreeOfAKind(List<Card> sortedCards, ref List<Card> effectiveHand, ref List<int> value)
        {
            return Get_N_OfAKind(HandType.ThreeOfAKind, 3, sortedCards, ref effectiveHand, ref value);
        }

        private static bool GetTwoPair(List<Card> sortedCards, ref List<Card> effectiveHand, ref List<int> value)
        {
            // Find a pair
            if (!Get_N_OfAKind(2, sortedCards, ref effectiveHand)) return false;
            
            // Then, find an additional pair
            var remainingCards = sortedCards.Except(effectiveHand).ToList();
            var pairHand = new List<Card>();
            if (Get_N_OfAKind(2, remainingCards, ref pairHand))
            {
                // Assign hand value
                value.Add((int) HandType.TwoPair);
                value.Add(effectiveHand[0].Rank);
                value.Add(pairHand[0].Rank);
                effectiveHand.AddRange(pairHand);
                AddKickers(sortedCards.Except(effectiveHand), ref effectiveHand, ref value);
                return true;
            }
            effectiveHand.Clear();
            return false;
        }
        
        private static bool GetPair(List<Card> sortedCards, ref List<Card> effectiveHand, ref List<int> value)
        {
            return Get_N_OfAKind(HandType.Pair, 2, sortedCards, ref effectiveHand, ref value);
        }
        
        private static void GetHighCard(List<Card> sortedCards, ref List<Card> effectiveHand, ref List<int> value)
        {
            value.Add((int)HandType.HighCard);
            AddKickers(sortedCards, ref effectiveHand, ref value);
        }
        
        // *** HELPERS ***
        private static bool Get_N_OfAKind(HandType handType, int n, List<Card> sortedCards, ref List<Card> effectiveHand, ref List<int> value)
        {
            if (Get_N_OfAKind(n, sortedCards, ref effectiveHand))
            {
                // Assign hand value
                value.Add((int) handType);
                value.Add(effectiveHand[0].Rank);
                AddKickers(sortedCards.Except(effectiveHand), ref effectiveHand, ref value);
                return true;
            }
            else return false;
        }
        
        private static bool Get_N_OfAKind(int n, List<Card> sortedCards, ref List<Card> effectiveHand)
        {
            if (sortedCards.Count < n) return false;
            
            for (int topIndex = 0; topIndex <= sortedCards.Count - n; topIndex++)
            {
                var prevCard = sortedCards[topIndex];
                effectiveHand.Add(prevCard);
                for (int i = 1; i < n; i++) // Try to form N of a kind
                {
                    if (sortedCards[topIndex + i].Rank == prevCard.Rank) effectiveHand.Add(sortedCards[topIndex + i]);
                    else break;
                }
                if (effectiveHand.Count == n) break;
                else
                {
                    topIndex += effectiveHand.Count - 1; // Skip matching ranks
                    effectiveHand.Clear();
                }
            }
            return effectiveHand.Count == n;
        }
        
        private static void AddKickers(IEnumerable<Card> remainingSortedCards, ref List<Card> effectiveHand, ref List<int> value)
        {
            foreach (var card in remainingSortedCards)
            {
                if (effectiveHand.Count == 5) break;
                effectiveHand.Add(card);
                value.Add(card.Rank);
            }
        }
        
        private static void SortByRank(List<Card> cards)
        {
            cards.Sort((lhs, rhs) =>
            {
                var rankDiff = rhs.Rank - lhs.Rank; // Descending rank
                if (rankDiff == 0) return (int) lhs.Suit - (int) rhs.Suit; // Then ascending suit
                return rankDiff;
            });
        }
    }
}