using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;
using static UI.IHudSelectable.State;

namespace UI
{
    public class HudHoverable : MonoBehaviour
    {
        [SerializeField] private TMP_Text _Text;
        [SerializeField] private Color _Normal;
        [SerializeField] private Color _Highlight;
        [FormerlySerializedAs("_Selectable")] [SerializeField] private MonoBehaviour _IHudSelectable;
        
        private RectTransform _rectTransform;
        
        void Awake()
        {
            _rectTransform = transform as RectTransform;
        }
        
        public bool IsSelectableActive()
        {
            return _IHudSelectable is IHudSelectable selectable && selectable.CurrentState is Pressed or Selected;
        }
        
        public void ManagedUpdate(bool forceHighlight, bool allowHighlight)
        {
            if (forceHighlight)
            {
                _Text.color = _Highlight;
                return;
            }

            if (!allowHighlight)
            {
                _Text.color = _Normal;
                return;
            }
            
            bool mouseOver = RectTransformUtility.RectangleContainsScreenPoint(_rectTransform, Input.mousePosition, null);
            _Text.color = mouseOver ? _Highlight : _Normal;
        }
    }
}