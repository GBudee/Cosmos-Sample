using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PokerMode
{
    public class ExpressionVisuals : MonoBehaviour
    {
        [SerializeField] private Renderer _Renderer;
        [SerializeField] private List<ExpressionTex> _Expressions;
        [SerializeField] private Expression _DefaultExpression;
        
        [Serializable]
        public struct ExpressionTex
        {
            public Expression Expression;
            public Texture2D Texture;
        }
        
        public enum Expression { Neutral, Happy, Sad, Shifty_Left, Shifty_Right, Blinking, Disengaged }
        
        private bool _disengaged;
        
        void Awake()
        {
            ShowExpression(_DefaultExpression);
        }
        
        public void ExpressionToShifty(Expression value, float? probability = null)
        {
            if (probability.HasValue && Random.Range(0f, 1f) > probability.Value) return;
            
            const float EXPRESSION_DURATION = .5f;
            const float SHIFTY_DURATION = .3f;
            bool shiftyLeftFirst = Random.Range(0, 2) == 0;
            this.DOKill();
            DOTween.Sequence().SetTarget(this)
                .AppendCallback(() => ShowExpression(value, noKill: true))
                .InsertCallback(EXPRESSION_DURATION, () => ShowExpression(shiftyLeftFirst ? Expression.Shifty_Left : Expression.Shifty_Right, noKill: true))
                .InsertCallback(EXPRESSION_DURATION + SHIFTY_DURATION, () => ShowExpression(shiftyLeftFirst ? Expression.Shifty_Right : Expression.Shifty_Left, noKill: true))
                .InsertCallback(EXPRESSION_DURATION + SHIFTY_DURATION * 2f, () => ShowExpression(Expression.Neutral, noKill: true));
        }
        
        public void MarkDisengaged(bool value) => _disengaged = value;
        
        public void ShowExpression(Expression value, float? duration = null, float? probability = null, float? delay = null, bool noKill = false)
        {
            // Handle probability
            if (probability.HasValue && Random.Range(0f, 1f) > probability.Value) return;
            
            // Handle delay
            if (delay.HasValue)
            {
                DOVirtual.DelayedCall(delay.Value, () => ShowExpression(value, duration), ignoreTimeScale: false);
                return;
            }
            
            if (!noKill) this.DOKill();
            
            // Render expression
            var resultTex = _Expressions.SingleOrDefault(x => x.Expression == (_disengaged && value == Expression.Neutral ? Expression.Disengaged : value)).Texture;
            if (resultTex == null) return;
            if (_Renderer == null) Debug.LogError("ExpressionVisuals' _Renderer not assigned", gameObject);
            _Renderer.material.mainTexture = resultTex;
            
            // Handle duration (and special case for blinking in neutral) 
            if (duration.HasValue) DOVirtual.DelayedCall(duration.Value, () => ShowExpression(Expression.Neutral), ignoreTimeScale: false).SetTarget(this);
            else if (value == Expression.Neutral) DOVirtual.DelayedCall(Random.Range(2f, 8.5f), () => ShowExpression(Expression.Blinking, .2f), ignoreTimeScale: false).SetTarget(this);
        }
    }
}