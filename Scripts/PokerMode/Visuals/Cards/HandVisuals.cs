using System;
using System.Collections.Generic;
using DG.Tweening;
using Lean.Pool;
using UnityEngine;

namespace PokerMode
{
    public class HandVisuals : MonoBehaviour
    {
        [SerializeField] private Transform _FoldAnchor;
        [SerializeField] private Transform _ShowdownAnchor;
        [SerializeField] private float _StackOffset = .0005f;
        [SerializeField] private Vector2 _SplayOffset;
        [SerializeField] private float _SplayAngle;
        [SerializeField, Range(0, 1)] private float _SplayLevel;
        [SerializeField, Range(0, 1)] private float _FlipLevel;
        
        public IEnumerable<CardVisuals> Cards => _cards;
        public bool Revealed { get; set; }
        public bool Folded { get; set; }
        public HandEvaluator.HandType? HandValue { get; set; }
        
        private Transform _rigHand;
        private List<CardVisuals> _cards = new();
        
        public void Initialize(Transform rigHand)
        {
            _rigHand = rigHand;
        }
        
        public void AddCard(int index, CardVisuals card)
        {
            // Get intended local transform state
            card.transform.parent = transform;
            var localPos = card.transform.localPosition;
            var localRot = card.transform.localRotation;
            
            // Put the card in the hand anchor in the appropriate state
            card.transform.parent = _rigHand;
            card.transform.localPosition = localPos;
            card.transform.localRotation = localRot;
            _cards.Insert(index, card);
        }
        
        public void Fold_Anim()
        {
            int index = 0;
            foreach (var card in _cards)
            {
                card.transform.parent = _FoldAnchor;
                var (pos, rot) = CardPlacement(index, Player.HAND_SIZE, _FoldAnchor);
                card.transform.rotation = rot;
                
                var posDelta = pos - card.transform.position;
                var horizPosDelta = Vector3.ProjectOnPlane(posDelta, -_FoldAnchor.up);
                var verticalPosDelta = Vector3.Project(posDelta, -_FoldAnchor.up);
                
                const float DURATION = .2f;
                DOTween.Sequence()
                    .Join(card.transform.DOBlendableMoveBy(horizPosDelta, DURATION).SetEase(Ease.OutQuad))
                    .Join(card.transform.DOBlendableMoveBy(verticalPosDelta, DURATION).SetEase(Ease.InQuad));
                index++;
            }
        }
        
        public void Showdown_Anim()
        {
            int index = 0;
            foreach (var card in _cards)
            {
                card.transform.parent = _ShowdownAnchor;
                var (pos, rot) = CardPlacement(index, Player.HAND_SIZE, _ShowdownAnchor);
                card.transform.rotation = rot;
                
                var posDelta = pos - card.transform.position;
                var horizPosDelta = Vector3.ProjectOnPlane(posDelta, _ShowdownAnchor.up);
                var verticalPosDelta = Vector3.Project(posDelta, _ShowdownAnchor.up);
                
                const float DURATION = .2f;
                DOTween.Sequence()
                    .Join(card.transform.DOBlendableMoveBy(horizPosDelta, DURATION).SetEase(Ease.OutQuad))
                    .Join(card.transform.DOBlendableMoveBy(verticalPosDelta, DURATION).SetEase(Ease.InQuad));
                index++;
            }
        }
        
        public void RemoveCard(CardVisuals card)
        {
            card.transform.parent = null;
            _cards.Remove(card);
        }
        
        public (Vector3 pos, Quaternion rot) CardPlacement(int index, int handSize, Transform variantTransform = null)
        {
            var centeredIndex = index - (handSize - 1) / 2f;
            
            // Card position
            var pos = (Vector3.up * _StackOffset * index);
            var splayOffset = new Vector3(_SplayOffset.x * centeredIndex, 0, -Mathf.Pow(centeredIndex, 2) * _SplayOffset.y);
            var localPos = pos + splayOffset * _SplayLevel;
            
            // Card rotation
            var splayAngle = _SplayAngle * Mathf.Clamp(centeredIndex, -10, 10) * _SplayLevel;
            var flipAngle = Mathf.Lerp(0, -180f, _FlipLevel);
            var localRot = Quaternion.Euler(0, splayAngle, flipAngle);
            
            var transformSpace = variantTransform ?? transform;
            return (transformSpace.TransformPoint(localPos), transformSpace.rotation * localRot);
        }
    }
}