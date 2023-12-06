using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Lean.Pool;
using MEC;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using Utilities;

namespace Audio
{
    [ExecuteBefore(typeof(GameController))]
    public class AudioController : MonoBehaviour
    {
        [SerializeField] private AudioMixer _Mixer;
        [SerializeField] private List<AudioHelper> prefab_MusicTracks;
        [SerializeField] private bool _MenuMode;

        private static List<AudioHelper> _radioClips;
        
        // Audio spawning
        private Dictionary<string, AudioHelper> _audioPrefabs = new();
        private List<(string label, AudioHelper instance)> _spawnedAudio = new();
        private List<GameObject> _activeLayers = new();
        
        // Music/radio control
        private AudioMixerGroup _baseMusic;
        private AudioMixerGroup _baseVoice;
        private AudioMixerGroup _spatialMusic;
        private AudioMixerGroup _spatialVoice;
        private List<Radio> _spatialRadios = new();
        
        void Awake()
        {
            var allAudioPrefabs = Resources.LoadAll<AudioHelper>("Audio");
            foreach (var sound in allAudioPrefabs) _audioPrefabs.Add(sound.gameObject.name, sound);
            
            _baseMusic = _Mixer.FindMatchingGroups("Music - Nondiegetic")[0];
            _spatialMusic = _Mixer.FindMatchingGroups("Music - Diegetic")[0];
            _baseVoice = _Mixer.FindMatchingGroups("Radio Voice - Nondiegetic")[0];
            _spatialVoice = _Mixer.FindMatchingGroups("Radio Voice - Diegetic")[0];
            
            var startingMusicIndex = SceneManager.GetActiveScene().name switch
            {
                "MainMenu" => 1,
                _ => 3
            };
            PlayMusic(startingMusicIndex);
        }
        
        public void SetVolume(string label, int value) => DOVirtual.DelayedCall(.001f, () => _Mixer.SetFloat(label, Mathf.Log10(Mathf.Max(.001f, value / 100f)) * 20));
        
        public void RegisterRadio(Radio radio) => _spatialRadios.Add(radio);
        
        public void SetRadioSpatialization(bool value)
        {
            ChangeMixerSnapshot(value ? "Radio" : "NonRadio");
        }
        
        public void ActivateLayer(GameObject layer)
        {
            _activeLayers.Add(layer);
        }
        
        public void DeactivateLayer(GameObject layer)
        {
            _activeLayers.Remove(layer);
        }
        
        public void Play(string label, Vector3? pos = null, Transform anchor = null, GameObject layer = null, float? customPitch = null
            , int? randomizer = null, System.Action onComplete = null)
        {
            // Layer test to avoid playing undesired sounds
            if (layer != null && !_activeLayers.Contains(layer)) return;
            
            string clipName = label;
            if (randomizer != null) clipName += Random.Range(1, randomizer.Value + 1).ToString(); // Randomize clip selection
            if (!_audioPrefabs.TryGetValue(clipName, out var audioHelper)) // Find audio prefab
            {
                Debug.Log($"Audio clip \"{label}\" doesn't exist.");
                return;
            }
            
            // Spawn audio prefab
            var instance = LeanPool.Spawn(audioHelper, pos ?? anchor?.position ?? Vector3.zero, Quaternion.identity, anchor ?? transform);
            _spawnedAudio.Add((label, instance));
            
            // Play sound, and despawn when done
            instance.Play(customPitch);
            Timing.RunCoroutine(DespawnFinishedAudio());
            IEnumerator<float> DespawnFinishedAudio()
            {
                yield return Timing.WaitUntilFalse(() => instance.IsPlaying);
                LeanPool.Despawn(instance);
                _spawnedAudio.Remove((label, instance));
                onComplete?.Invoke();
            }
        }
        
        public void FadeOut(string label)
        {
            foreach (var (clipLabel, instance) in _spawnedAudio)
            {
                if (clipLabel == label) instance.FadeOut();
            }
        }
        
        private void ChangeMixerSnapshot(string snapshotName)
        {
            var snapshot = _Mixer.FindSnapshot(snapshotName);
            if (Mathf.Approximately(Time.timeSinceLevelLoad, 0f))
            {
                DOVirtual.DelayedCall(.001f, () => snapshot.TransitionTo(0f));
            }
            else
            {
                snapshot.TransitionTo(1f);
            }
        }
        
        private void PlayMusic(int trackIndex, bool playInterstitial = true, int interstitialIndex = -1)
        {
            // Pick track prefab
            if (prefab_MusicTracks.Count == 0 || SceneManager.GetActiveScene().name == "NewRenoStation") return;
            if (trackIndex >= prefab_MusicTracks.Count) trackIndex = 0;
            var musicPrefab = prefab_MusicTracks[trackIndex];
            var currentTrack = PlayRadioClip(musicPrefab, _baseMusic, _spatialMusic);
            
            // Play next track when this one is done
            Timing.RunCoroutine(PlayNext());
            IEnumerator<float> PlayNext()
            {
                if (_MenuMode || !playInterstitial)
                {
                    yield return Timing.WaitUntilFalse(() => currentTrack.IsPlaying);
                }
                else
                {
                    yield return Timing.WaitForSeconds(currentTrack.Duration - 1.5f);
                    
                    // Play an interstitial between tracks 
                    var nextInterstitial = Random.Range(0, 26);
                    if (interstitialIndex == nextInterstitial) nextInterstitial++;
                    interstitialIndex = nextInterstitial;
                    var interstitialClip = PlayInterstitial(nextInterstitial);
                    yield return Timing.WaitUntilFalse(() => interstitialClip.IsPlaying);
                    LeanPool.Despawn(interstitialClip);
                }
                
                var nextTrack = Random.Range(0, prefab_MusicTracks.Count - 2);
                if (nextTrack == trackIndex) nextTrack++; // Disallow repeat plays
                LeanPool.Despawn(currentTrack);
                PlayMusic(nextTrack, !playInterstitial, interstitialIndex);
            }
        }
        
        private AudioHelper PlayInterstitial(int interstitialIndex)
        {
            var path = $"Audio/RadioVoiceLines/RadioVoice{interstitialIndex + 1}";
            var interstitialPrefab = Resources.Load<GameObject>(path).GetComponent<AudioHelper>();
            return PlayRadioClip(interstitialPrefab, _baseVoice, _spatialVoice);
        }
        
        private AudioHelper PlayRadioClip(AudioHelper prefab, AudioMixerGroup baseMixer, AudioMixerGroup spatialMixer)
        {
            var baseClip = LeanPool.Spawn(prefab, transform);
            baseClip.MixerGroup = baseMixer;
            baseClip.Spatialize = false;
            baseClip.Play();
            foreach (var radio in _spatialRadios) radio.SpawnMusic(prefab, spatialMixer);
            
            return baseClip;
        }
    }
}