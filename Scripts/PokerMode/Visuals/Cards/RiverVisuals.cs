using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Lean.Pool;
using UnityEngine;

namespace PokerMode
{
    public class RiverVisuals : MonoBehaviour
    {
        [SerializeField] private CardVisuals prefab_CardVisuals;
        [SerializeField] private float _CardWidth;
        [SerializeField, Range(0, 1)] private float _FlipLevel;
        
        public IEnumerable<CardVisuals> Cards => _cards;
        
        private List<CardVisuals> _cards = new();
        
        public void AddCard(int riverIndex, CardVisuals card)
        {
            card.transform.parent = transform;
            _cards.Insert(riverIndex, card);
        }
        
        public void RemoveCard(CardVisuals card)
        {
            card.transform.parent = null;
            _cards.Remove(card);
        }
        
        public (Vector3 pos, Quaternion rot) CardPlacement(int index)
        {
            var localPos = Vector3.right * _CardWidth * index;
            var localRot = Quaternion.Euler(0, 0, Mathf.Lerp(0, -180f, _FlipLevel));
            return (transform.TransformPoint(localPos), transform.rotation * localRot);
        }
    }
}