using UnityEditor;
using UnityEngine;
using Utilities;

namespace Audio
{
    [CustomEditor(typeof(AudioHelper)), CanEditMultipleObjects]
    public class AudioHelperEditor : Editor
    {
        AudioHelper _target;

        SerializedProperty _AudioSource;
        SerializedProperty PlayOnAwake;
        SerializedProperty RandomizePitch;
        SerializedProperty PitchLowerBound;
        SerializedProperty PitchUpperBound;
        SerializedProperty Delay;
        
        void OnEnable()
        {
            _target = target as AudioHelper;
            
            _AudioSource = serializedObject.FindProperty("_AudioSource");
            PlayOnAwake = serializedObject.FindProperty("PlayOnAwake");
            RandomizePitch = serializedObject.FindProperty("RandomizePitch");
            RandomizePitch = serializedObject.FindProperty("RandomizePitch");
            PitchLowerBound = serializedObject.FindProperty("PitchLowerBound");
            PitchUpperBound = serializedObject.FindProperty("PitchUpperBound");
            Delay = serializedObject.FindProperty("Delay");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            // Properties
            GEditorUtility.ShowScriptReference(_target);
            EditorGUILayout.PropertyField(_AudioSource);
            EditorGUILayout.PropertyField(PlayOnAwake);
            EditorGUILayout.PropertyField(RandomizePitch);
            if (RandomizePitch.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(PitchLowerBound);
                EditorGUILayout.PropertyField(PitchUpperBound);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.PropertyField(Delay);
            
            // Editor-preview button
            EditorGUILayout.Space();
            if (GUILayout.Button("Preview Audio")) _target.Play();
            if (GUILayout.Button("Stop Audio")) _target.Stop();

            serializedObject.ApplyModifiedProperties();
        }
    }
}