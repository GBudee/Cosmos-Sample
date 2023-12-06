using System.Collections.Generic;
using UnityEngine;
using Utilities;

namespace PokerMode.Cheats
{
    public class Cheat_RiverRedraw : CheatCard
    {
        public override TargetingType Targeting => TargetingType.Self;
        public override BackgroundType Background => BackgroundType.Yellow;
        public override string Name => "Board Again";
        public override string Icon => "river_redraw";
        public override string Description => $"Redraw each card currently on the board";
        public override int Cost => 1;
        public override bool Allow(Round round) => round != Round.Preflop;

        public override void Apply(Table table, Card unused)
        {
            // Draw replacement cards
            var drawnCards = new List<Card>();
            table.Deck.Shuffle();
            table.Deck.Deal(table.River.Count, drawnCards, keepCards: true);
            
            // Swap card states from draw w/ river card states
            drawnCards.ZipDo(table.River, (drawnCard, riverCard) =>
            {
                var temp = (drawnCard.Rank, drawnCard.Suit);
                drawnCard.Rank = riverCard.Rank;
                drawnCard.Suit = riverCard.Suit;
                drawnCard.Visuals.Initialize(drawnCard.Rank, drawnCard.Suit);
                
                riverCard.Rank = temp.Rank;
                riverCard.Suit = temp.Suit;
                riverCard.Visuals.Initialize(riverCard.Rank, riverCard.Suit, affectedByCheat: true);
            });
        }
    }
}