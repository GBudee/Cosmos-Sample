using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class HudSlider : Slider, IHudSelectable
    {
        [Header("Hud Slider References")]
        [SerializeField] private TMP_Text _Text;
        [SerializeField] private bool _UseCustomRounding;
        [SerializeField] private float _CustomRounding = 1f;
        
        public IHudSelectable.State CurrentState { get; private set; }
        
        protected override void Set(float input, bool sendCallback = true)
        {
            if (_UseCustomRounding) input = Mathf.Round(input / _CustomRounding) * _CustomRounding;
            base.Set(input, sendCallback);
            if (_Text != null) _Text.text = wholeNumbers ? ((int)m_Value).ToString() : m_Value.ToString("0.00");
        }
        
        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            CurrentState = (IHudSelectable.State) state;
            base.DoStateTransition(state, instant);
        }
        
        public void SetListener(System.Action<float> action, float? initialValue = null)
        {
            onValueChanged.RemoveAllListeners();
            if (initialValue != null) value = initialValue.Value;
            onValueChanged.AddListener(action.Invoke);
        }
        
        public void SetListener(System.Action<int> action, int? initialValue = null)
        {
            onValueChanged.RemoveAllListeners();
            if (initialValue != null) value = initialValue.Value;
            onValueChanged.AddListener(localValue => action.Invoke((int)localValue));
        }
    }
}