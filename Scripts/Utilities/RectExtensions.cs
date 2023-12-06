using UnityEngine;

namespace Utilities
{
    public static class RectExtensions
    {
        public static RectTransform GetRect(this CanvasGroup target) => target.transform as RectTransform;
        
        public static void SetAnchorX(this RectTransform target, float x)
        {
            target.anchoredPosition = new Vector2(x, target.anchoredPosition.y);
        }
        
        public static void SetAnchorY(this RectTransform target, float y)
        {
            target.anchoredPosition = new Vector2(target.anchoredPosition.x, y);
        }
    }
}