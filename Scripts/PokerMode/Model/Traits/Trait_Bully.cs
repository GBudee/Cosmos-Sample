using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Utilities;

namespace PokerMode.Traits
{
    [TraitDescription("Bully", "When this player raises or bets, the next player must raise or fold -- they cannot call.")]
    public class Trait_Bully : Trait
    {
        // CHEAT_MustRaise Flag interpreted in RoundController
        public override void OnRaiseOrBet(Player player, Table table)
        {
            table.NextPlayer(player, skipFolded: true, skipAllIn: true).CHEAT_MustRaise = true;
        }
    }
}