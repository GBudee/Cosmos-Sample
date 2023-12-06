using System.Collections.Generic;
using DG.Tweening;
using MEC;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI.Menus
{
    public class LoadingScreen : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _CanvasGroup;
        
        public void LoadScene(string sceneToLoad)
        {
            DontDestroyOnLoad(gameObject);
            
            // Destroy all coroutines and tweens
            Timing.KillCoroutines();
            DOTween.Clear();
            
            // Fade-in, then load
            const float FADE_DURATION = 1f; 
            _CanvasGroup.DOFade(1f, FADE_DURATION).From(0f)
                .SetUpdate(UpdateType.Normal, isIndependentUpdate: true)
                .OnComplete(() => SceneManager.LoadScene(sceneToLoad, LoadSceneMode.Single));
            
            // Attach fade-out to scene load
            SceneManager.sceneLoaded += FadeOut;
            void FadeOut(Scene scene, LoadSceneMode mode)
            {
                Timing.RunCoroutine(FadeOutRoutine(), Segment.RealtimeUpdate);
                SceneManager.sceneLoaded -= FadeOut;
            }
            
            // Fade-out
            IEnumerator<float> FadeOutRoutine()
            {
                // Wait for scene to be completely loaded
                yield return Timing.WaitForOneFrame;
                yield return Timing.WaitForOneFrame;
                
                Time.timeScale = 1f; // Unpause timescale in case we're loading from the escape menu
                
                // Load scene
                _CanvasGroup.DOFade(0f, FADE_DURATION)
                    .SetUpdate(UpdateType.Normal, isIndependentUpdate: true)
                    .OnComplete(() => Destroy(gameObject));
            }
        }
    }
}
