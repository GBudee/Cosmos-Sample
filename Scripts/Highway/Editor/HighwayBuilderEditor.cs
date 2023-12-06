using UnityEditor;
using UnityEngine;
using Utilities;

namespace LevelDesign
{
    [CustomEditor(typeof(HighwayBuilder))]
    public class HighwayBuilderEditor : Editor
    {
        HighwayBuilder _target;
        
        SerializedProperty _Spline;
        SerializedProperty prefab_TrafficCone;
        SerializedProperty _RoadWidth;
        SerializedProperty _ObjectOffset;
        SerializedProperty _ObjectScale;
        SerializedProperty _ObjectSpacing;
        
        SerializedProperty SettingsChanged;
        
        void OnEnable()
        {
            _target = target as HighwayBuilder;
            
            _Spline = serializedObject.FindProperty("_Spline");
            prefab_TrafficCone = serializedObject.FindProperty("prefab_TrafficCone");
            _RoadWidth = serializedObject.FindProperty("_RoadWidth");
            _ObjectOffset = serializedObject.FindProperty("_ObjectOffset");
            _ObjectScale = serializedObject.FindProperty("_ObjectScale");
            _ObjectSpacing = serializedObject.FindProperty("_ObjectSpacing");
            
            SettingsChanged = serializedObject.FindProperty("SettingsChanged");
        }
        
        public override void OnInspectorGUI()
        {    
            serializedObject.Update();
            
            GEditorUtility.ShowScriptReference(_target);
            EditorGUILayout.PropertyField(_Spline);
            EditorGUILayout.PropertyField(prefab_TrafficCone);
            EditorGUILayout.PropertyField(_RoadWidth);
            EditorGUILayout.PropertyField(_ObjectOffset);
            EditorGUILayout.PropertyField(_ObjectScale);
            EditorGUILayout.PropertyField(_ObjectSpacing);
            
            SettingsChanged.boolValue |= !Mathf.Approximately(_RoadWidth.floatValue, _target._RoadWidth)
                                         || _ObjectOffset.vector3Value != _target._ObjectOffset
                                         || !Mathf.Approximately(_ObjectScale.floatValue, _target._ObjectScale)
                                         || !Mathf.Approximately(_ObjectSpacing.floatValue, _target._ObjectSpacing);
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}