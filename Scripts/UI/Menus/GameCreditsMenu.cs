using System;
using System.Collections.Generic;
using DG.Tweening;
using MEC;
using TMPro;
using UnityEngine;

namespace UI.Menus
{
    public class GameCreditsMenu : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _CanvasGroup;

        private Tween _fader;
        private bool _initialized;
        private System.Action _onHide;
        
        private void Initialize()
        {
            if (_initialized) return;
            
            _fader = _CanvasGroup.DOFade(1f, .5f).From(0f)
                .OnRewind(() => _CanvasGroup.gameObject.SetActive(false))
                .OnPlay(() => _CanvasGroup.gameObject.SetActive(true))
                .SetAutoKill(false).Pause();

            _initialized = true;
        }
        
        public void Show(System.Action onHide)
        {
            Initialize();
            _fader.PlayForward();
            _onHide = onHide;
        }
        
        private void Update()
        {
            if (Input.GetMouseButtonDown(0)) Hide();
        }
        
        private void Hide()
        {
            _onHide?.Invoke();
            _fader.PlayBackwards();
        }
    }
}