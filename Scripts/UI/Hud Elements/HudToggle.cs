using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class HudToggle : Toggle, IHudSelectable
    {
        [Header("Hud Toggle References")]
        [SerializeField] private TMP_Text _Text;
        
        public TMP_Text Text => _Text;
        public IHudSelectable.State CurrentState { get; private set; }
        
        protected override void Awake()
        {
            onValueChanged.AddListener(value => DoStateTransition(currentSelectionState, false));
            base.Awake();
        }
        
        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            CurrentState = (IHudSelectable.State) state;
            
            if (isOn && state == SelectionState.Normal) state = SelectionState.Selected;
            base.DoStateTransition(state, instant);
        }
        
        public void SetListener(System.Action<bool> action, bool? initialValue = null)
        {
            onValueChanged.RemoveAllListeners();
            if (initialValue != null) isOn = initialValue.Value;
            onValueChanged.AddListener(value => DoStateTransition(currentSelectionState, false));
            onValueChanged.AddListener(value => action(value));
        }
    }
}