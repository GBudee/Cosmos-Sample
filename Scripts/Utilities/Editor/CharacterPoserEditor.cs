using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
 
namespace Utilities
{
    [CustomEditor(typeof(CharacterPoser))]
    public class CharacterPoserEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            CharacterPoser poser = (CharacterPoser)target;
            serializedObject.Update();
            
            // Check if any control changed between here and EndChangeCheck
            EditorGUI.BeginChangeCheck();
 
            base.DrawDefaultInspector();
            
            EditorGUILayout.BeginVertical();
            string poseData = "";
            if (poser.data == null)
                poseData = "No pose data.";
            else
                poseData = $"Pose Saved. {poser.data.Length} transforms detected.";
            EditorGUILayout.LabelField(poseData);
            EditorGUILayout.EndVertical();
            
            if (GUILayout.Button("Save Pose", GUILayout.Width(150), GUILayout.Height(20)))
            {
                SavePose(poser);
                Debug.Log($"Pose Saved for {poser.data.Length} transforms");
            }
            
            if (GUILayout.Button("Set Pose", GUILayout.Width(150), GUILayout.Height(20)))
            {
                SetPose(poser);
                Debug.Log($"Character Pose set");
            }
            
            // If any control changed, then apply changes
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
        }
        
        private void SavePose(CharacterPoser poser)
        {
            List<CharacterPoser.TransformData> data = new List<CharacterPoser.TransformData>();
            foreach (var transform in poser.transform.GetComponentsInChildren<Transform>())
            {
                CharacterPoser.TransformData tdata = new CharacterPoser.TransformData(transform);
                data.Add(tdata);
            }
                
            poser.data = data.ToArray();
                
            GUI.changed = true;
            UnityEditor.EditorUtility.SetDirty(target);
        }
        
        private void SetPose(CharacterPoser poser)
        {
            foreach (var t in poser.data)
            {
                t.Apply();
            }
        }
 
    }
}