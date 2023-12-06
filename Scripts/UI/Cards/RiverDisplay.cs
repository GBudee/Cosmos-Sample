using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Lean.Pool;
using PokerMode;
using UI;
using UnityEngine;
using UnityEngine.UI;

public class RiverDisplay : MonoBehaviour
{
    [SerializeField] private MiniCardDisplay prefab_Minicard;
    [SerializeField] private Image _Background;
    [SerializeField] private TooltipTarget _TooltipTarget;
    
    private RiverVisuals _target;
    private List<MiniCardDisplay> _displayedCards = new();
    private Tween _backgroundFader;
    
    void Awake()
    {
        _backgroundFader = _Background.DOFade(1f, .3f).From(0f)
            .OnRewind(() => _TooltipTarget.enabled = false)
            .OnPlay(() => _TooltipTarget.enabled = true)
            .SetAutoKill(false).Pause();
    }
    
    public void Initialize(RiverVisuals target)
    {
        foreach (var card in _displayedCards) LeanPool.Despawn(card);
        _displayedCards.Clear();
        _Background.color = new Color(1, 1, 1, 0);
        
        _target = target;
    }

    public void ManagedUpdate(bool allowHighlight, out MiniCardDisplay hoveredCard)
    {
        hoveredCard = null;
        foreach (var card in _displayedCards)
        {
            card.ManagedUpdate(allowHighlight, out bool mouseOver);
            if (mouseOver) hoveredCard = card;
        }
    }
    
    private void LateUpdate()
    {
        if (_target == null) return;
        
        // Match river
        int i = 0;
        foreach (var card in _target.Cards)
        {
            // Spawn new cards if appropriate
            if (i >= _displayedCards.Count)
            {
                var newMinicard = LeanPool.Spawn(prefab_Minicard, transform);
                newMinicard.Initialize(card.State, i);
                newMinicard.SpawnAnim_River();
                _displayedCards.Add(newMinicard);
            }
            else if (card.State != _displayedCards[i].State)
            {
                // Match display to state
                _displayedCards[i].Initialize(card.State, i);
                if (card.AffectedByCheat)
                {
                    _displayedCards[i].OnCheatAnim();
                    card.AffectedByCheat = false;
                }
            }
            i++;
        }
        
        // Despawn unused cards
        if (i < _displayedCards.Count)
            for (int j = i; j < _displayedCards.Count; j++)
            {
                _displayedCards[j].DespawnAnim();
                _displayedCards.RemoveAt(j);
                j--;
            }
        
        if (_displayedCards.Count > 0) _backgroundFader.PlayForward();
        else _backgroundFader.PlayBackwards();
    }
}
