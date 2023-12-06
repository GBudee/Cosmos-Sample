using System.Collections.Generic;
using DG.Tweening;
using Lean.Pool;
using MPUIKIT;
using PokerMode;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

namespace UI
{
    public class MiniHandDisplay : MonoBehaviour
    {
        [SerializeField] private MiniCardDisplay prefab_Minicard;
        [SerializeField] private CanvasGroup _HandValue;
        [SerializeField] private MPImage _HandValueBackground_Normal;
        [SerializeField] private MPImage _HandValueBackground_Faded;
        [SerializeField] private TMP_Text _HandValueText;
        
        private HandVisuals _target;
        
        private bool _revealed;
        private bool _folded;
        private List<MiniCardDisplay> _displayedCards = new();
        
        public void Initialize(HandVisuals target)
        {
            foreach (var card in _displayedCards) LeanPool.Despawn(card);
            _displayedCards.Clear();
            _revealed = false;
            _HandValue.alpha = 0f;
            
            _target = target;
        }
        
        private void LateUpdate()
        {
            if (_target == null) return;

            if (_revealed != _target.Revealed)
            {
                if (_target.Revealed)
                {
                    // Spawn cards
                    int index = 0;
                    foreach (var card in _target.Cards)
                    {
                        var newMinicard = LeanPool.Spawn(prefab_Minicard, transform);
                        newMinicard.Initialize(card.State, index);
                        newMinicard.SpawnAnim_Hand(_folded);
                        _displayedCards.Add(newMinicard);
                        index++;
                    }
                    
                    // Show hand value
                    _HandValueText.text = _target.HandValue.ToString().AddSpacesToCamelCase();
                    
                    // Fade in hand value
                    this.DOKill();
                    var rectTransform = _HandValue.transform as RectTransform;
                    const float DURATION = .4f;
                    DOTween.Sequence().SetTarget(this)
                        .Join(_HandValue.DOFade(1f, DURATION))
                        .Join(rectTransform.DOSizeDelta(new Vector2(rectTransform.sizeDelta.x, 35), DURATION)
                            .From(new Vector2(rectTransform.sizeDelta.x, 0)).SetEase(Ease.OutQuad));
                }
                else
                {
                    // Despawn cards
                    foreach (var card in _displayedCards) card.DespawnAnim();
                    _displayedCards.Clear();
                    
                    // Hide hand value
                    this.DOKill();
                    const float DURATION = .25f;
                    _HandValue.DOFade(0f, DURATION).SetTarget(this);
                }
                
                _revealed = _target.Revealed;
            }
            
            if (_folded != _target.Folded)
            {
                _folded = _target.Folded;
                _HandValueBackground_Normal.gameObject.SetActive(!_folded);
                _HandValueBackground_Faded.gameObject.SetActive(_folded);
            }
        }
    }
}