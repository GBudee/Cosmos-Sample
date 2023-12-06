using BezierSolution;
using DG.Tweening;
using Lean.Pool;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UI.IHudSelectable;

namespace UI.Interaction
{
    public class PlanetButton : MonoBehaviour
    {
        [SerializeField] private HudButton _Button;
        [SerializeField] private Transform _Container;
        [SerializeField] private Image _Shadow;
        [SerializeField] private Image _SelectionGlow;
        [SerializeField] private CanvasGroup _InfoBox;
        [SerializeField] private TMP_Text _InfoDescription;
        
        private bool _selected;
        private Tween _glowAnim;
        private Tween _scaleAnim;
        private Tween _infoAnim;
        
        void Awake()
        {
            _Button.OnStateTransition += OnStateTransition;
            _Button.GetComponent<Image>().alphaHitTestMinimumThreshold = .1f;
            
            const float DURATION = .1f;
            _scaleAnim = DOTween.Sequence()
                .Join(_Container.DOScale(1.1f, DURATION).From(1f).SetEase(Ease.OutQuad))
                .Join(_Shadow.rectTransform.DOScale(1.1f, DURATION).From(1f).SetEase(Ease.OutQuad))
                .Join(_Shadow.rectTransform.DOAnchorPos(new Vector2(10f, -10f), DURATION).From(new Vector2(4f, -4f)).SetEase(Ease.OutQuad))
                .SetAutoKill(false).Pause();

            _InfoBox.alpha = 0f;
            _infoAnim = DOTween.Sequence().SetDelay(.2f)
                .Join(_InfoBox.DOFade(1f, .3f).From(0f))
                .Join(_InfoBox.transform.DOScaleX(1f, .3f).From(0.1f).SetEase(Ease.OutBack))
                .SetAutoKill(false).Pause();
        }
        
        public void SetSelected(bool value)
        {
            if (_selected == value) return;
            
            _glowAnim.Kill();
            _SelectionGlow.color = new Color(1, 1, 1, value ? 1f : 0f);
            if (value) 
            {
                _glowAnim = _SelectionGlow.DOFade(1f, .4f).From(0f).SetEase(Ease.OutQuad)
                    .OnComplete(() => _glowAnim = _SelectionGlow.DOFade(.6f, .6f).SetEase(Ease.InOutQuad).SetLoops(-1, LoopType.Yoyo));
            }
            
            _selected = value;
        }

        public void ShowUnlockedDescription()
        {
            _InfoDescription.text = "  Travel Unlocked!";
        }
        
        public void SetListener(System.Action action) => _Button.SetListener(action);
        public void ClearListener() => _Button.onClick.RemoveAllListeners();
        
        private void OnStateTransition(State state)
        {
            if (state is State.Highlighted) _scaleAnim.PlayForward();
            else _scaleAnim.PlayBackwards();
            
            if (state is State.Highlighted or State.Pressed) _infoAnim.PlayForward();
            else _infoAnim.Rewind();
        }
    }
}