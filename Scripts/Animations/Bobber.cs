using System;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Highway
{
    public class Bobber : MonoBehaviour
    {
        [SerializeField] private float _BobWidth = 1f;
        [SerializeField] private float _LowerPeriodBound = 1.5f;
        [SerializeField] private float _UpperPeriodBound = 4f;
        
        private void Awake()
        {
            transform.DOMove(transform.position + transform.up * _BobWidth * .5f, Random.Range(_LowerPeriodBound, _UpperPeriodBound))
                .From(transform.position - transform.up * _BobWidth * .5f)
                .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
        }
    }
}