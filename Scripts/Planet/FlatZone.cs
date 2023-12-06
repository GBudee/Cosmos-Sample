using System;
using UnityEngine;

namespace Planet
{
    public class FlatZone : MonoBehaviour
    {
        [SerializeField] private float Radius = 10f;
        [SerializeField] private float EdgeWidth = 1f;
        [SerializeField] private bool ManualOrientation;

        private PlanetDescription _planet;
        
#if UNITY_EDITOR
        public void OnDrawGizmosSelected()
        {
            var planet = GetComponentInParent<PlanetDescription>();
            if (Radius + EdgeWidth >= planet.Radius) return;
            
            planet.Gizmos_DrawDisc(transform.position, Radius, new Color(0, 1, 0, .75f));
            planet.Gizmos_DrawDisc(transform.position, Radius + EdgeWidth, new Color(1, 1, 0, .75f));
        }
#endif
        
        // Modifies the character's expected "up-vector" if in the flat zone
        public Vector3 Evaluate(Vector3 characterPos, Vector3 intendedUp)
        {
            _planet ??= GetComponentInParent<PlanetDescription>();
            if (Radius + EdgeWidth >= _planet.Radius) return intendedUp;
            
            var characterAngle = Vector3.Angle(characterPos - _planet.Origin, transform.position - _planet.Origin) * Mathf.Deg2Rad;
            var innerEffectAngle = Mathf.Asin(Radius / _planet.Radius);
            var outerEffectAngle = Mathf.Asin((Radius + EdgeWidth) / _planet.Radius);
            float effectStrength = Mathf.InverseLerp(outerEffectAngle, innerEffectAngle, characterAngle);
            if (effectStrength <= 0) return intendedUp;
            
            var myOrientation = ManualOrientation 
                ? transform.up
                : _planet.ProjectOnSurface(transform.position, 1 << gameObject.layer).normal;
            return Vector3.Slerp(intendedUp, myOrientation, effectStrength);
        }
    }
}
