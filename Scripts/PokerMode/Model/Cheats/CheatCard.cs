using System;
using UI;
using UnityEngine;

namespace PokerMode.Cheats
{
    public abstract class CheatCard
    {
        public enum TargetingType { Hand, River, Player, Self }
        public enum BackgroundType { Black, Red, Purple, Yellow }
        
        public abstract TargetingType Targeting { get; }
        public abstract BackgroundType Background { get; }
        public abstract string Icon { get; }
        public abstract string Name { get; }
        public abstract string Description { get; }
        public abstract int Cost { get; }
        
        public bool SelfTargeted => Targeting == TargetingType.Self;
        
        public virtual bool Allow(Round round) => true;
        public abstract void Apply(Table table, Card card);

        protected int RepeatRank(int unclampedRank) => MathG.TrueModulo(unclampedRank - 2, 13) + 2;
        
        public static CheatCard CreateInstance(string cheatName)
        {
            Type cheatType = Type.GetType($"PokerMode.Cheats.Cheat_{cheatName}");
            Debug.Assert(cheatType != null, $"Couldn't instantiate cheat Cheat_{cheatName}");
            var newCheat = Activator.CreateInstance(cheatType) as CheatCard;
            return newCheat;
        }
    }
}