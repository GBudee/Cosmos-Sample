using DG.Tweening;
using MPUIKIT;
using PokerMode;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class BigCardDisplay : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CanvasGroup _CanvasGroup;
        [SerializeField] private CanvasGroup _Glow;
        [SerializeField] private Image _Card;
        [SerializeField] private Image _ForegroundFlash;
        [SerializeField] private Image _ForegroundFade;
        [SerializeField] private RectTransform _OutAnchor;
        [SerializeField] private RectTransform _InAnchor;
        [SerializeField] private ParticleSystem _ActivationParticle;
        [Header("Values")]
        [SerializeField] private int _HandIndex;
        
        public int HandIndex => _HandIndex;
        public (int rank, Suit suit) State { get; private set; }
        
        private CardVisuals _target;
        private RectTransform _rectTransform;
        private bool _folded;
        private Tween _fader;
        
        void Awake()
        {
            _rectTransform = _CanvasGroup.transform as RectTransform;
            
            _ForegroundFade.color = Color.clear;
            _fader = _ForegroundFade.DOFade(.9f, .5f).SetAutoKill(false).Pause();
        }
        
        public void Initialize(CardVisuals card)
        {
            _target = card;
            _Card.sprite = CardTextureLookup.CardSprites[card.State];
            State = card.State;
        }
        
        public void SpawnAnim()
        {
            _folded = false;
            _fader.Rewind();
            
            const float DURATION = .3f;
            this.DOKill();
            DOTween.Sequence().SetTarget(this)
                .Join(_CanvasGroup.DOFade(1f, DURATION).From(0f))
                .Join(_rectTransform.DOLocalRotate(_OutAnchor.localEulerAngles, DURATION).From(_InAnchor.localEulerAngles).SetEase(Ease.OutQuad))
                .Join(_rectTransform.DOAnchorPosX(_OutAnchor.anchoredPosition.x, DURATION).From(_InAnchor.anchoredPosition).SetEase(Ease.Linear))
                .Join(_rectTransform.DOAnchorPosY(_OutAnchor.anchoredPosition.y, DURATION).From(_InAnchor.anchoredPosition).SetEase(Ease.OutQuad));
        }
        
        public void DespawnAnim()
        {
            var rectTransform = _CanvasGroup.transform as RectTransform;
            
            const float DURATION = .2f;
            this.DOKill();
            DOTween.Sequence().SetTarget(this)
                .Join(_CanvasGroup.DOFade(0f, DURATION))
                .Join(rectTransform.DOAnchorPos(_InAnchor.anchoredPosition, DURATION));
        }
        
        public void OnCheatAnim()
        {
            _ForegroundFlash.color = Color.white;
            _ForegroundFlash.DOFade(0f, .25f);
            _rectTransform.DOShakeAnchorPos(.4f, 15f);
            _ActivationParticle.Play();
        }
        
        public void UpdateFolded(bool value)
        {
            if (_folded == value) return;
            if (value) _fader.PlayForward();
            _folded = value;
        }
        
        public void Hide()
        {
            this.DOKill();
            _CanvasGroup.alpha = 0f;
        }
        
        public void ManagedUpdate(bool allowHighlight, out bool mouseOver)
        {
            mouseOver = RectTransformUtility.RectangleContainsScreenPoint(_Card.rectTransform, Input.mousePosition, null);
            _Glow.alpha = allowHighlight && mouseOver ? 1f : 0f;
        }
    }
}