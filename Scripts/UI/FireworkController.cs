using System.Collections.Generic;
using DG.Tweening;
using Lean.Pool;
using MEC;
using UnityEngine;
using Utilities;

namespace UI
{
    public class FireworkController : MonoBehaviour
    {
        [SerializeField] private Transform prefab_Firework;
        
        public void CelebrateVictory()
        {
            var rectTransform = transform as RectTransform;
            
            var rect = rectTransform.rect;
            for (int i = 0; i < 14; i++)
            {
                var randomInRect = new Vector2(Random.Range(rect.xMin, rect.xMax), Random.Range(rect.yMin, rect.yMax));
                const float MAX_DELAY = 1.5f;
                var randomDelay = DOVirtual.EasedValue(0, MAX_DELAY, Random.Range(0f, 1f), Ease.InQuad);
                
                DOVirtual.DelayedCall(randomDelay, () =>
                {
                    var newFirework = LeanPool.Spawn(prefab_Firework, transform);
                    newFirework.localPosition = randomInRect;
                    newFirework.DespawnFinishedParticle();
                    Service.AudioController.Play("Firework", randomizer: 3);
                }, ignoreTimeScale: false);
            }
        }
    }
}