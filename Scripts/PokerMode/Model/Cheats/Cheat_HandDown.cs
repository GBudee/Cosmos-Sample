using UnityEngine;

namespace PokerMode.Cheats
{
    public class Cheat_HandDown : CheatCard
    {
        public override TargetingType Targeting => TargetingType.Hand;
        public override BackgroundType Background => BackgroundType.Purple;
        public override string Name => "Hand Down";
        public override string Icon => "hand_down";
        public override string Description => $"Decrease a hand card's rank by 1";
        public override int Cost => 1;
        
        public override void Apply(Table table, Card card)
        {
            card.Rank = RepeatRank(card.Rank - 1);
            card.Visuals.Initialize(card.Rank, card.Suit, affectedByCheat: true);
        }
    }
}