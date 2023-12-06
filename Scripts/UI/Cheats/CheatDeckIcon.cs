using TMPro;
using UnityEngine;

namespace UI.Cheats
{
    public class CheatDeckIcon : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _CanvasGroup;
        [SerializeField] private TMP_Text _Text;
        
        public CanvasGroup CanvasGroup => _CanvasGroup;
        public RectTransform rectTransform => transform as RectTransform;
        
        public void SetCount(int value)
        {
            _Text.text = value.ToString();
        }
    }
}