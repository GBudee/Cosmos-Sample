using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Utilities;

namespace PokerMode.Traits
{
    [TraitDescription("Four Eyes", "This player can peek at the human player's hand at the flop.")]
    public class Trait_FourEyes : Trait
    {
        private List<Card> _peekedHand = new();

        public override void OnDraw(Player player)
        {
            _peekedHand.Clear();
        }
        
        public override void OnFlop(Player player, Table table)
        {
            if (table.HumanPlayer != null)
            {
                foreach (var card in table.HumanPlayer.Hand) 
                    _peekedHand.Add(new Card(card.Rank, card.Suit));
            }
        }
        
        public override float CustomSimulateHand(Table table, Player fixedPlayer)
        {
            table.Deck.Shuffle();
            
            // Deal out hands and river
            foreach (var player in table.Players)
            {
                if (player.IsHuman)
                {
                    player.SetCustomHand(_peekedHand); // CUSTOM: Apply HumanPlayer hand
                }
                else player.Draw(table.Deck);
            }
            table.Deck.Deal(5 - table.River.Count, table.River);
            
            Table.GetPlacements(table, table.Players.Concat(fixedPlayer.ToEnumerable()), out var placements, markHandValues: false);
            
            float result = 0f;
            if (placements.Any(x => x.player == fixedPlayer && x.placement == 1)) result = 1f / placements.Count(x => x.placement == 1);
            table.HumanPlayer?.Discard(); // CUSTOM: Discard HumanPlayer hand
            return result;
        }
    }
}