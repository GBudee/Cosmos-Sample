﻿using UnityEngine;

namespace PokerMode.Cheats
{
    public class Cheat_RiverUp : CheatCard
    {
        public override TargetingType Targeting => TargetingType.River;
        public override BackgroundType Background => BackgroundType.Purple;
        public override string Name => "Better Board";
        public override string Icon => "river_up";
        public override string Description => $"Increase a community card's rank by 1";
        public override int Cost => 1;
        
        public override void Apply(Table table, Card card)
        {
            card.Rank = RepeatRank(card.Rank + 1);
            card.Visuals.Initialize(card.Rank, card.Suit, affectedByCheat: true);
        }
    }
}