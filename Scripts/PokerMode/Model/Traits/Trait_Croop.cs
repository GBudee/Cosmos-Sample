using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Utilities;

namespace PokerMode.Traits
{
    [TraitDescription("Croop", "This player is always the dealer.", "This player prefers to be the dealer, but the tournament refs put a stop to it.")]
    public class Trait_Croop : Trait
    {
        // Implemented in Table.StartHand()
    }
}