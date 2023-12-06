using System;
using DG.Tweening;
using PokerMode.Dialogue;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Orientation = PokerMode.Dialogue.DialogueAnchor.Orientation;

namespace UI
{
    public class DialogueDisplay : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _CanvasGroup;
        [SerializeField] private Image _Background;
        [SerializeField] private TMP_Text _Text;
        
        private DialogueAnchor _target;
        private string _displayedText;
        
        public void Initialize(DialogueAnchor target)
        {
            _target = target;
            _displayedText = null;
            _CanvasGroup.alpha = 0f;
        }
        
        void LateUpdate()
        {
            if (_target == null) return;
            
            Vector2 position = Camera.main.WorldToScreenPoint(_target.GetAnchor().position);
            RectTransformUtility.ScreenPointToWorldPointInRectangle(transform.parent as RectTransform, position, null, out var resultPos);
            transform.position = resultPos;
            
            if (_displayedText == _target.Text) return;
            
            // Show/Hide dialogue box (w/ updated text)
            var newText = _target.Text;
            _CanvasGroup.DOKill();
            if (newText != null)
            {
                _Text.text = newText;
                const float DURATION = .75f;
                DOTween.Sequence()
                    .Join(transform.DOScaleX(1f, DURATION).From(2f).SetEase(Ease.OutBack, overshoot: 2.5f))
                    .Join(transform.DOScaleY(1f, DURATION).From(.25f).SetEase(Ease.OutBack, overshoot: 2.5f))
                    .Join(_CanvasGroup.DOFade(1f, DURATION).SetEase(Ease.OutQuad));
            }
            else
            {
                transform.localScale = Vector3.one;
                _CanvasGroup.DOFade(0f, .2f);
            }
            
            _displayedText = newText;
        }
    }
}