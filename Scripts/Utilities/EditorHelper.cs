using UnityEngine;
using UnityEditor;

namespace Utilities
{
    public static class GEditorUtility
    {
        public static void ShowScriptReference<T>(T target) where T : MonoBehaviour
        {
#if UNITY_EDITOR
            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour(target), typeof(T), false);
            GUI.enabled = true;
#endif
        }
    }
}