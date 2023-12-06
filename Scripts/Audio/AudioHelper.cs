using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Lean.Pool;
using MEC;
using UnityEngine.Audio;

namespace Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioHelper : MonoBehaviour, IPoolable
    {
        [SerializeField] public AudioSource _AudioSource;
        public bool PlayOnAwake;
        public bool RandomizePitch;
        [Range(0.001f, 3f)] public float PitchLowerBound = 1f;
        [Range(0.001f, 3f)] public float PitchUpperBound = 1f;
        public float Delay = 0f;
        
        public bool IsPlaying => _waitingForDelay || _AudioSource.isPlaying;
        public float Duration => _AudioSource.clip.length;
        
        public bool Mute
        {
            get => _AudioSource.mute;
            set => _AudioSource.mute = value;
        }
        public bool Spatialize
        {
            get => _AudioSource.spatialBlend > 0f;
            set => _AudioSource.spatialBlend = value ? 1f : 0f;
        }
        public AudioMixerGroup MixerGroup
        {
            get => _AudioSource.outputAudioMixerGroup;
            set => _AudioSource.outputAudioMixerGroup = value;
        }
        
        private float? _baseVolume;
        private bool _waitingForDelay;
        
        private void Awake()
        {
            if (PlayOnAwake) Play();
        }
        
        public void Play(float? customPitch = null)
        {
            if (RandomizePitch) _AudioSource.pitch = Random.Range(PitchLowerBound, PitchUpperBound);
            if (customPitch != null) _AudioSource.pitch = customPitch.Value;
            if (Delay > 0 && Application.isPlaying)
            {
                _waitingForDelay = true;
                DOVirtual.DelayedCall(Delay, () =>
                {
                    _waitingForDelay = false;
                    _AudioSource.Play();
                }, ignoreTimeScale: false);
            }
            else
            {
                _AudioSource.Play();
            }
        }
        
        public void Stop()
        {
            _AudioSource.Stop();
        }
        
        public void FadeOut(float duration = .15f)
        {
            _AudioSource.DOFade(0f, duration);
        }
        
        public void GoToSnapshot(string snapshotName)
        {
            var snapshot = _AudioSource.outputAudioMixerGroup.audioMixer.FindSnapshot(snapshotName);
            snapshot.TransitionTo(1f);
        }
        
        void IPoolable.OnSpawn()
        {
            _AudioSource.DOKill();
            if (_baseVolume != null) _AudioSource.volume = _baseVolume.Value;
            else _baseVolume = _AudioSource.volume;
        }
        
        void IPoolable.OnDespawn() { }
    }
}