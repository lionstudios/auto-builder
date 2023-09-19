using UnityEditor;
using UnityEngine;


namespace LionStudios.Editor.AutoBuilder
{
    [CustomPropertyDrawer(typeof(IOSBuildSettings))]
    public class IOSBuilderSettingsDrawer : PropertyDrawer
    {
        bool showUI = false;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            showUI = EditorGUILayout.Foldout(showUI, "iOS Build Settings", true);
            if (showUI)
            {
                EditorGUI.indentLevel++;
                SerializedProperty iterator = property.Copy();
                bool enterChildren = true;

                while (iterator.NextVisible(enterChildren))
                {
                    if (iterator.propertyPath.Contains("ios"))
                    {
                        enterChildren = false;
                        EditorGUILayout.PropertyField(iterator);
                    }
                }
                GUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Create  Export.plist file", GUILayout.Width(250), GUILayout.Height(20)))
                    {
                        IOSBuilder.CreateExportPlist();
                    }

                    GUILayout.FlexibleSpace();
                }
                GUILayout.EndHorizontal();


                EditorGUI.indentLevel--;
            }
            // EditorGUI.EndProperty();
        }
    }
}