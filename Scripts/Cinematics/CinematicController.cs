using System;
using System.Collections.Generic;
using Cinemachine;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

namespace Cinematics
{
    public class CinematicController : MonoBehaviour
    {
        [SerializeField] private CinemachineVirtualCamera _VirtualCamera;
        [SerializeField] private PlayableDirector _PlayableDirector;
        [SerializeField] private Canvas _CinematicCanvas;
        [SerializeField] private Image _BlackScrim;
        [SerializeField] private TMP_Text _CanvasText;
        [SerializeField] private List<GameObject> _CinematicProps;
        
        private bool _playing;
        private bool _readyToSkip;
        
        void OnEnable()
        {
            _VirtualCamera.enabled = true;
            _PlayableDirector.enabled = true;
        }
        
        private void OnDisable()
        {
            _VirtualCamera.enabled = false;
            _PlayableDirector.enabled = false;
        }
        
#if UNITY_EDITOR
        public void OnValidate()
        {
            _VirtualCamera.enabled = enabled;
            _PlayableDirector.enabled = enabled;
        }
#endif

        public void Initialize()
        {
            if (enabled) PlayIntro();
            else Destroy(gameObject);
        }
        
        private void PlayIntro()
        {
            Service.AudioController.SetRadioSpatialization(false);
            _CinematicCanvas.gameObject.SetActive(true);
            _CanvasText.alpha = 0f;
            
            _PlayableDirector.Play();
            _playing = true;
            _readyToSkip = false;
            DOVirtual.DelayedCall((float) _PlayableDirector.duration, OnIntroEnd, ignoreTimeScale: false).SetTarget(this);
        }
        
        private void OnIntroEnd()
        {
            //_PlayableDirector.Stop(); // <- Possibly causes enormous crashes
            _playing = false;
            
            Service.AudioController.SetRadioSpatialization(true);
            foreach (var prop in _CinematicProps) prop.gameObject.SetActive(false);
            
            _BlackScrim.DOFade(0f, .5f).OnComplete(() =>
            {
                _CinematicCanvas.gameObject.SetActive(false);
                Destroy(gameObject);
            });
            
            Service.GameController.SetMode(GameController.GameMode.Navigation);
        }
        
        void Update()
        {
            if (_playing && Input.GetMouseButtonDown(0))
            {
                if (!_readyToSkip)
                {
                    _CanvasText.DOFade(1f, .5f).SetTarget(this);
                    _readyToSkip = true;
                }
                else
                {
                    this.DOKill();
                    _CanvasText.alpha = 0f;
                    _BlackScrim.DOFade(1f, .5f).SetUpdate(UpdateType.Late).OnComplete(() =>
                    {
                        _PlayableDirector.Stop();
                        OnIntroEnd();
                    });
                }
            }
        }
    }
}