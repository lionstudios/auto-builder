using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

public class LionMaxAdapterServiceWindow : EditorWindow
{
    List<AdNetwork> adNetworks;
    [MenuItem("LionStudios/Adapter Service")]
    public static void ShowWindow()
    {
        var window = GetWindow<LionMaxAdapterServiceWindow>();
        window.titleContent = new GUIContent("Lion Max Adapter Service");
        window.minSize = new Vector2(1000, 1200);
        window.maxSize = new Vector2(1000, 1200);
    }

    private void OnEnable()
    {
        adNetworks = LionMaxAdapterStabiliser.AdNetworks;
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
        if(adNetworks == null)
        {
            GUILayout.Label("Couldn't fetch stable ad networks. Check your internet connection.", EditorStyles.boldLabel);

            return;
        }
        EditorGUILayout.Space();

        GUILayout.Label("Installed Ad Networks", EditorStyles.boldLabel);

        EditorGUILayout.Space();

        EditorGUILayout.BeginVertical(GUI.skin.box);


        EditorGUILayout.BeginHorizontal();

        GUILayout.Label("Network Name", GUILayout.Width(150));
        GUILayout.Label("Code Name", GUILayout.Width(150));

        GUILayout.Label($"Android Installed", GUILayout.Width(150));
        GUILayout.Label($"Android Stable", GUILayout.Width(150));

        GUILayout.Label($"iOS Installed", GUILayout.Width(150));
        GUILayout.Label($"iOS Stable", GUILayout.Width(150));

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        Color originalColor = GUI.color;

        Color[] colors = new Color[] { new Color(1.0f, 1.0f, 1.0f, 0.05f), new Color(1.0f, 1.0f, 1.0f, 0.1f) };
        GUIStyle gsTest = new GUIStyle();

        int rowColorIndex = 0;
        foreach (var network in adNetworks.OrderBy(x => x.NetworkName))
        { 
            if(string.IsNullOrEmpty(network.InstalledAndroidVersion) && string.IsNullOrEmpty(network.InstalledIosVersion))
            {
                continue;
            }
            gsTest.normal.background = MakeTex(600, 1, colors[rowColorIndex % 2]);


            EditorGUILayout.BeginHorizontal(gsTest);

            GUILayout.Label(network.NetworkName, GUILayout.Width(150));
            GUILayout.Label(network.NetworkCodeName, GUILayout.Width(150));

            if(string.IsNullOrEmpty(network.AndroidBuild))
            {
                GUI.color = originalColor;
            }
            else if (network.InstalledAndroidVersion == network.AndroidBuild)
            {
                GUI.color = Color.green;
            }
            else if (new Version(network.InstalledAndroidVersion) >= new Version(network.AndroidBuild))
            {
                GUI.color = Color.yellow;
            }
            else
            {
                GUI.color = Color.red;
            }

            GUILayout.Label($"{network.InstalledAndroidVersion}", GUILayout.Width(150));
            GUILayout.Label($"{network.AndroidBuild}", GUILayout.Width(150));

            GUI.color = originalColor;

            if (string.IsNullOrEmpty(network.IOSBuild))
            {
                GUI.color = originalColor;
            }
            else if (network.InstalledIosVersion == network.IOSBuild)
            {
                GUI.color = Color.green;
            }
            else if (new Version(network.InstalledIosVersion) >= new Version(network.IOSBuild))
            {
                GUI.color = Color.yellow;
            }
            else
            {
                GUI.color = Color.red;
            }

            GUILayout.Label($"{network.InstalledIosVersion}", GUILayout.Width(150));
            GUILayout.Label($"{network.IOSBuild}", GUILayout.Width(150));

            GUI.color = originalColor;

            EditorGUILayout.EndHorizontal();

            rowColorIndex++;
        }

        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        if (GUILayout.Button("Update All Adapters to QA Stable Versions"))
        {
            FixInstalledAdNetworkVersions();
        }
    }

    private void FixInstalledAdNetworkVersions()
    {
        LionMaxAdapterStabiliser.LoadInstalledNetworkVersions(true);
    }
}


