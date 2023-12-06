using System;
using DG.Tweening;
using Shapes;
using UnityEngine;

namespace PokerMode
{
    public class TurnIndicator : MonoBehaviour
    {
        [SerializeField] private ShapeRenderer _Shape;
        [SerializeField] private float _MaxFade;
        [SerializeField] private float _BounceHeight = .1f;
        
        public void Activate()
        {
            _Shape.transform.DOKill();
            _Shape.transform.DOLocalMoveY(0f, .35f).From(_BounceHeight).SetEase(Ease.InOutQuad).SetLoops(-1);
            Fade(true);
        }
        
        public void Deactivate()
        {
            Fade(false);
        }
        
        private void Fade(bool value)
        {
            const float DURATION = .3f;
            DOTween.ToAlpha(() => _Shape.Color, c => _Shape.Color = c,
                value ? _MaxFade : 0f, DURATION);
        }
        
        void LateUpdate()
        {
            var towardCamera = transform.position - Camera.main.transform.position;
            var planarFacing = transform.parent.InverseTransformDirection(Vector3.ProjectOnPlane(towardCamera, transform.parent.up));
            float angle = Mathf.Atan2(planarFacing.x, planarFacing.z) * Mathf.Rad2Deg;
            transform.localEulerAngles = new Vector3(0, angle, 0);
        }
    }
}
