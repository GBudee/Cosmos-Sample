using System;
using System.Collections.Generic;
using Coffee.UIEffects;
using DG.Tweening;
using MEC;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Cheats
{
    public class GumptionIcon : MonoBehaviour
    {
        [SerializeField] private Image _Image;
        [SerializeField] private UIShadow _Shadow;
        [SerializeField] private Sprite _Active;
        [SerializeField] private Sprite _Full;
        [SerializeField] private Sprite _Empty;
        [SerializeField] private ParticleSystem _SpendParticle;
        
        public enum State { Empty, Full, Active }
        
        private State? _state;
        
        public void SetState(State state)
        {
            if (_state == state) return;
            
            // Reset tweens
            this.DOKill();
            _Image.rectTransform.anchoredPosition = Vector2.zero;
            _Image.rectTransform.localScale = Vector3.one;
            _Image.rectTransform.localRotation = Quaternion.identity;
            
            if (state == State.Empty && _state == State.Active)
            {
                // Show spend anim (particle & pulse)
                _SpendParticle.Play();
                
                const float DURATION = .3f;
                DOTween.Sequence().SetTarget(this)
                    .Join(_Image.rectTransform.DOScale(1.6f, DURATION).SetEase(Ease.InQuad))
                    .InsertCallback(DURATION, () =>
                    {
                        _Image.sprite = _Empty;
                        _Shadow.enabled = false;
                    })
                    .Append(_Image.rectTransform.DOScale(1f, DURATION).SetEase(Ease.OutQuad));
            }
            else if (state == State.Active)
            {
                // Show "active" anim (persistent rotational wiggle)
                const float DURATION = .1f;
                const float ANGLE = 5;
                _Image.rectTransform.DOLocalRotate(new Vector3(0, 0, ANGLE), DURATION).From(new Vector3(0, 0, -ANGLE))
                    .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo)
                    .SetTarget(this);
                
                Shake();
                void Shake()
                {
                    _Image.rectTransform.DOShakeAnchorPos(duration: 10f, strength: 1.8f, vibrato: 10, fadeOut: false)
                        .OnComplete(Shake)
                        .SetTarget(this);
                }
            }
            else
            {
                _Image.sprite = state switch
                {
                    State.Empty => _Empty,
                    State.Full => _Full,
                    //State.Active => _Active,
                };
                _Shadow.enabled = state == State.Full;
            }
            _state = state;
        }
    }
}