using System.Collections;
using System.Collections.Generic;
using UI;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;
using Utilities;

namespace UI
{
    [CustomEditor(typeof(HudToggle), true)]
    [CanEditMultipleObjects]
    public class HudToggleEditor : ToggleEditor 
    {
        private string[] _toggleBaseProperties;
        
        protected override void OnEnable()
        {
            _toggleBaseProperties = new[]
            {
                serializedObject.FindProperty("m_Script").propertyPath,
                serializedObject.FindProperty("m_Interactable").propertyPath,
                serializedObject.FindProperty("m_TargetGraphic").propertyPath,
                serializedObject.FindProperty("m_Transition").propertyPath,
                serializedObject.FindProperty("m_Colors").propertyPath,
                serializedObject.FindProperty("m_SpriteState").propertyPath,
                serializedObject.FindProperty("m_AnimationTriggers").propertyPath,
                serializedObject.FindProperty("m_Navigation").propertyPath,
                serializedObject.FindProperty("toggleTransition").propertyPath,
                serializedObject.FindProperty("graphic").propertyPath,
                serializedObject.FindProperty("m_Group").propertyPath,
                serializedObject.FindProperty("m_IsOn").propertyPath,
                serializedObject.FindProperty("onValueChanged").propertyPath,
            };
            base.OnEnable();
        }
        
        public override void OnInspectorGUI()
        {
            GEditorUtility.ShowScriptReference((HudToggle)target);
            
            base.OnInspectorGUI();
        
            DrawPropertiesExcluding(serializedObject, _toggleBaseProperties);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
