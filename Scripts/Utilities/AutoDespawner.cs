using System.Collections.Generic;
using Lean.Pool;
using MEC;
using UnityEngine;

namespace Utilities
{
    public static class AutoDespawner
    {
        
        public static void DespawnFinishedParticle(this GameObject container)
        {
            Timing.RunCoroutine(DespawnFinishedParticle(container, container.GetComponentInChildren<ParticleSystem>()));
        }
        
        private static IEnumerator<float> DespawnFinishedParticle(GameObject container, ParticleSystem particle)
        {
            yield return Timing.WaitUntilFalse(particle.IsAlive);
            LeanPool.Despawn(container);
        }
        
        public static void DespawnFinishedParticle<T>(this T container) where T : Component
        {
            Timing.RunCoroutine(DespawnFinishedParticle(container, container.GetComponentInChildren<ParticleSystem>()));
        }
        
        private static IEnumerator<float> DespawnFinishedParticle<T>(T container, ParticleSystem particle) where T : Component
        {
            yield return Timing.WaitUntilFalse(particle.IsAlive);
            LeanPool.Despawn(container);
        }
    }
}