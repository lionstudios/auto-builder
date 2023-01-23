using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using AutomatedBuilding;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

#if UNITY_IOS
using UnityEditor.iOS.Xcode;
using UnityEditor.iOS.Xcode.Extensions;
#endif

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
            scenes = GetScenes(),
            locationPathName = buildLocation,
            target = BuildTarget.iOS,
            options = BuildOptions.None
        };

        return buildPlayerOptions;
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

        #region Capability Manager Operations

        var capabilityManager = new ProjectCapabilityManager(pbxPath, entitlementFileRelativePath, targetName);
        capabilityManager.AddPushNotifications(true);
        capabilityManager.AddInAppPurchase();
        capabilityManager.AddBackgroundModes(BackgroundModesOptions.RemoteNotifications);
        capabilityManager.WriteToFile();

        #endregion

        PBXProject proj = new PBXProject();
        proj.ReadFromString(File.ReadAllText(pbxPath));

        string mainTargetGuid = proj.GetUnityMainTargetGuid();

#if UNITY_2019_3_OR_NEWER
        string targetGuid = proj.GetUnityFrameworkTargetGuid();
#else
        string targetGuid = proj.TargetGuidByName(targetName);
#endif
        Debug.Log("mainTarget GUID" + mainTargetGuid);
        Debug.Log("target GUID" + targetGuid);

        #region Build Settings Operations

        // Disable bitcode
        proj.SetBuildProperty(targetGuid, "ENABLE_BITCODE", "NO");
        proj.SetBuildProperty(mainTargetGuid, "ENABLE_BITCODE", "NO");


        // Fake Unity Auth Token (issue with command line build - won't let you upload to Unity Cloud, but will allow Archive)
        proj.SetBuildProperty(targetGuid, "USYM_UPLOAD_AUTH_TOKEN", "placeholder-XcodeProject");

        // Set Manual Provisioning Profiles Unity-iPhone
        // string unityTargetGuid = proj.GetUnityMainTargetGuid();
        proj.SetBuildProperty(mainTargetGuid, "CODE_SIGN_STYLE", "Manual");

        proj.SetBuildProperty(mainTargetGuid, "CODE_SIGN_IDENTITY", $"Apple Distribution: {_iosBuildSettings.OrgName} ({_iosBuildSettings.OrgCode})");
        proj.SetBuildProperty(mainTargetGuid, "CODE_SIGN_IDENTITY[sdk=iphoneos*]", $"Apple Distribution: {_iosBuildSettings.OrgName} ({_iosBuildSettings.OrgCode})");

        proj.SetBuildProperty(mainTargetGuid, "USYM_UPLOAD_AUTH_TOKEN", "placeholder-Unity-iPhone");

        // Needs to be called twice in a row for XCode to take it into account. Do not remove.
        for (int i = 0; i < 2; i++)
        {
            proj.SetBuildProperty(mainTargetGuid, "PROVISIONING_PROFILE_SPECIFIER", _iosBuildSettings.ProvisioningProfileName);
            proj.SetBuildProperty(mainTargetGuid, "PROVISIONING_PROFILE_APP", _iosBuildSettings.ProvisioningProfileName);
            proj.SetBuildProperty(mainTargetGuid, "PROVISIONING_PROFILE", _iosBuildSettings.ProvisioningProfileName);
        }

        proj.SetBuildProperty(mainTargetGuid, "DEVELOPMENT_TEAM", _iosBuildSettings.OrgCode);


        if (_iosBuildSettings.usingOneSignal)
        {
            // Set Manual Provisioning Profiles One Signal Notification Service Extension
            string oneSignalTargetGuid = proj.TargetGuidByName("OneSignalNotificationServiceExtension");
            proj.SetBuildProperty(oneSignalTargetGuid, "ENABLE_BITCODE", "NO");
            proj.SetBuildProperty(oneSignalTargetGuid, "PRODUCT_BUNDLE_IDENTIFIER", _iosBuildSettings.oneSignalProductIdentifier);
            proj.AddBuildProperty(oneSignalTargetGuid, "CODE_SIGN_STYLE", "Manual");
            proj.SetBuildProperty(oneSignalTargetGuid, "CODE_SIGN_IDENTITY", $"Apple Distribution: {_iosBuildSettings.OrgName} ({_iosBuildSettings.OrgCode})");
            proj.SetBuildProperty(oneSignalTargetGuid, "CODE_SIGN_IDENTITY[sdk=iphoneos*]", $"Apple Distribution: {_iosBuildSettings.OrgName} ({_iosBuildSettings.OrgCode})");

            proj.SetBuildProperty(oneSignalTargetGuid, "PROVISIONING_PROFILE_SPECIFIER", _iosBuildSettings.oneSignalProvisionalProfileName);
            proj.SetBuildProperty(oneSignalTargetGuid, "PROVISIONING_PROFILE_SPECIFIER", _iosBuildSettings.oneSignalProvisionalProfileName);
            proj.SetBuildProperty(oneSignalTargetGuid, "DEVELOPMENT_TEAM", _iosBuildSettings.OrgCode);
        }

        // Add search paths
        proj.SetBuildProperty(targetGuid, "LD_RUNPATH_SEARCH_PATHS", "$(inherited) @executable_path/Frameworks");


        File.WriteAllText(pbxPath, proj.WriteToString());
        proj.ReadFromString(File.ReadAllText(pbxPath));

        PlistDocument entitlements = new PlistDocument();
        if (File.Exists(entitlementFilePath)) entitlements.ReadFromFile(entitlementFilePath);
        if (entitlements.root["aps-environment"] == null)
            entitlements.root.SetString("aps-environment", "development");
        entitlements.WriteToFile(entitlementFilePath);
        proj.AddFile(entitlementFileRelativePath, entitlementFileName);
        proj.AddBuildProperty(targetGuid, "CODE_SIGN_ENTITLEMENTS", entitlementFileRelativePath);

        proj.SetBuildProperty(targetGuid, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "NO");
        proj.SetBuildProperty(mainTargetGuid, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "YES");

        File.WriteAllText(pbxPath, proj.WriteToString());

        #endregion

        #region Info Plist Operations

        var plistPath = Path.Combine(path, "Info.plist");
        var plist = new PlistDocument();
        plist.ReadFromFile(plistPath);
        PlistElementDict rootDict = plist.root;
        rootDict.SetString("NSLocationWhenInUseUsageDescription", "Detect user location which is shown in leaderboard.");
        rootDict.SetString("NSLocationAlwaysUsageDescription", "Detect user location which is shown in leaderboard.");
        rootDict.SetString("NSUserTrackingUsageDescription", "This only uses device info for more interesting and relevant ads");
        rootDict.SetString("CFBundleShortVersionString", PlayerSettings.bundleVersion);
        rootDict.SetString("CFBundleVersion", PlayerSettings.iOS.buildNumber);

        PlistElementArray deviceCapabilitiesArray = rootDict.values["UIRequiredDeviceCapabilities"].AsArray();
        string metal = "metal";
        deviceCapabilitiesArray.values.RemoveAll(x => metal.Equals(x.AsString()));

        rootDict.SetBoolean("ITSAppUsesNonExemptEncryption", false);

        #region MAX IOS 14 SKAdNetwork Add

        try
        {
            HashSet<string> skAdNetworkIds = GetSKAdNetworkIds();

            if (skAdNetworkIds != null && skAdNetworkIds.Count > 0)
            {
                Debug.Log("Staring to add SKAddNetwork IDs!");

                if (rootDict.values.TryGetValue("SKAdNetworkItems", out PlistElement skAdNetworkItems))
                {
                    PlistElementArray skAdNetworkItemsArray;

                    HashSet<string> existingSKAdNetworkIds = new HashSet<string>();
                    if (skAdNetworkItems != null && skAdNetworkItems.GetType() == typeof(PlistElementArray))
                    {
                        skAdNetworkItemsArray = rootDict.values["SKAdNetworkItems"].AsArray();

                        var plistElementDictionaries = skAdNetworkItemsArray.values
                            .Where(plistElement => plistElement.GetType() == typeof(PlistElementDict));
                        foreach (var plistElement in plistElementDictionaries)
                        {
                            plistElement.AsDict().values.TryGetValue("SKAdNetworkIdentifier",
                                out PlistElement existingSKAdNetworkId);
                            if (existingSKAdNetworkId == null ||
                                existingSKAdNetworkId.GetType() != typeof(PlistElementString) ||
                                string.IsNullOrEmpty(existingSKAdNetworkId.AsString())) continue;

                            existingSKAdNetworkIds.Add(existingSKAdNetworkId.AsString());
                        }
                    }
                    else
                    {
                        skAdNetworkItemsArray = rootDict.CreateArray("SKAdNetworkItems");
                    }

                    foreach (var skAdNetworkId in skAdNetworkIds)
                    {
                        if (existingSKAdNetworkIds.Contains(skAdNetworkId)) continue;

                        PlistElementDict skAdNetworkItemDict = new PlistElementDict();
                        skAdNetworkItemDict.SetString("SKAdNetworkIdentifier", skAdNetworkId);
                        skAdNetworkItemsArray.values.Add(skAdNetworkItemDict);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("Could not add SKAdNetwork IDs!");
        }

        #endregion

        #endregion

        File.WriteAllText(plistPath, plist.WriteToString());

#endif
    }

    #region Ios Support Methods

    public static HashSet<string> GetSKAdNetworkIds()
    {
        HashSet<string> skAdNetworkIdsHashSet = new HashSet<string>();

        string[] skAdNetworkIdsArr = File.ReadAllLines("Assets/Editor/SKAdNetworkIds.txt");
        Regex skAdNetworkRegex = new Regex(@"^(.*skadnetwork).*$", RegexOptions.Multiline);

        foreach (var skAdNetworkId in skAdNetworkIdsArr)
        {
            var match = skAdNetworkRegex.Match(skAdNetworkId);

            if (match.Success)
            {
                skAdNetworkIdsHashSet.Add(match.Groups[1].Value);
            }
            else
            {
                Debug.Log(string.Format("Couldn't find a SKAdNetworkId in: {0}", skAdNetworkId));
            }
        }

        return skAdNetworkIdsHashSet;
    }

    #endregion
}