using System;
using System.Collections.Generic;
using UnityEngine;
using BezierSolution;
using Lean.Pool;
using LevelDesign;
using Utilities;
using Random = UnityEngine.Random;

namespace Highway
{
    [RequireComponent(typeof(HighwayBuilder))]
    public class HighwayController : MonoBehaviour
    {
        public const float CAR_SPACING = 15f;
        
        [Header("References")]
        [SerializeField] private List<CarController> prefab_Cars;
        [SerializeField] private ParticleSystem _SpawnParticle;
        [SerializeField] private ParticleSystem _DespawnParticle;
        [Header("Values")]
        [SerializeField] private float _LaneOffset;
        [SerializeField] private int _Seed;
        [Header("Forced Spawn")]
        [SerializeField] private CarController _PreSpawnedCar;
        [SerializeField] private float _NormalizedPreSpawnPos;
        
        private HighwayBuilder _highwayData;
        private List<CarController> _activeCars;
        private float _randomNextCarSpacing;
        private Random.State _seedState;
        private CarController _preSpawnedCar;
        
        private void Awake()
        {
            _highwayData = GetComponent<HighwayBuilder>();
            _seedState = Determinism.GetSeed(_Seed);
            
            Determinism.Begin(_seedState);
            _activeCars = new();
            float normalizedPos = 1f;
            bool forceSpawned = false;
            while (normalizedPos > 0f)
            {
                bool shouldForceSpawn = _PreSpawnedCar != null && !forceSpawned && normalizedPos <= _NormalizedPreSpawnPos;
                forceSpawned |= shouldForceSpawn;
                var newCar = SpawnNewCar(normalizedPos, shouldForceSpawn ? _PreSpawnedCar : null);
                if (shouldForceSpawn) _preSpawnedCar = newCar;
                _highwayData._Spline.MoveAlongSpline(ref normalizedPos, -(CAR_SPACING * Random.Range(.15f, 2f) + newCar.CarLength));
            }
            _randomNextCarSpacing = Random.Range(.15f, .5f);
            Determinism.End(out _seedState);
        }
        
        void Update()
        {
            Determinism.Begin(_seedState);
            for (int i = 0; i < _activeCars.Count; i++)
            {
                var carInFront = i == _activeCars.Count - 1 ? null : _activeCars[i + 1];
                _activeCars[i].UpdateCar(carInFront, out float newPos);
                if (newPos >= 1)
                {
                    LeanPool.Despawn(_activeCars[i]);
                    _activeCars.RemoveAt(i);
                    _DespawnParticle.Play();
                    Service.AudioController.Play("PortalSound", _DespawnParticle.transform.position);

                    i--;
                }
                else if (i == 0 && _highwayData._Spline.GetLengthApproximately(0, newPos) > CAR_SPACING * _randomNextCarSpacing + _activeCars[i].CarLength)
                {
                    SpawnNewCar(0);
                    _SpawnParticle.Play();
                    Service.AudioController.Play("PortalSound", _SpawnParticle.transform.position);

                    i++;
                    
                    _randomNextCarSpacing = Random.Range(.15f, .5f);
                }
            }
            Determinism.End(out _seedState);
        }
        
        public void DisengagePreSpawnedCar()
        {
            if (_preSpawnedCar != null)
            {
                _activeCars.Remove(_preSpawnedCar);
            }
        }
        
        private CarController SpawnNewCar(float normalizedPos, CarController forceSpawn = null)
        {
            var prefab = prefab_Cars[Random.Range(0, prefab_Cars.Count)];
            var newCar = forceSpawn ?? LeanPool.Spawn(prefab, transform);
            newCar.transform.localRotation = Quaternion.identity;
            newCar.Initialize(_highwayData, normalizedPos, _LaneOffset);
            _activeCars.Insert(0, newCar);

            return newCar;
        }
    }
}