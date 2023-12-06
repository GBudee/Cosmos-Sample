using System.Collections.Generic;
using UnityEngine;

namespace PokerMode
{
    public class CreditChip : MonoBehaviour
    {
        [Header("References")] 
        [SerializeField] private Renderer _Renderer;
        [SerializeField] private List<Material> _Materials;
        
        public enum ChipColor { White, Red, Blue, Green, Orange, Black, Pink, Purple, Yellow, LightBlue, Brown }
        
        public void SetColor(ChipColor value)
        {
            var colorIndex = (int)value;
            _Renderer.material = _Materials[colorIndex];
        }
    }
}