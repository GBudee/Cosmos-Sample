using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Utilities;

namespace PokerMode.Traits
{
    [TraitDescription("Second Chance", "This player may secretly redraw after the flop if they don't like their hand.")]
    public class Trait_SecondChance : Trait
    {
        public override void OnFlop(Player player, Table table)
        {
            var bestHand = HandEvaluator.GetBestHand(table, player);
            if (bestHand < HandEvaluator.HandType.Pair)
            {
                // Discard hand
                List<Card> drawnCards = new();
                table.Deck.Deal(2, drawnCards, keepCards: true);
                
                // Swap card states from draw w/ hand card states
                drawnCards.ZipDo(player.Hand, (drawnCard, handCard) =>
                {
                    var temp = (drawnCard.Rank, drawnCard.Suit);
                    drawnCard.Rank = handCard.Rank;
                    drawnCard.Suit = handCard.Suit;
                    drawnCard.Visuals.Initialize(drawnCard.Rank, drawnCard.Suit);
                    
                    handCard.Rank = temp.Rank;
                    handCard.Suit = temp.Suit;
                    handCard.Visuals.Initialize(handCard.Rank, handCard.Suit);
                });
                
                Debug.Log($"Second Chance cheater redrew");
            }
        }
    }
}