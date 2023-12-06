using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Planet;
using UnityEngine;

public class PlanetDescription : MonoBehaviour
{
    [Header("Values")]
    [SerializeField] private float _Radius = 80f;
    
    public Vector3 Origin => transform.position;
    public float Radius => _Radius;
    public IEnumerable<FlatZone> FlatZones => _flatZones ??= GetComponentsInChildren<FlatZone>();
    
    private FlatZone[] _flatZones;
    
#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0, 0, 0.75F);
        Gizmos.DrawWireSphere(transform.position, _Radius);
    }
    
    public void Gizmos_DrawDisc(Vector3 position, float radius, Color color, bool usePositionCenter = false)
    {
        var axis = (position - Origin).normalized;
        
        Vector3 discCenter;
        if (usePositionCenter) discCenter = position;
        else
        {
            var distFromOrigin = Mathf.Cos(Mathf.Asin(radius / Radius)) * Radius;
            discCenter = Origin + axis * distFromOrigin;
        }
        
        UnityEditor.Handles.color = color;
        UnityEditor.Handles.DrawWireDisc(discCenter, axis, radius);
    }
#endif
    
    public RaycastHit ProjectOnSurface(Vector3 pos, LayerMask layerCast)
    {
        var origin = transform.position;
        var radius = _Radius;
        var ray = new Ray(origin + (pos - origin).normalized * radius * 2f, origin - pos);
        Physics.Raycast(ray, out var hitInfo, radius * 2f, layerCast);
        return hitInfo;
    }
}
