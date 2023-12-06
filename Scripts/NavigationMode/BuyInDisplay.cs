using System;
using DG.Tweening;
using PokerMode;
using Shapes;
using TMPro;
using UnityEngine;
using Utilities;
using static GameController;

namespace NavigationMode
{
    [ExecuteBefore(typeof(NavigationTrigger))]
    public class BuyInDisplay : MonoBehaviour
    {
        [SerializeField] private TableController _TableController;
        [Header("Internal References")]
        [SerializeField] private NavigationTrigger _NavigationTrigger;
        [SerializeField] private SpriteRenderer _Background;
        [SerializeField] private TMP_Text _Header;
        [SerializeField] private TMP_Text _Footer;
        [SerializeField] private TMP_Text _BuyIn;
        [SerializeField] private ShapeRenderer _IconDisc;
        [SerializeField] private SpriteRenderer _Icon;
        
        private bool _objectiveAchieved;
        private Tween _fader;
        
        private void Awake()
        {
            var discColor = _IconDisc.Color;
            
            const float DURATION = .25f;
            _fader = DOTween.Sequence()
                .Join(_Background.DOFade(0f, DURATION))
                .Join(_Header.DOFade(0f, DURATION))
                .Join(_Footer.DOFade(0f, DURATION))
                .Join(_BuyIn.DOFade(0f, DURATION))
                .Join(DOVirtual.Float(1f, 0f, DURATION, t => _IconDisc.Color = Color.Lerp(discColor.WithAlpha(0f), discColor, t)))
                .Join(_Icon.DOFade(0f, DURATION))
                .SetAutoKill(false).Pause();
            
            bool fadeState = Service.GameController.CurrentMode == GameMode.Navigation;
            if (fadeState) _fader.Rewind();
            else _fader.Complete();
            
            _BuyIn.text = $"Buy-in: {CreditVisuals.CREDIT_SYMBOL}{_TableController.BuyIn}";
            _NavigationTrigger.OnFadeChanged += FadeIn;
        }
        
        public void FadeIn(bool value)
        {
            if (value && !_objectiveAchieved) _fader.PlayBackwards();
            else _fader.PlayForward();
        }
    }
}