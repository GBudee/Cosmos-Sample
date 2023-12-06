using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace PokerMode
{
    public class BidMessage : MonoBehaviour
    {
        [SerializeField] private TMP_Text _Text;
        [SerializeField] private bool _TextFollowsCamera;
        
        private Tween textFader;
        
        void Awake()
        {
            _Text.text = "";
            textFader = _Text.DOFade(1f, .25f).From(0f).SetAutoKill(false);
            textFader.Complete();
        }
        
        public void Show(string message, int credits)
        {
            _Text.text = $"{message}{CreditVisuals.CREDIT_SYMBOL}{credits}";
            textFader.PlayForward();
            this.DOKill();
            _Text.transform.DOPunchScale(Vector3.one * .15f, .25f).SetTarget(this);
        }
        
        public void Hide()
        {
            textFader.PlayBackwards();
        }
        
        private void LateUpdate()
        {
            if (!_TextFollowsCamera) return;
            var relativePos = transform.position - Camera.main.transform.position;
            float angle = Mathf.Atan2(relativePos.x, relativePos.z) * Mathf.Rad2Deg;
            _Text.transform.rotation = Quaternion.Euler(0, angle, 0);
        }
    }
}