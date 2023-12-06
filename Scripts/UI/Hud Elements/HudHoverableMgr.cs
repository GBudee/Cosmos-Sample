using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    public class HudHoverableMgr : MonoBehaviour
    {
        [SerializeField] private List<HudHoverable> _HudHoverables;

        private bool _allowHighlight;

        public bool AllowHighlight(bool value) => _allowHighlight = value;
        
        void LateUpdate()
        {
            // Pick active selectable system
            HudHoverable selectedElement = null;
            if (_allowHighlight)
                foreach (var highlightElement in _HudHoverables)
                    if (highlightElement.IsSelectableActive())
                    {
                        selectedElement = highlightElement;
                        break;
                    }
            
            // Update highlight state for each hoverable
            foreach (var highlightElement in _HudHoverables)
                highlightElement.ManagedUpdate(highlightElement == selectedElement, _allowHighlight && selectedElement == null);
        }
    }
}