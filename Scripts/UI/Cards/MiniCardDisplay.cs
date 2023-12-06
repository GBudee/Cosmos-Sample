using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Lean.Pool;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

public class MiniCardDisplay : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CanvasGroup _VisualContainer;
    [SerializeField] private Image _Scrim;
    [SerializeField] private TMP_Text _Rank;
    [SerializeField] private Image _Suit;
    [SerializeField] private CanvasGroup _Glow;
    [SerializeField] private ParticleSystem _ActivationParticle;
    [Header("Suit Colors")]
    [SerializeField] private Color _Red;
    [SerializeField] private Color _Black;
    [Header("Suit Sprites")]
    [SerializeField] private Sprite _Star;
    [SerializeField] private Sprite _Moon;
    [SerializeField] private Sprite _Planet;
    [SerializeField] private Sprite _Rocket;
    
    public (int Rank, Suit suit) State { get; private set; }
    public int Index { get; private set; }
    
    private RectTransform _rectTransform;
    
    void Awake()
    {
        _rectTransform = (RectTransform)transform;
    }
    
    public void Initialize((int rank, Suit suit) state, int index)
    {
        _Rank.text = state.rank switch
        {
            14 => "A",
            13 => "K",
            12 => "Q",
            11 => "J",
            10 => "<indent=-.2em><cspace=-.1em>10", // To fit two characters, scrunch them together
            _ => state.rank.ToString()
        };
        _Rank.color = state.suit is Suit.Moons or Suit.Rockets ? _Black : _Red;
        
        _Suit.sprite = state.suit switch
        {
            Suit.Rockets => _Rocket,
            Suit.Moons => _Moon,
            Suit.Stars => _Star,
            Suit.Planets => _Planet,
        };
        
        State = state;
        Index = index;
    }
    
    public void SpawnAnim_River()
    {
        var rectTransform = _VisualContainer.transform as RectTransform;
            
        _Scrim.color = Color.clear;
        
        const float DURATION = .3f;
        this.DOKill();
        DOTween.Sequence().SetTarget(this)
            .Join(_VisualContainer.DOFade(1f, DURATION).From(0f))
            .Join(rectTransform.DOAnchorPos(Vector2.zero, DURATION).From(new Vector2(-20, 0)).SetEase(Ease.OutQuad));
    }
    
    public void SpawnAnim_Hand(bool faded)
    {
        var rectTransform = _VisualContainer.transform as RectTransform;
        
        _Scrim.color = new Color(0, 0, 0, faded ? .6f : 0f);
        
        const float DURATION = .3f;
        this.DOKill();
        DOTween.Sequence().SetTarget(this)
            .Join(_VisualContainer.DOFade(1f, DURATION).From(0f))
            .Join(rectTransform.DOAnchorPos(Vector2.zero, DURATION).From(new Vector2(0, -20)).SetEase(Ease.OutQuad));
    }
    
    public void DespawnAnim()
    {
        const float DURATION = .2f;
        this.DOKill();
        DOTween.Sequence().SetTarget(this)
            .Join(_VisualContainer.DOFade(0f, DURATION))
            .OnComplete(() => LeanPool.Despawn(this));
    }
    
    public void ManagedUpdate(bool allowHighlight, out bool mouseOver)
    {
        mouseOver = RectTransformUtility.RectangleContainsScreenPoint(_rectTransform, Input.mousePosition, null);
        _Glow.alpha = allowHighlight && mouseOver ? 1f : 0f;
    }
    
    public void OnCheatAnim()
    {
        this.DOKill();
        DOTween.Sequence().SetTarget(this)
            .Join(_Scrim.DOFade(0f, .25f).From(Color.white))
            .Join(_rectTransform.DOShakeAnchorPos(.4f, 15f));
        _ActivationParticle.Play();
    }
}
