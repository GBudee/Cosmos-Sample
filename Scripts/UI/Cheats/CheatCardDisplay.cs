using System.Collections.Generic;
using DG.Tweening;
using Lean.Pool;
using MEC;
using PokerMode.Cheats;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Utilities;

namespace UI.Cheats
{
    [ExecuteAfter(typeof(SleeveDisplay))]
    public class CheatCardDisplay : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CanvasGroup _CanvasGroup;
        [SerializeField] private CanvasGroup _Glow;
        [SerializeField] private Image _Foreground_Gray;
        [FormerlySerializedAs("_Foreground")] [SerializeField] private Image _Foreground_White;
        [SerializeField] private Image _Background;
        [SerializeField] private Image _Icon;
        [SerializeField] private TMP_Text _Name;
        [SerializeField] private TMP_Text _Description;
        [SerializeField] private List<Sprite> _BackgroundSprites;
        [SerializeField] private GameObject prefab_ActivationParticle;
        [SerializeField] private Color _HighlightTeal;
        
        public enum Mode { Sleeve, Menu }
        
        public CheatCard Cheat => _target;
        private bool SelfTargeted => _target?.SelfTargeted ?? false;
        
        private RectTransform _rectTransform;
        private Tween _scaleAnim;
        private Tween _glowAnim;
        private Tween _castableAnim;
        private bool _hoverAffectsText;
        
        private CheatCard _target;
        private bool _interactable;
        private bool _castable;
        private bool _selected;
        private bool _forceNoGlow;
        
        // Self-cast movement helpers
        private float _dampTime;
        private Vector3 _dampVelocity;
        private bool _firstInit = true;
        
        // Init as regular cheat card
        public void Initialize(CheatCard card, bool show = false, bool castable = true)
        {
            if (_firstInit)
            {
                _rectTransform = transform as RectTransform;
            
                const float DURATION = .125f;
                _scaleAnim = _rectTransform.DOScale(1.2f, DURATION).From(1f).SetEase(Ease.InOutQuad)
                    .SetAutoKill(false).Pause();
                _glowAnim = _Glow.DOFade(1f, DURATION).From(0f)
                    .SetAutoKill(false).Pause();
                _castableAnim = _Foreground_Gray.DOFade(0f, DURATION).From(.75f)
                    .SetAutoKill(false).Pause();
                _firstInit = false;
            }
            
            _target = card;
            
            _selected = false;
            _castable = true;
            _forceNoGlow = false;
            _interactable = show;
            _CanvasGroup.alpha = show ? 1f : 0f;
            transform.localScale = Vector3.one;
            transform.localRotation = Quaternion.identity;
            
            _scaleAnim.Rewind();
            _glowAnim.Rewind();
            if (castable) _castableAnim.Complete();
            else _castableAnim.Rewind();
            
            if (card == null)
            {
                _hoverAffectsText = true;
            }
            else
            {
                _hoverAffectsText = false;
                _Background.sprite = _BackgroundSprites[(int)card.Background];
                _Icon.sprite = CheatIconLookup.CheatIcons[card.Icon];
                _Name.text = card.Name;
                _Description.text = card.Description;
            }
        }
        
        public void SetSlot(Transform slot)
        {
            transform.parent = slot;
            transform.localPosition = Vector3.zero;
        }
        
        public void DrawAnim(float duration)
        {
            _Foreground_White.color = Color.white;
            _rectTransform.localRotation = Quaternion.identity;
            
            const float VERT_DIST = 40;
            DOTween.Sequence()
                .Join(_CanvasGroup.DOFade(1f, duration).From(0f))
                .Join(_rectTransform.DOLocalMoveY(0, duration).From(VERT_DIST).SetEase(Ease.OutQuad))
                .Insert(duration * .5f, _Foreground_White.DOFade(0f, duration * .75f))
                .OnComplete(() => _interactable = true);
        }
        
        public IEnumerator<float> ActivateAnim()
        {
            // Spawn activation particle
            var rectParent = transform.parent as RectTransform;
            var activationParticle = LeanPool.Spawn(prefab_ActivationParticle, transform.position, transform.rotation, rectParent);
            activationParticle.GetComponentInChildren<ParticleSystem>().Play();
            activationParticle.DespawnFinishedParticle();
            
            // Pulse and flash
            const float DURATION = .25f;
            DOTween.Sequence()
                .Join(_rectTransform.DOScale(1.4f, DURATION * .5f).SetEase(Ease.InQuad).SetLoops(2))
                .Join(_Foreground_White.DOFade(1f, DURATION * .5f).From(0f).SetLoops(2));
            
            yield return Timing.WaitForSeconds(DURATION + .5f);
        }
        
        public void DiscardAnim(float duration, Transform discardAnchor)
        {
            _interactable = false;
            
            var currentPos = _rectTransform.position;
            var intendedPos = discardAnchor.position;
            var verticalMove = Vector3.Project(intendedPos - currentPos, Vector3.up);
            var horizMove = Vector3.Project(intendedPos - currentPos, Vector3.right);
            
            DOTween.Sequence()
                .Join(_CanvasGroup.DOFade(0f, duration))
                .Join(_rectTransform.DOBlendableMoveBy(verticalMove, duration * .75f).SetEase(Ease.OutCubic))
                .Join(_rectTransform.DOBlendableMoveBy(horizMove, duration).SetEase(Ease.InCubic))
                .Join(_rectTransform.DOLocalRotate(new Vector3(0, 0, -90), duration))
                //.Join(_Foreground.DOFade(0f, duration * .75f))
                .Join(_rectTransform.DOScale(.05f, duration * .8f).SetEase(Ease.OutQuad))
                .OnComplete(() => LeanPool.Despawn(this));
        }
        
        public void ShowCastable(bool value)
        {
            if (_castable == value) return;
            
            if (value) _castableAnim.PlayForward();
            else _castableAnim.PlayBackwards();
            
            _castable = value;
        }
        
        public void ShowSelected(bool value, Transform slot = null, Transform selectionAnchor = null)
        {
            if (_selected == value) return;
            _selected = value;
            if (slot == null) return;
            
            // Go to selection point
            this.DOKill();
            transform.parent = value ? selectionAnchor : slot;
            if (!value || !SelfTargeted)
                transform.DOLocalMove(Vector3.zero, .25f).SetEase(Ease.InOutQuad).SetTarget(this) // Go to appropriate pos
                    .OnComplete(() => { if (!value) ForceNoGlow(false); }); // After return to position reset "forceNoGlow"
            
            if (value && SelfTargeted)
            {
                _dampTime = .05f;
                DOVirtual.Float(.05f, 0f, .2f, t => _dampTime = t);
                _dampVelocity = Vector3.zero;
            }
        }
        
        public void ForceNoGlow(bool value)
        {
            _forceNoGlow = value;
        }

        public void ActHovered()
        {
            _glowAnim.PlayForward();
            _scaleAnim.PlayForward();
        }
        
        public void ManagedUpdate(out bool mouseOver, Mode mode = Mode.Sleeve)
        {
            if (!_interactable)
            {
                mouseOver = false;
                return;
            }
            
            // Respond to mouseover
            mouseOver = RectTransformUtility.RectangleContainsScreenPoint(_rectTransform, Input.mousePosition, null);
            if (((mouseOver && _castable && mode == Mode.Sleeve) || _selected) && !_forceNoGlow)
            {
                _glowAnim.PlayForward();
                if (_hoverAffectsText) _Name.color = _HighlightTeal;
            }
            else
            {
                _glowAnim.PlayBackwards();
                if (_hoverAffectsText) _Name.color = Color.white;
            }
            if (mouseOver && mode == Mode.Sleeve && (SelfTargeted || !_selected)) _scaleAnim.PlayForward();
            else _scaleAnim.PlayBackwards();
            
            // Drag for self-targeted behaviour
            if (mode == Mode.Sleeve && _selected && SelfTargeted)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.parent as RectTransform,
                    Input.mousePosition, null, out var targetPos);
                if (_dampTime == 0) transform.localPosition = targetPos;
                else transform.localPosition = Vector3.SmoothDamp(transform.localPosition, targetPos, ref _dampVelocity, _dampTime);
            }
        }
    }
}