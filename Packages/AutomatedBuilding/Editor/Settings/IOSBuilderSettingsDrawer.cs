using UnityEditor;
using UnityEngine;


namespace LionStudios.Editor.AutoBuilder
{
    [CustomPropertyDrawer(typeof(IOSBuildSettings))]
    public class IOSBuilderSettingsDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUILayout.PropertyField(property);
            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;
#if UNITY_IOS
                if (IndentedGUILayout.Button(new GUIContent("Create  Export.plist file")))
                {
                    IOSBuilder.CreateExportPlist();
                }
#else
                EditorGUI.BeginDisabledGroup(true);
                IndentedGUILayout.Button(new GUIContent("Create  Export.plist file"));
                EditorGUI.EndDisabledGroup();
                Color c = GUI.color;
                GUI.color = Color.yellow;
                EditorGUILayout.LabelField("Switch to iOS platform to use this button.");
                GUI.color = c;
#endif
                EditorGUI.indentLevel--;
            }
        }
    }
}