using System;
using System.Collections.Generic;
using DG.Tweening;
using InteractionMode;
using PokerMode;
using TMPro;
using UI.Menus;
using UnityEngine;
using static GameController;

namespace NavigationMode
{
    [ExecuteAfter(typeof(GameController))]
    public class NavigationTrigger : MonoBehaviour
    {
        [SerializeField] private TableController _TargetTable;
        [SerializeField] private InteractionPoint _TargetInteraction;
        [SerializeField] private List<TMP_Text> _Text;
        [SerializeField] private string _FarText;
        [SerializeField] private string _CloseText;
        
        public event Action<bool> OnFadeChanged;
        
        private Sequence _fader;
        private bool _playerPresent;
        private bool _hideText;
        
        void Awake()
        {
            const float DURATION = .2f;
            _fader = DOTween.Sequence().SetTarget(this).SetAutoKill(false).Pause();
            foreach (var text in _Text)
            {
                _fader.Join(text.DOFade(1f, DURATION).From(0f));
                text.text = _FarText;
            }
            bool fadeState = Service.GameController.CurrentMode == GameMode.Navigation;
            if (fadeState) _fader.Complete();
            else _fader.Rewind();
        }
        
        void OnTriggerEnter(Collider other)
        {
            foreach (var text in _Text) text.text = _CloseText;
            _playerPresent = true;
        }
        
        void OnTriggerExit(Collider other)
        {
            foreach (var text in _Text) text.text = _FarText;
            _playerPresent = false;
        }

        public void HideText()
        {
            _hideText = true;
        }
        
        void Update()
        {
            if (Service.GameController.CurrentMode == GameMode.Cinematic) return;
            
            // Activate on button press
            if (_playerPresent && !EscapeMenu.Paused && Input.GetKeyDown(KeyCode.E))
            {
                if (_TargetTable != null && Service.GameController.CurrentMode == GameMode.Navigation)
                {
                    Service.GameController.SetMode(GameMode.Poker, table: _TargetTable);
                }
                else if (_TargetInteraction != null)
                {
                    var intendedMode = Service.GameController.CurrentMode == GameMode.Interaction ? GameMode.Navigation : GameMode.Interaction;
                    Service.GameController.SetMode(intendedMode, interaction: _TargetInteraction);
                }
            }
            
            // Show/hide when appropriate
            if (EscapeMenu.Paused || _hideText) _fader.Rewind();
            else if (Service.GameController.CurrentMode == GameMode.Navigation)
            {
                _fader.PlayForward();
                OnFadeChanged?.Invoke(true);
            }
            else
            {
                _fader.PlayBackwards();
                OnFadeChanged?.Invoke(false);
            }
        }
    }
}
