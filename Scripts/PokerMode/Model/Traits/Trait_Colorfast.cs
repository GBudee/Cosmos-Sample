using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Utilities;

namespace PokerMode.Traits
{
    [TraitDescription("Colorfast", "This player only needs 4 cards to form a flush.")]
    public class Trait_Colorfast : Trait
    {
        public override void CustomBestHand(List<Card> sortedCards, ref List<Card> effectiveHand, ref List<int> value)
        {
            if (sortedCards.Count < 4) return;
            if (value.First() >= (int) HandEvaluator.HandType.Flush) return;
            
            var tempEffectiveHand = new List<Card>();
            var tempValue = new List<int>();
            if (HandEvaluator.GetFlush(sortedCards, ref tempEffectiveHand, ref tempValue, count: 4))
            {
                effectiveHand.Clear();
                value.Clear();
                foreach (var card in tempEffectiveHand) effectiveHand.Add(card);
                foreach (var element in tempValue) value.Add(element);
            }
        }
    }
}