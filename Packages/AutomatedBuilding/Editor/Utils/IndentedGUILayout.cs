using UnityEngine;
using UnityEditor;

namespace LionStudios.Editor.AutoBuilder
{

    public static class IndentedGUILayout
    {
        public static bool Button(GUIContent content, GUIStyle style = null)
        {
            return style == null ? 
                GUI.Button(EditorGUI.IndentedRect(EditorGUILayout.GetControlRect()), content) : 
                GUI.Button(EditorGUI.IndentedRect(EditorGUILayout.GetControlRect()), content, style);
        }
        
        
    }

}