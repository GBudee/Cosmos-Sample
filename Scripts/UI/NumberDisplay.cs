using DG.Tweening;
using PokerMode;
using TMPro;
using UnityEngine;

namespace UI
{
    public class NumberDisplay : MonoBehaviour
    {
        [SerializeField] private TMP_Text _Text;
        [SerializeField] private float _Delay = 0f;
        [SerializeField] private string _Prepend = CreditVisuals.CREDIT_SYMBOL;
        [SerializeField] private string _Append = "";
        
        private System.Func<int> _getTargetValue;
        private int _intendedValue;
        private int _displayedValue;
        
        public void Initialize(System.Func<int> getTargetValue)
        {
            _getTargetValue = getTargetValue;
            _intendedValue = -1;
        }
        
        private void LateUpdate()
        {
            if (_getTargetValue == null) return;
            var targetValue = _getTargetValue();
            if (_intendedValue == targetValue) return;
            
            _intendedValue = targetValue;
            
            const float MIN_DURATION = .3f;
            const float MAX_DURATION = 1.6f;
            const float MAX_DELTA = 300f;
            this.DOKill();
            var duration = Mathf.Lerp(MIN_DURATION, MAX_DURATION, Mathf.Abs(_intendedValue - _displayedValue) / MAX_DELTA);
            DOTween.To(t =>
                {
                    _displayedValue = Mathf.RoundToInt(t);
                    _Text.text = $"{_Prepend}{FormattedValue(_displayedValue)}{_Append}";
                }, _displayedValue, _intendedValue, duration)
                .SetDelay(_Delay)
                .SetTarget(this).SetEase(Ease.OutQuart);
        }

        public static string FormattedValue(int value)
        {
            if (value == 0) return value.ToString();
            float formattedValue = value;
            string formatPostfix = "";
            if (formattedValue >= 10000000)
            {
                formattedValue /= 1000000f;
                formatPostfix = "M";
            }
            else if (formattedValue >= 10000)
            {
                formattedValue /= 1000f;
                formatPostfix = "K";
            }
            return $"{formattedValue:#,###.#}{formatPostfix}";
        }
    }
}