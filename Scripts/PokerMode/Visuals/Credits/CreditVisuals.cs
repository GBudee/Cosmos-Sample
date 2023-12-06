using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Lean.Pool;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utilities;
using Random = UnityEngine.Random;

namespace PokerMode
{
    public class CreditVisuals : MonoBehaviour
    {
        public static readonly string CREDIT_SYMBOL = "¢";
        
        [Header("References")]
        [SerializeField] private CreditChip prefab_CreditChip;
        [SerializeField] private ParticleSystem prefab_ChangePoof;
        [Header("Values")] 
        [SerializeField] private bool _TextFollowsCamera = true;
        [SerializeField] private Mode _DisplayMode;
        [SerializeField] private float _ChipWidth;
        [SerializeField] private float _ChipHeight;
        [SerializeField] private float _Noise;
        
        public enum Mode { Player, Pot }
        public int Credits { get; private set; }
        
        private List<CreditChip> _displayChips = new();
        
        public void Show(int credits)
        {
            // Compare new and old credits
            if (credits == Credits) return;
            Credits = credits;
            
            // Distribute chips -- prefer to break at least 2 black chips
            int remainingCredits = credits;
            int chipTypeCount = Enum.GetValues(typeof(CreditChip.ChipColor)).Length;
            var chipCounts = new int[chipTypeCount];
            var chipValues = new[] { 1, 5, 25, 100, 500, 2500, 10000, 50000, 250000, 1000000, 5000000 };
            for (int i = chipTypeCount - 1; i >= 0; i--)
            {
                if (i == 0)
                {
                    chipCounts[i] = remainingCredits;
                    break;
                }
                while (remainingCredits > chipValues[i])
                {
                    remainingCredits -= chipValues[i];
                    chipCounts[i]++;
                }
            }
            
            // Match total chip count w/ spawned visuals
            int chipCount = chipCounts.Sum();
            for (int i = _displayChips.Count; i < chipCount; i++)
            {
                var newChip = LeanPool.Spawn(prefab_CreditChip, Vector3.zero, Quaternion.identity, transform);
                _displayChips.Add(newChip);
            }
            for (int i = _displayChips.Count - 1; i >= chipCount; i--)
            {
                LeanPool.Despawn(_displayChips[i]);
                _displayChips.RemoveAt(i);
            }
            
            // Set chip colors and assign stacks
            int currentColor = 0; // 0 is white, 1 is red, etc.
            int stackHeight = 0;
            int stackIndex;
            Vector3 stackPos;
            UpdateStackIndex(0);
            for (int i = 0; i < chipCount; i++)
            {
                // Assess intended color for chip index
                for (int color = 0; color < chipTypeCount; color++)
                {
                    if (i < chipCounts.Take(color + 1).Sum())
                    {
                        // Show intended color (and if necessary, increment stack index)
                        if (currentColor < color)
                        {
                            if (stackHeight > 0)
                            {
                                UpdateStackIndex(stackIndex + 1);
                            }
                            stackHeight = 0;
                            currentColor = color;
                        }
                        _displayChips[i].SetColor((CreditChip.ChipColor) color);
                        break;
                    }
                }
                
                // Limit to stacks of 5
                if (stackHeight >= 5)
                {
                    UpdateStackIndex(stackIndex + 1);
                    stackHeight = 0;
                }
                
                // Place chips in stacks (w/ noise to give organic feel)
                var pos = stackPos + Vector3.up * stackHeight * _ChipHeight;
                var noise = Random.insideUnitCircle * _Noise;
                _displayChips[i].transform.localPosition = pos + noise.XYtoXZ();
                _displayChips[i].transform.localRotation = Quaternion.Euler(0, Random.Range(0, 360f), 0);
                stackHeight++;
            }
            void UpdateStackIndex(int index)
            {
                stackIndex = index;
                stackPos = new Vector3(stackIndex * _ChipWidth * (.8f + Random.Range(-.1f, .1f)), 0, stackIndex % 2 * _ChipWidth * -(.8f + Random.Range(-.1f, .1f)));
            }
            
            // Spawn poof particle from change
            var centerOfChips = transform.TransformPoint(new Vector3((stackIndex + 1) * _ChipWidth / 2f, _ChipHeight * 2.5f, 0f));
            LeanPool.Spawn(prefab_ChangePoof, centerOfChips, transform.rotation, transform).DespawnFinishedParticle();
        }
        
        public void PlayPotIncreaseSound(int newCredits, GameObject audioLayer)
        {
            var prevCredits = Credits;
            
            const float PING_INTERVAL = .08f;
            const float MIN_DURATION = .08f;
            const float MAX_DURATION = 1.6f;
            const float MAX_DELTA = 300f;
            var duration = Mathf.Lerp(MIN_DURATION, MAX_DURATION, (newCredits - prevCredits) / MAX_DELTA);
            
            float t = 0;
            var s = DOTween.Sequence();
            while (t < duration)
            {
                var normalizedT = Mathf.InverseLerp(0, MAX_DURATION, t);
                var effectiveCredits = Mathf.Lerp(prevCredits, newCredits, normalizedT);
                var maxPitchCredits = SceneManager.GetActiveScene().name switch
                {
                    "BuzzGazz" => 600,
                    "TheBlackHole" => 10000,
                    "FarOutDiner" => 250000,
                    _ => 500000,
                };
                var pitch = Mathf.Lerp(.5f, 1f, Mathf.InverseLerp(0, maxPitchCredits, effectiveCredits));
                s.InsertCallback(t, () => Service.AudioController.Play("PotIncrease", customPitch: pitch, layer: audioLayer));
                t += PING_INTERVAL;
            }
        }
    }
}
