using UnityEngine;

namespace Utilities
{
    public static class ColorExtensions
    {
        public static Color WithAlpha(this Color target, float value)
        {
            return new Color(target.r, target.g, target.b, value);
        }
        
    }
}