using System.Collections;
using System.Collections.Generic;
using Audio;
using DG.Tweening;
using Lean.Pool;
using MEC;
using UnityEngine;
using UnityEngine.Audio;

[ExecuteBefore(typeof(AudioController))]
public class Radio : MonoBehaviour
{
    [SerializeField] private float _BPM = 80f;

    void Awake()
    {
        Service.AudioController.RegisterRadio(this);
    }
    
    void Start()
    {
        const float SCALE_DIFF = .035f;
        float duration = (60f / _BPM) * .5f;
        DOTween.Sequence()
            .Join(transform.DOScaleY(1f + SCALE_DIFF, duration).From(1f - SCALE_DIFF).SetEase(Ease.InOutQuad))
            .Join(transform.DOScaleZ(1f - SCALE_DIFF, duration).From(1f + SCALE_DIFF).SetEase(Ease.InOutQuad))
            .SetLoops(-1, LoopType.Yoyo);
    }
    
    public void SpawnMusic(AudioHelper prefab, AudioMixerGroup mixerGroup)
    {
        var instance = LeanPool.Spawn(prefab, transform.position, Quaternion.identity, transform);
        instance.MixerGroup = mixerGroup;
        instance.Spatialize = true;
        instance.Play();
        
        Timing.RunCoroutine(DespawnFinishedAudio());
        IEnumerator<float> DespawnFinishedAudio()
        {
            yield return Timing.WaitUntilFalse(() => instance.IsPlaying);
            LeanPool.Despawn(instance);
        }
    }
}
