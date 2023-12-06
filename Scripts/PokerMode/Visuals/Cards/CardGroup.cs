using System;
using System.Collections.Generic;
using DG.Tweening;
using Lean.Pool;
using UnityEngine;

namespace PokerMode
{
    public class CardGroup : MonoBehaviour
    {
        [SerializeField] private CardVisuals prefab_CardVisuals;
        [SerializeField, Range(0, 52)] private int _Count;
        [SerializeField] private float _StackOffset;
        [SerializeField] private Vector2 _SplayOffset;
        [SerializeField] private float _SplayAngle;
        [SerializeField] private float _CardWidth;
        [SerializeField] private SplayType _SplayType;
        [SerializeField, Range(0, 1)] private float _SplayLevel;
        [SerializeField, Range(0, 1)] private float _FlipLevel;
        
        public enum SplayType { Deck, River, Hand }
        
        private List<CardVisuals> _cards = new();
        
        void Update()
        {
            if (_cards.Count != _Count) PopulateDeck();
            
            // Update card orientations to reflect serialized layout intentions
            for (int i = 0; i < _Count; i++)
            {
                var centeredIndex = i - (_Count - 1) / 2f;
                
                var pos = (Vector3.up * _StackOffset * i);
                var splayOffset = _SplayType switch
                {
                    SplayType.Deck => Vector3.back * _SplayOffset.x * i,
                    SplayType.River => Vector3.right * _CardWidth * i,
                    SplayType.Hand => new Vector3(_SplayOffset.x * centeredIndex, 0, -Mathf.Pow(centeredIndex, 2) * _SplayOffset.y),
                };
                var flipOffset = Vector3.right * DOVirtual.EasedValue(0, -_CardWidth, _FlipLevel, Ease.InOutQuad);
                var splayAngle = _SplayType == SplayType.Hand ? _SplayAngle * Mathf.Clamp(centeredIndex, -10, 10) * _SplayLevel : 0f;
                var flipAngle = Mathf.Lerp(0, -180f, _FlipLevel);
                var rot = Quaternion.Euler(0, splayAngle, flipAngle);
                _cards[i].transform.localPosition = pos + splayOffset * _SplayLevel;
                _cards[i].transform.localRotation = rot;
            }
        }
        
        private void PopulateDeck()
        {
            // Clear existing cards
            foreach (var card in _cards) LeanPool.Despawn(card);
            _cards.Clear();
            
            // Repopulate deck up to _Count
            int cardIndex = 0;
            foreach (Suit suit in Enum.GetValues(typeof(Suit)))
                for (int i = 1; i <= 13; i++)
                {
                    if (cardIndex >= _Count) return;
                    
                    var position = (Vector3.up * _StackOffset * cardIndex);
                    var newCard = LeanPool.Spawn(prefab_CardVisuals, position, Quaternion.identity, transform);
                    newCard.Initialize(i, suit);
                    _cards.Add(newCard);
                    cardIndex++;
                }
        }
    }
}