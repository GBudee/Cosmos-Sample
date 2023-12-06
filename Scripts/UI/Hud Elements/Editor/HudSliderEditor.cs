using UnityEditor;
using UnityEditor.UI;
using UnityEngine;
using Utilities;

namespace UI
{
    [CustomEditor(typeof(HudSlider), true)]
    [CanEditMultipleObjects]
    public class HudSliderEditor : SliderEditor
    {
        private string[] _sliderBaseProperties;
        
        protected override void OnEnable()
        {
            _sliderBaseProperties = new[]
            {
                serializedObject.FindProperty("m_Script").propertyPath,
                serializedObject.FindProperty("m_Interactable").propertyPath,
                serializedObject.FindProperty("m_TargetGraphic").propertyPath,
                serializedObject.FindProperty("m_Transition").propertyPath,
                serializedObject.FindProperty("m_Colors").propertyPath,
                serializedObject.FindProperty("m_SpriteState").propertyPath,
                serializedObject.FindProperty("m_AnimationTriggers").propertyPath,
                serializedObject.FindProperty("m_Navigation").propertyPath,
                serializedObject.FindProperty("m_FillRect").propertyPath,
                serializedObject.FindProperty("m_HandleRect").propertyPath,
                serializedObject.FindProperty("m_Direction").propertyPath,
                serializedObject.FindProperty("m_MinValue").propertyPath,
                serializedObject.FindProperty("m_MaxValue").propertyPath,
                serializedObject.FindProperty("m_WholeNumbers").propertyPath,
                serializedObject.FindProperty("m_Value").propertyPath,
                serializedObject.FindProperty("m_OnValueChanged").propertyPath,
            };
            base.OnEnable();
        }
        
        public override void OnInspectorGUI()
        {
            GEditorUtility.ShowScriptReference((HudSlider)target);
            
            base.OnInspectorGUI();
            
            DrawPropertiesExcluding(serializedObject, _sliderBaseProperties);
            serializedObject.ApplyModifiedProperties();
        }
    }
}