using DG.Tweening;
using TMPro;
using UnityEngine;
using Utilities;

namespace UI
{
    public class WinMessage : MonoBehaviour
    {
        [SerializeField] private TMP_Text _Text;
        
        public void Show()
        {
            const float DURATION = .4f;
            
            this.DOKill();
            _Text.ForceMeshUpdate();
            var s = DOTween.Sequence().SetTarget(this)
                .Join(_Text.DOFade(1f, DURATION))
                .Join(_Text.rectTransform.DOAnchorPos(Vector2.zero, DURATION).From(new Vector2(0, -60)).SetEase(Ease.OutQuad))
                .OnComplete(WaveAnim);
        }
        
        public void Hide()
        {
            const float DURATION = .25f;
            
            this.DOKill();
            DOTween.Sequence().SetTarget(this)
                .Join(_Text.DOFade(0f, DURATION))
                .Join(_Text.rectTransform.DOAnchorPos(new Vector2(0, 60), DURATION).From(Vector2.zero).SetEase(Ease.OutQuad));
        }
        
        private void WaveAnim()
        {
            var curve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.25f, 2.0f), new Keyframe(0.5f, 0), new Keyframe(0.75f, 2.0f), new Keyframe(1, 0f));
            curve.preWrapMode = curve.postWrapMode = WrapMode.Clamp;
            
            const float DURATION = 2f;
            const float CURVE_SCALE = 10f;
            DOTween.To(waveProgress => _Text.MatchTextToCurve(textPos => curve.Evaluate(textPos - waveProgress) * CURVE_SCALE), -1, 1, DURATION)
                .SetTarget(this).SetLoops(2, LoopType.Restart).OnComplete(Hide);
        }
    }
}
