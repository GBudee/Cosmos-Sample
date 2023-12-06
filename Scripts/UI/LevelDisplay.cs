using System;
using Coffee.UIEffects;
using DG.Tweening;
using PokerMode;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

namespace UI
{
    public class LevelDisplay : MonoBehaviour
    {
        [SerializeField] private Image _LevelUpIcon;
        [SerializeField] private TooltipTarget _TooltipTarget;
        
        private UIEffect _LevelUpEffect;
        private CosmoController _target;
        private bool _showLevelUp;

        private void Awake()
        {
            _LevelUpEffect = _LevelUpIcon.GetComponent<UIEffect>();
        }
        
        public void Initialize(CosmoController target)
        {
            _target = target;
            _showLevelUp = true; // We want the first lateupdate after init to definitely hide if appropriate
        }
        
        private void LateUpdate()
        {
            if (_target == null) return;
            bool targetLevelUp = _target.HasLevelUp;
            if (targetLevelUp == _showLevelUp) return;
            
            if (targetLevelUp)
            {
                _LevelUpEffect.colorFactor = 1f;
                DOTween.Sequence().SetTarget(this)
                    .Join(_LevelUpIcon.DOFade(1f, .2f))
                    .Join(_LevelUpIcon.rectTransform.DOScale(1.2f, .2f).SetEase(Ease.OutQuad))
                    .Append(_LevelUpIcon.rectTransform.DOScale(1f, .5f).SetEase(Ease.InQuad))
                    .Join(DOVirtual.Float(1f, 0f, .5f, t => _LevelUpEffect.colorFactor = t))
                    .OnComplete(() => _LevelUpIcon.rectTransform.DOScale(1.05f, 2f).SetEase(Ease.InOutQuad).SetLoops(-1, LoopType.Yoyo).SetTarget(this));
            }
            else
            {
                this.DOKill();
                _LevelUpIcon.color = Color.white.WithAlpha(0f);
                _LevelUpEffect.colorFactor = 0f;
                _LevelUpIcon.rectTransform.localScale = Vector3.one;
            }
            
            if (_TooltipTarget != null) _TooltipTarget.enabled = targetLevelUp;
            _showLevelUp = targetLevelUp;
        }
    }
}