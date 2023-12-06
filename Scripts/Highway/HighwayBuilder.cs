using System;
using System.Collections.Generic;
using BezierSolution;
using UnityEditor;
using UnityEngine;

namespace LevelDesign
{
    [ExecuteInEditMode]
    public class HighwayBuilder : MonoBehaviour
    {
        private const string CONTAINER_NAME = "CONTAINER";
        
        public BezierSpline _Spline;
        [Header("Prefabs")]
        [SerializeField] private GameObject prefab_TrafficCone;
        [Header("Settings")]
        public float _RoadWidth = 0f;
        public Vector3 _ObjectOffset = Vector3.zero;
        public float _ObjectScale = 1f;
        public float _ObjectSpacing = 1f;
        
        // *** Hidden in inspector ***
        public List<SplineData> _TrackedSplineData;
        public bool SettingsChanged;
        
        [System.Serializable]
        public struct SplineData
        {
            public Vector3 Pos;
            public Vector3 PriorHandle;
            public Vector3 NextHandle;
            public Vector3 Normal;
            
            public SplineData(BezierPoint point)
            {
                Pos = point.position;
                PriorHandle = point.precedingControlPointPosition;
                NextHandle = point.followingControlPointPosition;
                Normal = point.normal;
            }
            
            public static bool operator ==(SplineData data, BezierPoint point)
            {
                return data.Pos == point.position
                       && data.PriorHandle == point.precedingControlPointPosition
                       && data.NextHandle == point.followingControlPointPosition
                       && data.Normal == point.normal;
            }
            public static bool operator !=(SplineData data, BezierPoint point) => !(data == point);
        }
        
#if UNITY_EDITOR
        private void OnEnable()
        {
            if (Application.isPlaying) return;
            EditorApplication.update += EditorUpdate;
        }
        
        private void OnDisable()
        {
            EditorApplication.update -= EditorUpdate;
        }
        
        private void EditorUpdate()
        {
            // Evaluate whether spline has changed
            if (_Spline == null) return;
            bool countChanged = _Spline.Count != _TrackedSplineData.Count;
            if (!SettingsChanged && !countChanged)
            {
                bool dataChanged = false;
                for (int i = 0; i < _Spline.Count; i++)
                {
                    dataChanged = _TrackedSplineData[i] != _Spline[i];
                    if (dataChanged) break;
                }
                if (!dataChanged) return;
            }
            
            // Save new spline state
            _TrackedSplineData.Clear();
            foreach (var point in _Spline) _TrackedSplineData.Add(new SplineData(point));
            SettingsChanged = false;
            
            // Update placement
            PlaceObjects();
        }
        
        private void PlaceObjects()
        {
            Debug.Log("Updating object placement", gameObject);
            
            // Find/create object container
            Transform container = transform.Find(CONTAINER_NAME);
            if (container == null)
            {
                container = new GameObject(CONTAINER_NAME).transform;
                container.parent = transform;
                container.localPosition = Vector3.zero;
                container.localRotation = Quaternion.identity;
            }
            
            // Clear existing objects
            while (container.childCount > 0) DestroyImmediate(container.GetChild(0).gameObject);
            
            // Re-place objects
            if (prefab_TrafficCone == null) return;
            float t = 0f;
            while (t < 1f)
            {
                for (int i = 0; i < 2; i ++)
                {
                    var point = _Spline.GetPoint(t);
                    var tangent = _Spline.GetTangent(t);
                    var normal = transform.up;
                    
                    var newObject = PrefabUtility.InstantiatePrefab(prefab_TrafficCone, parent: container) as GameObject;
                    var widthOffset = Vector3.Cross(tangent, normal).normalized * _RoadWidth * .5f * (i == 0 ? -1f : 1f);
                    newObject.transform.position = point + widthOffset;
                    newObject.transform.rotation = Quaternion.LookRotation(tangent, normal);
                    newObject.transform.localScale = Vector3.one * _ObjectScale;
                }
                
                _Spline.MoveAlongSpline(ref t, _ObjectSpacing);
            }
        }
#endif
    }
}