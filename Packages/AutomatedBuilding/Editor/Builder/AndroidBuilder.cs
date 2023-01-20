using System;
using System.Collections.Generic;
using System.IO;
using AutomatedBuilding;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class AndroidBuilder : Builder
{
    
    protected static readonly string ANDROID_SETTINGS_PATH = $"{SETTINGS_PATH}/AndroidBuildSettings.asset";
    
    private static AndroidBuildSettings _androidBuildSettings;
    
    protected override string DefaultBuildFolder => _androidBuildSettings.DefaultBuildFolder;
    protected override string DefineSymbols => _androidBuildSettings.AdditionalDefineSymbols;
    protected override ScriptingImplementation ScriptingImplementation => ScriptingImplementation.IL2CPP;
    protected override BuildTargetGroup BuildTargetGroup => BuildTargetGroup.Android;
    
    public AndroidBuilder(ICMDArgsProvider cmdArgsProvider) : base(cmdArgsProvider)
    {
        _androidBuildSettings = AssetDatabase.LoadAssetAtPath<AndroidBuildSettings>(ANDROID_SETTINGS_PATH);
    }

    ~AndroidBuilder()
    {
        _androidBuildSettings = null;
    }

    protected override BuildPlayerOptions InitializeSpecific(IDictionary<string, string> cmdParamsMap, bool isProduction, 
        string buildLocation)
    {
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        }

        var bundleVersion = !string.IsNullOrEmpty(cmdParamsMap["versionNumber"])
            ? cmdParamsMap["versionNumber"] : PlayerSettings.bundleVersion;

        PlayerSettings.bundleVersion = bundleVersion;

        if (cmdParamsMap.TryGetValue("buildNumber", out var bundleVersionCode) && 
            int.TryParse(bundleVersionCode, out var bundleVersionCodeVal))
        {
            PlayerSettings.Android.bundleVersionCode = bundleVersionCodeVal;
        }
        
        EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        EditorUserBuildSettings.buildAppBundle = isProduction;
        PlayerSettings.Android.useCustomKeystore = isProduction;
        SetSigningKeys(isProduction);
        
        var buildName = GetBuildName(cmdParamsMap);
        buildName = isProduction ? $"{buildName}.aab" : $"{buildName}.apk";
        var locationPathName = Path.Combine(buildLocation, buildName);

        var buildPlayerOptions = new BuildPlayerOptions
        {
            locationPathName = locationPathName,
            scenes = GetScenes(),
            target = BuildTarget.Android,
            options = BuildOptions.None
        };
        
        return buildPlayerOptions;
    }

    protected override void DeleteOldBuilds(string path, int capacity = 5)
    {
        KeepLeastRecentlyUsedFiles(path, "*.apk", capacity);
        KeepLeastRecentlyUsedFiles(path, "*.aab", capacity);
        KeepLeastRecentlyUsedFiles(path, "*.symbols.zip", capacity);
    }

    protected override void CreateBuildDirectory(string path)
    {
        var pathToProject = Directory.GetParent(Application.dataPath)?.FullName;

        if (!string.IsNullOrEmpty(pathToProject))
        {
            Directory.CreateDirectory(Path.Combine(pathToProject, path));
        }
    }

    private static string GetBuildName(IDictionary<string, string> cmdParamsMap)
    {
        if (cmdParamsMap.TryGetValue("buildName", out var buildName))
        {
            return buildName;
        }

        if (!string.IsNullOrEmpty(cmdParamsMap["buildNumber"]))
        {
            buildName = $"{CommonBuildSettings.ProjectName}-{DateTime.Now.ToString(CommonBuildSettings.DateFormat)}";
        }
        else
        {
            buildName = $"{CommonBuildSettings.ProjectName}-{DateTime.Now.ToString(CommonBuildSettings.DateFormat)}-{cmdParamsMap["buildNumber"]}";
        }

        return buildName;
    }

    private static void SetSigningKeys(bool isProduction)
    {
        if (!isProduction)
        {
            return;
        }
        
        PlayerSettings.Android.keystoreName = _androidBuildSettings.KEY_STORE_PATH;
        PlayerSettings.Android.keyaliasName = _androidBuildSettings.KEY_STORE_ALIAS;
        PlayerSettings.Android.keystorePass = CryptoHelper.Decrypt(_androidBuildSettings.KEY_STORE_PASS);
        PlayerSettings.Android.keyaliasPass = CryptoHelper.Decrypt(_androidBuildSettings.KEY_STORE_ALIAS_PASS);
    }

    private static void KeepLeastRecentlyUsedFiles(string path, string extension, int capacity)
    {
        var heap = new SortedDictionary<DateTime, string>();
        var files = Directory.GetFiles(path, extension, SearchOption.AllDirectories);

        foreach (var file in files)
        {
            heap[File.GetCreationTimeUtc(file)] = file;
        }

        var numToDelete = heap.Count - capacity;

        foreach (var kvp in heap)
        {
            if (numToDelete <= 0)
            {
                return;
            }
            
            File.Delete(kvp.Value);
            numToDelete--;
        }
    }
    
    [InitializeOnLoadMethod]
    private static void SetupProject()
    {
        if (AssetDatabase.LoadAssetAtPath<AndroidBuildSettings>(ANDROID_SETTINGS_PATH) == null)
        {
            Directory.CreateDirectory(SETTINGS_PATH);
            AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<AndroidBuildSettings>(), ANDROID_SETTINGS_PATH);
        }
    }

}