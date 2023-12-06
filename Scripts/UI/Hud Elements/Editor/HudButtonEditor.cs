using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;
using Utilities;

namespace UI
{
    [CustomEditor(typeof(HudButton), true)]
    [CanEditMultipleObjects]
    public class HudButtonEditor : ButtonEditor
    {
        private string[] _buttonBaseProperties;
        
        protected override void OnEnable()
        {
            _buttonBaseProperties = new[]
            {
                serializedObject.FindProperty("m_Script").propertyPath,
                serializedObject.FindProperty("m_Interactable").propertyPath,
                serializedObject.FindProperty("m_TargetGraphic").propertyPath,
                serializedObject.FindProperty("m_Transition").propertyPath,
                serializedObject.FindProperty("m_Colors").propertyPath,
                serializedObject.FindProperty("m_SpriteState").propertyPath,
                serializedObject.FindProperty("m_AnimationTriggers").propertyPath,
                serializedObject.FindProperty("m_Navigation").propertyPath,
                serializedObject.FindProperty("m_OnClick").propertyPath
            };
            base.OnEnable();
        }
        
        public override void OnInspectorGUI()
        {
            GEditorUtility.ShowScriptReference((HudButton)target);
            
            base.OnInspectorGUI();
            
            DrawPropertiesExcluding(serializedObject, _buttonBaseProperties);
            serializedObject.ApplyModifiedProperties();
        }
    }
}