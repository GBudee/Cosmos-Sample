using UI;

namespace PokerMode.Cheats
{
    public class Cheat_PocketAce : CheatCard
    {
        public override TargetingType Targeting => TargetingType.Hand;
        public override BackgroundType Background => BackgroundType.Purple;
        public override string Name => "Pocket Ace";
        public override string Icon => "pocket_ace";
        public override string Description => "Turn one of your hand cards into an Ace (costs 2 Gumption)";
        public override int Cost => 2;
        
        public override void Apply(Table table, Card card)
        {
            card.Rank = 14;
            card.Visuals.Initialize(card.Rank, card.Suit, affectedByCheat: true);
        }
    }
}