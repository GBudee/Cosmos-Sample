using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Utilities;

namespace PokerMode.Traits
{
    [TraitDescription("Short Line", "This player only needs 4 cards to form a straight.")]
    public class Trait_ShortLine : Trait
    {
        public override void CustomBestHand(List<Card> sortedCards, ref List<Card> effectiveHand, ref List<int> value)
        {
            if (sortedCards.Count < 4) return;
            if (value.First() >= (int) HandEvaluator.HandType.Straight) return;
            
            var tempEffectiveHand = new List<Card>();
            var tempValue = new List<int>();
            if (HandEvaluator.GetStraight(sortedCards, ref tempEffectiveHand, ref tempValue, count: 4))
            {
                effectiveHand.Clear();
                value.Clear();
                foreach (var card in tempEffectiveHand) effectiveHand.Add(card);
                foreach (var element in tempValue) value.Add(element);
            }
        }
    }
}