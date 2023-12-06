using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PokerMode.Traits
{
    public abstract class Trait
    {
        public static Trait CreateInstance(string traitName)
        {
            Type traitType = Type.GetType($"PokerMode.Traits.Trait_{traitName}");
            Debug.Assert(traitType != null, $"Couldn't instantiate cheat Cheat_{traitName}");
            var newCheat = Activator.CreateInstance(traitType) as Trait;
            return newCheat;
        }
        
        public static (string name, string contents) GetDescription(string traitName)
        {
            Type traitType = Type.GetType($"PokerMode.Traits.Trait_{traitName}");
            var description = traitType.GetCustomAttribute<TraitDescriptionAttribute>();
            if (description == null) return default;
            if (SceneManager.GetActiveScene().name == "NewRenoStation" && description.Tournament != null)
                return (description.Name, description.Tournament);
            else return (description.Name, description.Contents);
        }
        
        public virtual void OnDraw(Player player) { }
        public virtual void OnFlop(Player player, Table table) { }
        public virtual void OnRaiseOrBet(Player player, Table table) { }
        public virtual void OnPreBet(Player player, Table table, Player bettingPlayer, int raiseAmount, ref int minBet) { }
        public virtual void CustomBestHand(List<Card> sortedCards, ref List<Card> effectiveHand, ref List<int> value) { }
        public virtual float CustomSimulateHand(Table table, Player fixedPlayer) => table.SimulateHand(fixedPlayer);
    }
}