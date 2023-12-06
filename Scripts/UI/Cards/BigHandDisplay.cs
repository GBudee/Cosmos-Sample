using System;
using System.Collections.Generic;
using DG.Tweening;
using PokerMode;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Utilities;

namespace UI
{
    public class BigHandDisplay : MonoBehaviour
    {
        [FormerlySerializedAs("_CardVisuals")] [SerializeField] private List<BigCardDisplay> _BigCards;
        [SerializeField] private TooltipTarget _HandCardsTooltip;
        [SerializeField] private CanvasGroup _HandValueGroup;
        [SerializeField] private TMP_Text _HandValueText;
        [SerializeField] private Image _HandValueScrim;
        [SerializeField] private TooltipTarget _HandValueTooltip;

        private HandVisuals _target;
        private List<CardVisuals> _displayedCards = new();
        private HandEvaluator.HandType? _currentHandValue;
        private Tween _handValueFader;

        void Awake()
        {
            _handValueFader = _HandValueGroup.DOFade(1, .5f).From(0)
                .OnRewind(() => _HandValueTooltip.enabled = false)
                .OnPlay(() => _HandValueTooltip.enabled = true)
                .SetAutoKill(false).Pause();
        }
        
        public void Initialize(HandVisuals target)
        {
            _target = target;
            foreach (var card in _BigCards) card.Hide();
            _displayedCards.Clear();
            _currentHandValue = null;
        }
        
        public void ShowHandValue(HandEvaluator.HandType hand)
        {
            if (hand == _currentHandValue) return;
            _HandValueText.text = hand.ToString().AddSpacesToCamelCase();
            if (_currentHandValue == null) _handValueFader.PlayForward();
            else
            {
                _HandValueScrim.DOKill();
                _HandValueScrim.color = Color.white;
                _HandValueScrim.DOFade(0f, .6f);
            }
            
            _currentHandValue = hand;
        }
        
        public void ManagedUpdate(bool allowHighlight, out BigCardDisplay hoveredCard)
        {
            hoveredCard = null;
            for (var i = _BigCards.Count - 1; i >= 0; i--)
            {
                var card = _BigCards[i];
                // Test hoveredCard to prevent overlapping mouseOvers
                card.ManagedUpdate(hoveredCard == null && allowHighlight, out bool mouseOver);
                if (hoveredCard == null && mouseOver) hoveredCard = card;
            }
        }
        
        private void HideHandValue()
        {
            _HandValueText.text = "";
            _handValueFader.PlayBackwards();
            
            _currentHandValue = null;
        }
        
        private void LateUpdate()
        {
            if (_target == null) return;
            
            // Spawn new cards
            int i = 0;
            foreach (var card in _target.Cards)
            {
                if (i >= _displayedCards.Count)
                {
                    _BigCards[i].Initialize(card);
                    _BigCards[i].SpawnAnim();
                    _displayedCards.Add(card);
                }
                else if (card.State != _BigCards[i].State)
                {
                    // Match display to state
                    _BigCards[i].Initialize(card);
                    if (card.AffectedByCheat)
                    {
                        _BigCards[i].OnCheatAnim();
                        card.AffectedByCheat = false;
                    }
                }
                i++;
            }
            
            // Despawn unused cards
            if (i < _displayedCards.Count)
                for (int j = i; j < _displayedCards.Count; j++)
                {
                    _BigCards[j].DespawnAnim();
                    _displayedCards.RemoveAt(j);
                    j--;
                }
            
            foreach (var card in _BigCards) card.UpdateFolded(_target.Folded);
            _HandCardsTooltip.enabled = _displayedCards.Count > 0;
            if (_displayedCards.Count == 0) HideHandValue();
        }
    }
}