using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace LionStudios.Editor.AutoBuilder.AdapterStabilizer
{
    
    public class LionMaxAdapterServiceWindow : EditorWindow
    {
        private const int COLUMN_WIDTH = 120;
        
        [MenuItem("LionStudios/Adapter Service")]
        public static void ShowWindow()
        {
            var window = GetWindow<LionMaxAdapterServiceWindow>();
            window.titleContent = new GUIContent("Lion Max Adapter Service");
            window.minSize = new Vector2(1000, 1200);
            window.maxSize = new Vector2(1000, 1200);
        }

        private Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];

            for (int i = 0; i < pix.Length; i++)
                pix[i] = col;

            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();

            return result;
        }

        private void OnGUI()
        {
            EditorGUILayout.Space();
            if (LionMaxAdapterStabiliser.AdNetworks == null)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Couldn't fetch stable ad networks. Check your internet connection.", EditorStyles.boldLabel);
                EditorGUILayout.Space();
                if (GUILayout.Button("Refresh", GUILayout.Width(100f)))
                    Refresh();
                EditorGUILayout.EndHorizontal();
                return;
            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Installed Ad Networks", EditorStyles.boldLabel);

            EditorGUILayout.Space();

            if (GUILayout.Button("Refresh", GUILayout.Width(100f)))
            {
                Refresh();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginVertical(GUI.skin.box);


            EditorGUILayout.BeginHorizontal();

            GUILayout.Label("Network Name", GUILayout.Width(COLUMN_WIDTH));
            GUILayout.Label("Code Name", GUILayout.Width(COLUMN_WIDTH));

            GUILayout.Label($"Android Installed", GUILayout.Width(COLUMN_WIDTH));
            GUILayout.Label($"Android Stable", GUILayout.Width(COLUMN_WIDTH));
            GUILayout.Label($"Android Broken", GUILayout.Width(COLUMN_WIDTH));

            GUILayout.Label($"iOS Installed", GUILayout.Width(COLUMN_WIDTH));
            GUILayout.Label($"iOS Stable", GUILayout.Width(COLUMN_WIDTH));
            GUILayout.Label($"iOS Broken", GUILayout.Width(COLUMN_WIDTH));

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            Color originalColor = GUI.color;

            Color[] colors = new Color[] { new Color(1.0f, 1.0f, 1.0f, 0.05f), new Color(1.0f, 1.0f, 1.0f, 0.1f) };
            GUIStyle gsTest = new GUIStyle();

            int rowColorIndex = 0;
            foreach (var network in LionMaxAdapterStabiliser.AdNetworks.OrderBy(x => x.NetworkName))
            {
                if (network.InstalledAndroidVersion == null && network.InstalledIosVersion == null)
                {
                    continue;
                }
                gsTest.normal.background = MakeTex(600, 1, colors[rowColorIndex % 2]);


                EditorGUILayout.BeginHorizontal(gsTest);

                GUILayout.Label(network.NetworkName, GUILayout.Width(COLUMN_WIDTH));
                GUILayout.Label(network.NetworkCodeName, GUILayout.Width(COLUMN_WIDTH));

                GUI.color = GetInstalledVersionColor(network.InstalledAndroidVersion, network.AndroidBuild, network.AndroidBrokens);

                GUILayout.Label($"{network.InstalledAndroidVersion}", GUILayout.Width(COLUMN_WIDTH));
                GUILayout.Label($"{network.AndroidBuild}", GUILayout.Width(COLUMN_WIDTH));
                GUILayout.Label($"{network.GetAndroidBrokensString()}", GUILayout.Width(COLUMN_WIDTH));
                
                GUI.color = originalColor;

                GUI.color = GetInstalledVersionColor(network.InstalledIosVersion, network.IOSBuild, network.IOSBrokens);

                GUILayout.Label($"{network.InstalledIosVersion}", GUILayout.Width(COLUMN_WIDTH));
                GUILayout.Label($"{network.IOSBuild}", GUILayout.Width(COLUMN_WIDTH));
                GUILayout.Label($"{network.GetIOSBrokensString()}", GUILayout.Width(COLUMN_WIDTH));
                
                GUI.color = originalColor;


                EditorGUILayout.EndHorizontal();

                rowColorIndex++;
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            if (GUILayout.Button("Update Adapters to QA Stable Versions"))
            {
                FixInstalledAdNetworkVersions();
            }
            if (GUILayout.Button("Update privacy links"))
            {
                LionMaxAdapterStabiliser.AddPrivacyLinks();
            }
        }

        private Color GetInstalledVersionColor(Version installed, Version lastStable, List<Version> brokens)
        {
            if (brokens != null && brokens.Contains(installed))
                return Color.red;
            if (installed == lastStable)
                return Color.green;
            if (installed >= lastStable)
                return Color.yellow;
            return Color.red;
        }

        private void FixInstalledAdNetworkVersions()
        {
            LionMaxAdapterStabiliser.LoadInstalledNetworkVersions(true);
        }

        private async void Refresh()
        {
            await LionMaxAdapterStabiliser.InitAdNetworkData();
            Repaint();
        }
    }
}


