using UnityEditor;
using UnityEngine;

namespace LionStudios.Editor.AutoBuilder
{

    [CustomPropertyDrawer(typeof(ICapability), true)]
    public class CapabilityDrawer : PropertyDrawer
    {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty enabled = property.FindPropertyRelative("enabled");
            SerializedProperty settings = property.FindPropertyRelative("settings");

            EditorGUI.PropertyField(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), enabled, label);
            if (enabled.boolValue && settings != null)
            {
                EditorGUI.indentLevel += 2;
                EditorGUI.PropertyField(new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 2f, position.width, EditorGUI.GetPropertyHeight(settings)), settings, true);
                EditorGUI.indentLevel -= 2;
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty enabled = property.FindPropertyRelative("enabled");
            SerializedProperty settings = property.FindPropertyRelative("settings");
            if (enabled.boolValue && settings != null)
                return EditorGUIUtility.singleLineHeight + EditorGUI.GetPropertyHeight(settings);
            else
                return EditorGUIUtility.singleLineHeight;
        }
    }

}