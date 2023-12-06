using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PokerMode
{
    public static class CardTextureLookup
    {
        public static Dictionary<(int rank, Suit suit), Texture> CardTextures = null;
        public static Dictionary<(int rank, Suit suit), Sprite> CardSprites = null;
        
        public static void LoadCardTextures()
        {
            if (CardTextures != null) return;
            
            // Populate dictionary from list of card textures
            CardTextures = new();
            CardSprites = new();
            LoadCardVisuals(Resources.LoadAll<Texture>("Cards").Select(x => (x.name, x)), CardTextures);
            LoadCardVisuals(Resources.LoadAll<Sprite>("CardSprites").Select(x => (x.name, x)), CardSprites);
        }
        
        private static void LoadCardVisuals<T>(IEnumerable<(string name, T value)> loadedElements, Dictionary<(int rank, Suit suit), T> targetDict)
        {
            foreach (var element in loadedElements)
            {
                var nameParts = element.name.Split('_');
                if (nameParts.Length != 2) continue;
                int rank = nameParts[0] switch // Convert face cards and ace to number format
                {
                    "jack" => 11,
                    "queen" => 12,
                    "king" => 13,
                    "ace" => 14,
                    _ => int.Parse(nameParts[0])
                };
                Suit suit = nameParts[1] switch
                {
                    "rockets" => Suit.Rockets,
                    "stars" => Suit.Stars,
                    "moons" => Suit.Moons,
                    "planets" => Suit.Planets
                };
                
                targetDict.Add((rank, suit), element.value);
            }
        }
    }
}