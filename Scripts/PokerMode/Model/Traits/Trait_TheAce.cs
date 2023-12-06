using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace PokerMode.Traits
{
    [TraitDescription("The Ace", "This player's first hand card always becomes an ace.")]
    public class Trait_TheAce : Trait
    {
        public override void OnDraw(Player player)
        {
            var toAce = player.Hand.First();
            toAce.Rank = 14;
        }
    }
}