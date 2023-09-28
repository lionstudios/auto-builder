using UnityEditor;
using UnityEngine;

namespace LionStudios.Editor.AutoBuilder
{
    
    [CustomPropertyDrawer(typeof(BuildSettingsSectionBase), true)]
    public class BuildSettingsSectionBaseDrawer : PropertyDrawer
    {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.LabelField(
                new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), 
                label.text, 
                EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            SerializedProperty iterator = property.Copy();
            bool enterChildren = true;
            int depth = iterator.depth;
            while (iterator.NextVisible(enterChildren) && iterator.depth == depth + 1)
            {
                enterChildren = false;
                EditorGUILayout.PropertyField(iterator);
            }
            EditorGUI.indentLevel--;
            GUILayout.Space(10f);
        }
        
    }

}
