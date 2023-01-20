using System;
using System.Collections.Generic;
using System.IO;
using AutomatedBuilding;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.Callbacks;
//using UnityEditor.iOS.Xcode;
using UnityEngine;

public class IOSBuilder : Builder
{
    
    protected static readonly string IOS_SETTINGS_PATH = $"{SETTINGS_PATH}/IOSBuildSettings.asset";
    
    private static IOSBuildSettings _iosBuildSettings;

    protected override string DefaultBuildFolder => _iosBuildSettings.DefaultBuildFolder;
    protected override string DefineSymbols => _iosBuildSettings.AdditionalDefineSymbols;
    protected override ScriptingImplementation ScriptingImplementation => ScriptingImplementation.IL2CPP;
    protected override BuildTargetGroup BuildTargetGroup => BuildTargetGroup.iOS;

    public IOSBuilder(ICMDArgsProvider cmdArgsProvider) : base(cmdArgsProvider)
    {
        _iosBuildSettings = AssetDatabase.LoadAssetAtPath<IOSBuildSettings>(IOS_SETTINGS_PATH);
    }

    ~IOSBuilder()
    {
        _iosBuildSettings = null;
    }

    protected override string GetBuildLocation(IDictionary<string, string> cmdParamsMap)
    { 
        var parentDir = base.GetBuildLocation(cmdParamsMap);
        var buildPath = Path.Combine(parentDir, cmdParamsMap["buildName"]);

        return buildPath;
    }

    protected override BuildPlayerOptions InitializeSpecific(IDictionary<string, string> cmdParamsMap, bool isProduction, string buildLocation)
    {
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.iOS)
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS, BuildTarget.iOS);
        }

        var buildNumber = !string.IsNullOrEmpty(cmdParamsMap["buildNumber"])
            ? cmdParamsMap["buildNumber"] : PlayerSettings.bundleVersion;
        
        if (!string.IsNullOrEmpty(cmdParamsMap["versionNumber"]))
        {
            PlayerSettings.bundleVersion = cmdParamsMap["versionNumber"];
        }

        PlayerSettings.iOS.buildNumber = buildNumber;
        
        var buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = CommonBuildSettings.Scenes,
            locationPathName = buildLocation,
            target = BuildTarget.iOS,
            options = BuildOptions.None
        };

        return buildPlayerOptions;
    }

    protected override void DeleteOldBuilds(string path, int capacity = 5)
    {
        KeepLeastRecentlyUsedFolders(path, capacity);
    }
    
    protected override void CreateBuildDirectory(string path)
    {
        var pathToProject = Directory.GetParent(Application.dataPath)?.FullName;

        if (string.IsNullOrEmpty(pathToProject))
        {
            return;
        }
        
        var buildPath = Path.Combine(pathToProject, path);
        var buildPathDi = new DirectoryInfo(buildPath);
            
        if (buildPathDi.Parent != null)
        {
            Directory.CreateDirectory(buildPathDi.Parent.FullName);
        }
    }

    private static void KeepLeastRecentlyUsedFolders(string path, int capacity)
    {
        var directories = Directory.GetDirectories(path, "*BurstDebugInformation_DoNotShip",
            SearchOption.TopDirectoryOnly);

        foreach (var directory in directories)
        {
            Directory.Delete(directory, true);
        }

        var heap = new SortedDictionary<DateTime, string>();
        directories = Directory.GetDirectories(path, "*", SearchOption.TopDirectoryOnly);

        foreach (var directory in directories)
        {
            heap[File.GetCreationTimeUtc(directory)] = directory;
        }

        var numToDelete = heap.Count - capacity;

        foreach (var kvp in heap)
        {
            if (numToDelete <= 0)
            {
                return;
            }
            
            Directory.Delete(kvp.Value, true);
            numToDelete--;
        }
    }

    [InitializeOnLoadMethod]
    private static void SetupProject()
    {
        if (AssetDatabase.LoadAssetAtPath<IOSBuildSettings>(IOS_SETTINGS_PATH) == null)
        {
            Directory.CreateDirectory(SETTINGS_PATH);
            AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<IOSBuildSettings>(), IOS_SETTINGS_PATH);
        }
    }

    [PostProcessBuild(int.MaxValue)]
    public static void OnPostProcessBuild(BuildTarget buildTarget, string path) //copied from KingWing the way it is
    {
#if UNITY_IOS

        Debug.Log($"<---------->Path > {path}");

        if (buildTarget != BuildTarget.iOS)
        {
            return;
        }

        char separator = Path.DirectorySeparatorChar;

        string pbxPath = PBXProject.GetPBXProjectPath(path);
        string targetName = _iosBuildSettings.TargetName;
        string entitlementFileName = $"{targetName}.entitlements";
        string entitlementFileRelativePath = $"{targetName}{separator}{entitlementFileName}";
        string entitlementFilePath = $"{path}{separator}{entitlementFileRelativePath}";

        PBXProject proj = new PBXProject();
        proj.ReadFromString(File.ReadAllText(pbxPath));

#if UNITY_2019_3_OR_NEWER
        string targetGuid = proj.GetUnityFrameworkTargetGuid();
#else
        string targetGuid = proj.TargetGuidByName(targetName);
#endif

        // Disable bitcode
        proj.SetBuildProperty(targetGuid, "ENABLE_BITCODE", "NO");
        proj.AddBuildProperty(targetGuid, "CODE_SIGN_ENTITLEMENTS", entitlementFileRelativePath);


        // Fake Unity Auth Token (issue with command line build - won't let you upload to Unity Cloud, but will allow Archive)
        proj.SetBuildProperty(targetGuid, "USYM_UPLOAD_AUTH_TOKEN", "placeholder-XcodeProject");

        // Set Manual Provisioning Profiles Unity-iPhone
        string unityTargetGuid = proj.GetUnityMainTargetGuid();
        proj.SetBuildProperty(unityTargetGuid, "CODE_SIGN_STYLE", "Manual");
        proj.SetBuildProperty(unityTargetGuid, "CODE_SIGN_IDENTITY",
            $"Apple Distribution: {_iosBuildSettings.OrgName} ({_iosBuildSettings.OrgCode})");
        proj.SetBuildProperty(unityTargetGuid, "CODE_SIGN_IDENTITY[sdk=iphoneos*]",
            $"Apple Distribution: {_iosBuildSettings.OrgName} ({_iosBuildSettings.OrgCode})");
        proj.SetBuildProperty(unityTargetGuid, "PROVISIONING_PROFILE_SPECIFIER", _iosBuildSettings.PovisioningProfileName);
        proj.SetBuildProperty(unityTargetGuid, "PROVISIONING_PROFILE_APP", _iosBuildSettings.PovisioningProfileName);
        proj.SetBuildProperty(unityTargetGuid, "PROVISIONING_PROFILE", _iosBuildSettings.PovisioningProfileName);
        proj.SetBuildProperty(unityTargetGuid, "USYM_UPLOAD_AUTH_TOKEN", "placeholder-Unity-iPhone");

        // Set Manual Provisioning Profiles One Signal Notification Service Extension
        /*
        string oneSignalTargetGuid = proj.TargetGuidByName("OneSignalNotificationServiceExtension");
        proj.AddBuildProperty(oneSignalTargetGuid, "CODE_SIGN_STYLE", "Manual");
        proj.SetBuildProperty(oneSignalTargetGuid, "CODE_SIGN_IDENTITY", $"Apple Distribution: {_iosBuildSettings.OrgName} ({_iosBuildSettings.OrgCode})");
        proj.SetBuildProperty(oneSignalTargetGuid, "PROVISIONING_PROFILE_SPECIFIER", "Match 3D Distribution OneSignal");
        */

        // Add search paths
        //proj.SetBuildProperty(targetGuid, "LD_RUNPATH_SEARCH_PATHS", "$(inherited) @executable_path/Frameworks");

        File.WriteAllText(pbxPath, proj.WriteToString());
        proj.ReadFromString(File.ReadAllText(pbxPath));

        PlistDocument entitlements = new PlistDocument();
        if (File.Exists(entitlementFilePath)) entitlements.ReadFromFile(entitlementFilePath);
        if (entitlements.root["aps-environment"] == null)
            entitlements.root.SetString("aps-environment", "development");
        entitlements.WriteToFile(entitlementFilePath);
        proj.AddFile(entitlementFileRelativePath, entitlementFileName);

        proj.AddBuildProperty(targetGuid, "CODE_SIGN_ENTITLEMENTS", entitlementFileRelativePath);

        File.WriteAllText(pbxPath, proj.WriteToString());
#endif
    }
}