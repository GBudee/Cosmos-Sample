using UI;
using UnityEngine;

namespace PokerMode.Cheats
{
    public class Cheat_SwapToRiver : CheatCard
    {
        public override TargetingType Targeting => TargetingType.Hand;
        public override BackgroundType Background => BackgroundType.Purple;
        public override string Name => "Random Swap";
        public override string Icon => "swap_to_river";
        public override string Description => "Exchange target hand card with a <i>random</i> card on the board";
        public override int Cost => 1;
        public override bool Allow(Round round) => round != Round.Preflop;
        
        public override void Apply(Table table, Card card)
        {
            var randomRiverCard = table.River[Random.Range(0, table.River.Count)];
            var (rank, suit) = (randomRiverCard.Rank, randomRiverCard.Suit);
            randomRiverCard.Rank = card.Rank;
            randomRiverCard.Suit = card.Suit;
            card.Rank = rank;
            card.Suit = suit;
            
            randomRiverCard.Visuals.Initialize(randomRiverCard.Rank, randomRiverCard.Suit, affectedByCheat: true);
            card.Visuals.Initialize(card.Rank, card.Suit, affectedByCheat: true);
        }
    }
}