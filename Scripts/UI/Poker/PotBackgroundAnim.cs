using System.Collections.Generic;
using DG.Tweening;
using MEC;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class PotBackgroundAnim : MonoBehaviour
    {
        [SerializeField] private Image _FlickerTarget;
        [SerializeField] private Transform _RotateTarget;
        
        void Awake()
        {
            const float DEGREES_PER_SECOND = 6f;
            _RotateTarget.transform.DOLocalRotate(new Vector3(0, 0, DEGREES_PER_SECOND), 1f)
                .SetLoops(-1, LoopType.Incremental);

            Timing.RunCoroutine(FlickerAnim());
        }
        
        private IEnumerator<float> FlickerAnim()
        {
            while (true)
            {
                _FlickerTarget.DOFade(0.4f, .045f).From(1f).SetLoops(2);
                
                var timeBetweenFlickers = Random.Range(.15f, 4f);
                yield return Timing.WaitForSeconds(timeBetweenFlickers);
            }
        }
    }
}