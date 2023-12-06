using System;
using UnityEngine;

namespace UI
{
    public class TutorialTarget : MonoBehaviour
    {
        [SerializeField] private RectTransform _Anchor;
        [SerializeField] private string _Key;
        [SerializeField] private string _Header;
        [SerializeField] [TextArea(2, 18)] private string _Contents;
        
        public RectTransform Anchor => _Anchor;
        public string Header => _getHeader != null ? _getHeader() : _Header;
        public string Contents => _getContents != null ? _getContents() : _Contents;
        
        private Func<string> _getHeader;
        private Func<string> _getContents;
        
        void OnEnable()
        {
            if (!string.IsNullOrEmpty(_Key)) Service.SimpleTutorial.RegisterTarget(_Key, this);
        }

        void OnDisable()
        {
            Service.SimpleTutorial?.UnregisterTarget(this);
        }
        
        public void RegisterCustomKey(string key, Func<string> getHeader, Func<string> getContents)
        {
            Service.SimpleTutorial.RegisterTarget(key, this);
            _getHeader = getHeader;
            _getContents = getContents;
        }
    }
}