using System.Collections;
using System.Collections.Generic;
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
            EditorGUILayout.LabelField("These settings are deprecated. Please use builder settings in lion settings (Unity Menu -> LionStudios -> Settings Manager)", myCustomStyle);
            EditorGUILayout.LabelField("You can click the button below to migrate these settings to lion settings", myCustomStyle);
            if(GUILayout.Button("Migrate"))
            {
                SettingsMigrator.Migrate();
            }
            serializedObject.Update();

            base.OnInspectorGUI();
            
        }
    }

}

