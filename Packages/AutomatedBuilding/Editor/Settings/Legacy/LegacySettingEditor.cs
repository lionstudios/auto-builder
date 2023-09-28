using System.Collections;
using System.Collections.Generic;
using LionStudios.Suite.Editor;
using UnityEditor;
using UnityEngine;

namespace LionStudios.Editor.AutoBuilder.Legacy
{
    [CustomEditor(typeof(LegacySettingBase), true)]
    public class LegacySettingEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            GUIStyle myCustomStyle = new GUIStyle(GUI.skin.GetStyle("label"))
            {
                wordWrap = true
            };
            myCustomStyle.normal.textColor = Color.yellow;
            EditorGUILayout.LabelField("These settings are deprecated. Please use the Builder tab in (LionStudios -> Settings Manager).", myCustomStyle);
            GUILayout.Space(10f);
            if(GUILayout.Button("Migrate"))
            {
                SettingsMigrator.Migrate();
            }
            else
            {
                EditorGUILayout.LabelField("Click this button to migrate the old settings below to the Settings Manager and delete the deprecated files.", myCustomStyle);
                serializedObject.Update();
                GUILayout.Space(20f);
                base.OnInspectorGUI();
            }
        }
    }

}

