using System;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Animations
{
    public class Flickerer : MonoBehaviour
    {
        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
        
        [Header("References")]
        [SerializeField] private Renderer _Renderer;
        [SerializeField] private int _MatIndex;
        [Header("Values")]
        [SerializeField] private float _OnDurationLowerBound;
        [SerializeField] private float _OnDurationUpperBound;
        [SerializeField] private float _OffDurationLowerBound;
        [SerializeField] private float _OffDurationUpperBound;
        
        private Color _onColor;
        
        void OnEnable()
        {
            _onColor = _Renderer.materials[_MatIndex].GetColor(EmissionColor);
            Flicker(on: true);
        }
        
        void OnDisable()
        {
            this.DOKill();
        }
        
        private void Flicker(bool on)
        {
            var durationLower = on ? _OnDurationLowerBound : _OffDurationLowerBound;
            var durationUpper = on ? _OnDurationUpperBound : _OffDurationUpperBound;
            
            var mat = _Renderer.materials[_MatIndex];
            mat.SetColor(EmissionColor, on ? _onColor : Color.black);
            DOVirtual.DelayedCall(Random.Range(durationLower, durationUpper), () => Flicker(!on), false)
                .SetTarget(this);
        }
    }
}