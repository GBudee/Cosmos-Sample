using System.Collections.Generic;
using UnityEngine;

namespace UI.Cheats
{
    public static class CheatIconLookup
    {
        public static Dictionary<string, Sprite> CheatIcons => _cheatIcons ??= LoadIcons();
        
        private static Dictionary<string, Sprite> _cheatIcons;
        
        private static Dictionary<string, Sprite> LoadIcons()
        {
            // Populate dictionary from list of card textures
            var results = new Dictionary<string, Sprite>();
            foreach (var element in Resources.LoadAll<Sprite>("CheatIcons"))
            {
                results.Add(element.name, element);
            }
            return results;
        }
    }
}