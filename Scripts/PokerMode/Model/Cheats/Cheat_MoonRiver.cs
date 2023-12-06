namespace PokerMode.Cheats
{
    public class Cheat_MoonRiver : CheatCard
    {
        public override TargetingType Targeting => TargetingType.River;
        public override BackgroundType Background => BackgroundType.Black;
        public override string Name => "Moon Board";
        public override string Icon => "moon_river";
        public override string Description => "Change a community card's suit to Moons";
        public override int Cost => 1;
        
        public override void Apply(Table table, Card card)
        {
            card.Suit = Suit.Moons;
            card.Visuals.Initialize(card.Rank, card.Suit, affectedByCheat: true);
        }
    }
}