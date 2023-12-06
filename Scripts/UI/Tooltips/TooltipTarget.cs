using System;
using UnityEngine;

namespace UI
{
    public interface ITooltipTarget
    {
        public RectTransform Anchor { get; }
        public string Header { get; }
        public string Contents { get; }
    }
    
    [ExecuteBefore(typeof(TooltipManager))]
    public class TooltipTarget : MonoBehaviour, ITooltipTarget
    {
        [SerializeField] private RectTransform _Anchor;
        [SerializeField] private string _Header;
        [SerializeField] [TextArea(2, 18)] private string _Contents;
        [SerializeField] private float _Delay;
        
        public RectTransform Anchor => _Anchor;
        public string Header => _getHeader != null ? _getHeader() : _Header;
        public string Contents => _getContents != null ? _getContents() : _Contents;
        
        private Func<string> _getHeader;
        private Func<string> _getContents;
        private RectTransform _rectTransform;
        private bool _showing;
        private float _delayTimer;
        
        void Awake()
        {
            _rectTransform = transform as RectTransform;
        }
        
        public void SetDynamicContents(Func<string> getHeader = null, Func<string> getContents = null)
        {
            _getHeader = getHeader;
            _getContents = getContents;
        }
        
        private void LateUpdate()
        {
            bool mouseOver = RectTransformUtility.RectangleContainsScreenPoint(_rectTransform, Input.mousePosition, null);
            if (!mouseOver) _delayTimer = 0;
            if (_showing == mouseOver) return;
            
            if (mouseOver && _Delay > 0)
            {
                _delayTimer += Time.deltaTime;
                if (_delayTimer <= _Delay) return;
            }
            
            if (mouseOver) Service.TooltipManager.Show(this);
            else Service.TooltipManager.Hide(this);
            _showing = mouseOver;
        }
        
        private void OnDisable()
        {
            Service.TooltipManager?.Hide(this);
        }
    }
}