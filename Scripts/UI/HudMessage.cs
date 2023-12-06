using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class HudMessage : MonoBehaviour
    {
        [SerializeField] private Image _Dot;
        [SerializeField] private TMP_Text _Text;
        
        public void ShowMessage(string message, float? duration = null)
        {
            _Text.text = message;
            this.DOKill();
            var s = DOTween.Sequence().SetTarget(this)
                .Join(_Text.DOFade(1f, .3f));
                //.Join(_Dot.DOFade(0f, .3f));
            if (duration != null) s.Insert(duration.Value, _Text.DOFade(0f, .3f));
        }
        
        public void Hide()
        {
            this.DOKill();
            DOTween.Sequence().SetTarget(this)
                .Join(_Text.DOFade(0f, .3f));
            //.Join(_Dot.DOFade(0f, .3f));
        }
        
        public void ShowPlayerTurn()
        {
            _Text.text = "Your Turn";
            this.DOKill();
            DOTween.Sequence().SetTarget(this)
                .Join(_Text.DOFade(1f, .3f))
                .Join(_Dot.DOFade(1f, .3f))
                .OnComplete(() => _Dot.DOFade(.5f, .6f).SetTarget(this).SetLoops(-1));
        }
    }
}