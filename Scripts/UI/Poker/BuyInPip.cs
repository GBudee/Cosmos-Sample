using Coffee.UIEffects;
using DG.Tweening;
using Lean.Pool;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class BuyInPip : MonoBehaviour
    {
        [SerializeField] private Image _Image;
        
        public void Initialize()
        {
            _Image.rectTransform.localScale = Vector3.one;
            _Image.GetComponent<UIEffect>().colorFactor = 0f;
        }
        
        public void DespawnAnim()
        {
            const float DURATION = 1.2f;
            _Image.color = Color.white;
            _Image.GetComponent<UIEffect>().colorFactor = .5f;
            DOTween.Sequence()
                .Join(_Image.rectTransform.DOScale(2.5f, DURATION).SetEase(Ease.OutQuad))
                .Join(_Image.DOFade(0f, DURATION))
                .OnComplete(() => LeanPool.Despawn(this));
        }
        
        public void UpdateFade(float value)
        {
            _Image.color = Color.black;
            _Image.GetComponent<UIEffect>().colorFactor = Mathf.Lerp(0, .9f, value);
        }
    }
}