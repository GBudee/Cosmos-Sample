using System;
using System.Collections.Generic;
using DG.Tweening;
using Lean.Pool;
using MEC;
using UnityEngine;
using Utilities;

namespace PokerMode
{
    public class DeckVisuals : MonoBehaviour
    {
        [SerializeField] private CardVisuals prefab_CardVisuals;
        [SerializeField] private float _StackOffset = .0005f;
        [SerializeField, Range(0, 1)] private float _FlipLevel;
        
        public bool Animating { get; private set; }
        public IEnumerable<CardVisuals> Cards => _cards;
        public IEnumerable<CardVisuals> AllCards => _allCards;

        private List<CardVisuals> _cards = new();
        private List<CardVisuals> _allCards = new();

        void Awake()
        {
            CardTextureLookup.LoadCardTextures();
            
            // Populate deck w/ blank cards
            for (int i = 0; i < 52; i++)
            {
                var (pos, rot) = CardPlacement(i);
                var cardVisuals = LeanPool.Spawn(prefab_CardVisuals, pos, rot, transform);
                _cards.Add(cardVisuals);
                _allCards.Add(cardVisuals);
            }
        }
        
        public void AddCard(CardVisuals card)
        {
            card.transform.parent = transform;
            _cards.Add(card);
        }
        
        public void RemoveCard(CardVisuals card)
        {
            card.transform.parent = null;
            _cards.Remove(card);
        }
        
        public (Vector3 pos, Quaternion rot) CardPlacement(int index)
        {
            var localPos = Vector3.up * index * _StackOffset;
            var localRot = Quaternion.Euler(0, 0, Mathf.Lerp(0, -180f, _FlipLevel));
            return (transform.TransformPoint(localPos), transform.rotation * localRot);
        }
        
        public IEnumerator<float> Shuffle_Anim(IEnumerable<Card> cards)
        {
            Animating = true;
            
            const float SPAWN_DURATION = .6f;
            const float FALL_DURATION = .35f;
            const float FALL_HEIGHT = .5f;
            
            Service.AudioController.Play("Shuffle", transform.position, randomizer: 3);
            
            int index = 0;
            foreach (var card in cards)
            {
                // Initialize card visuals
                var cardVisuals = _cards[index];
                cardVisuals.Initialize(card.Rank, card.Suit);
                card.Visuals = cardVisuals;
                
                // Animate card falling from intended elevation
                var stackPos = _StackOffset * index;
                var normalizedIndex = index / (_cards.Count - 1f);
                cardVisuals.transform.localPosition = Vector3.up * stackPos;
                cardVisuals.transform.DOLocalMoveY(stackPos, FALL_DURATION).From(FALL_HEIGHT).SetEase(Ease.InQuad)
                    .SetDelay(normalizedIndex * SPAWN_DURATION)
                    .OnStart(() => cardVisuals.gameObject.SetActive(true));
                index++;
            }
            
            yield return Timing.WaitForSeconds(SPAWN_DURATION + FALL_DURATION);
            yield return Timing.WaitForOneFrame;

            Animating = false;
        }
    }
}