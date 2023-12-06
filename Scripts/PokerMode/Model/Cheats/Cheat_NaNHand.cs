using UI;
using UnityEngine;

namespace PokerMode.Cheats
{
    public class Cheat_NaNHand : CheatCard
    {
        public override TargetingType Targeting => TargetingType.Hand;
        public override BackgroundType Background => BackgroundType.Purple;
        public override string Name => "Heads Up";
        public override string Icon => "hand_up";
        public override string Description => "Turn a hand card into a random face card";
        public override int Cost => 1;
        
        public override void Apply(Table table, Card card)
        {
            card.Rank = Random.Range(11, 15);
            card.Visuals.Initialize(card.Rank, card.Suit, affectedByCheat: true);
        }
    }
}