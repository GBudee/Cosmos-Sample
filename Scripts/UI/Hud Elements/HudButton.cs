using System;
using MPUIKIT;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class HudButton : Button, IHudSelectable
    {
        [Header("Hud Button")]
        [SerializeField] private Image _DisableScrim;
        [SerializeField] private TMP_Text _Text;

        public TMP_Text Text => _Text;
        public event System.Action<IHudSelectable.State> OnStateTransition;
        public IHudSelectable.State CurrentState { get; private set; }
        
        private SelectionState _prevState;
        private bool _forceSelected;
        
        protected override void Awake()
        {
            _prevState = currentSelectionState;
            base.Awake();
        }
        
        public void ForceSelected(bool value)
        {
            _forceSelected = value;
            DoStateTransition(currentSelectionState, false);
        }
        
        public void SetText(string value) => _Text.text = value;
        
        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            CurrentState = (IHudSelectable.State) state;
            OnStateTransition?.Invoke(CurrentState);
            
            var newState = _forceSelected ? SelectionState.Selected : state;
            
            if (_DisableScrim != null) _DisableScrim.gameObject.SetActive(newState == SelectionState.Disabled);
            if (_prevState == SelectionState.Normal && newState == SelectionState.Highlighted) Service.AudioController.Play("ButtonHover");
            
            base.DoStateTransition(newState, instant);
            _prevState = newState;
        }
        
        public void SetListener(System.Action action)
        {
            onClick.RemoveAllListeners();
            onClick.AddListener(() => Service.AudioController.Play("ButtonClick"));
            onClick.AddListener(action.Invoke);
        }
    }
}