using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

#if UNITY_IOS
using UnityEditor.iOS.Xcode;
using UnityEditor.iOS.Xcode.Extensions;
#endif

namespace LionStudios.Editor.AutoBuilder
{

    public class IOSBuilder : Builder
    {
        protected const string IOS_BUILD_LOCATION = "builds/ios";

        protected static readonly string IOS_SETTINGS_PATH = $"{SETTINGS_PATH}/IOSBuildSettings.asset";

        private static IOSBuildSettings _iosBuildSettings;
        private static IOSBuildSettings iosBuildSettings
        {
            get
            {
                if (_iosBuildSettings == null)
                    _iosBuildSettings = AssetDatabase.LoadAssetAtPath<IOSBuildSettings>(IOS_SETTINGS_PATH);
                return _iosBuildSettings;
            }
        }

        protected override string BuildLocation => IOS_BUILD_LOCATION;
        protected override ScriptingImplementation ScriptingImplementation => ScriptingImplementation.IL2CPP;
        protected override BuildTargetGroup BuildTargetGroup => BuildTargetGroup.iOS;

        private const string EXPORT_OPTIONS_PATH = "buildTools/";
        private const string PROVISIONING_PROFILE_PATH = "ProvisioningProfiles/";

        public IOSBuilder(ICMDArgsProvider cmdArgsProvider) : base(cmdArgsProvider) { }
        
        private static string oneSignalProductIdentifier;

        ~IOSBuilder()
        {
            _iosBuildSettings = null;
        }

        protected override BuildPlayerOptions InitializeSpecific(IDictionary<string, string> cmdParamsMap, bool isProduction)
        {
#if UNITY_IOS
            CreateExportPlist();
#endif
            
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.iOS)
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS, BuildTarget.iOS);
            }

            var buildNumber = !string.IsNullOrEmpty(cmdParamsMap["buildNumber"])
                ? cmdParamsMap["buildNumber"]
                : PlayerSettings.bundleVersion;

            if (!string.IsNullOrEmpty(cmdParamsMap["versionNumber"]))
            {
                PlayerSettings.bundleVersion = cmdParamsMap["versionNumber"];
            }

            PlayerSettings.iOS.buildNumber = buildNumber;

            var locationPathName = Path.Combine(BuildLocation, cmdParamsMap["environment"], cmdParamsMap["buildName"]);
            var buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = GetScenes(),
                locationPathName = locationPathName,
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
            if (AssetDatabase.LoadAllAssetsAtPath(IOS_SETTINGS_PATH).Length == 0)
            {
                Directory.CreateDirectory(SETTINGS_PATH);
                AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<IOSBuildSettings>(), IOS_SETTINGS_PATH);
            }
            
            if (!Directory.Exists(PROVISIONING_PROFILE_PATH))
                Directory.CreateDirectory(PROVISIONING_PROFILE_PATH);
        }

#if UNITY_IOS

        [PostProcessBuild(int.MaxValue)]
        public static void OnPostProcessBuild(BuildTarget buildTarget, string path) //copied from KingWing the way it is
        {

            Debug.Log($"<---------->Path > {path}");

            if (buildTarget != BuildTarget.iOS)
            {
                return;
            }

            char separator = Path.DirectorySeparatorChar;

            string pbxPath = PBXProject.GetPBXProjectPath(path);
            string targetName = iosBuildSettings.TargetName;
            string entitlementFileName = $"{targetName}.entitlements";
            string entitlementFileRelativePath = $"{targetName}{separator}{entitlementFileName}";
            string entitlementFilePath = $"{path}{separator}{entitlementFileRelativePath}";

            #region Capability Manager Operations

            var capabilityManager = new ProjectCapabilityManager(pbxPath, entitlementFileRelativePath, targetName);
            if (iosBuildSettings.RemoteNotifications)
            {
                capabilityManager.AddPushNotifications(true);
                capabilityManager.AddBackgroundModes(BackgroundModesOptions.RemoteNotifications);
            }

            if (iosBuildSettings.inAppPurchase)
            {
                capabilityManager.AddInAppPurchase();
            }

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

        proj.SetBuildProperty(mainTargetGuid, "CODE_SIGN_IDENTITY", $"Apple Distribution: {iosBuildSettings.OrgName} ({iosBuildSettings.OrgTeamId})");
        proj.SetBuildProperty(mainTargetGuid, "CODE_SIGN_IDENTITY[sdk=iphoneos*]", $"Apple Distribution: {iosBuildSettings.OrgName} ({iosBuildSettings.OrgTeamId})");

            proj.SetBuildProperty(mainTargetGuid, "USYM_UPLOAD_AUTH_TOKEN", "placeholder-Unity-iPhone");

            // Needs to be called twice in a row for XCode to take it into account. Do not remove.
            for (int i = 0; i < 2; i++)
            {
            proj.SetBuildProperty(mainTargetGuid, "PROVISIONING_PROFILE_SPECIFIER", iosBuildSettings.ProvisioningProfileName);
            proj.SetBuildProperty(mainTargetGuid, "PROVISIONING_PROFILE_APP", iosBuildSettings.ProvisioningProfileName);
            proj.SetBuildProperty(mainTargetGuid, "PROVISIONING_PROFILE", iosBuildSettings.ProvisioningProfileName);
            }

            proj.SetBuildProperty(mainTargetGuid, "DEVELOPMENT_TEAM", iosBuildSettings.OrgTeamId);


            if (iosBuildSettings.usingOneSignal)
            {
                // Set Manual Provisioning Profiles One Signal Notification Service Extension
                string oneSignalTargetGuid = proj.TargetGuidByName("OneSignalNotificationServiceExtension");
                proj.SetBuildProperty(oneSignalTargetGuid, "ENABLE_BITCODE", "NO");
                proj.SetBuildProperty(oneSignalTargetGuid, "PRODUCT_BUNDLE_IDENTIFIER", oneSignalProductIdentifier);
                proj.AddBuildProperty(oneSignalTargetGuid, "CODE_SIGN_STYLE", "Manual");
            proj.SetBuildProperty(oneSignalTargetGuid, "CODE_SIGN_IDENTITY", $"Apple Distribution: {iosBuildSettings.OrgName} ({iosBuildSettings.OrgTeamId})");
            proj.SetBuildProperty(oneSignalTargetGuid, "CODE_SIGN_IDENTITY[sdk=iphoneos*]", $"Apple Distribution: {iosBuildSettings.OrgName} ({iosBuildSettings.OrgTeamId})");

            proj.SetBuildProperty(oneSignalTargetGuid, "PROVISIONING_PROFILE_SPECIFIER", iosBuildSettings.oneSignalProvisionalProfileName);
            proj.SetBuildProperty(oneSignalTargetGuid, "PROVISIONING_PROFILE_SPECIFIER", iosBuildSettings.oneSignalProvisionalProfileName);
                proj.SetBuildProperty(oneSignalTargetGuid, "DEVELOPMENT_TEAM", iosBuildSettings.OrgTeamId);
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

        }

        #region ExportPlist Methods

        public static void CreateExportPlist()
        {
            
            
            bool directoryExists = Directory.Exists(EXPORT_OPTIONS_PATH);

            if (!directoryExists)
            {
                Directory.CreateDirectory(EXPORT_OPTIONS_PATH);
            }

            bool fileExists = File.Exists(EXPORT_OPTIONS_PATH + "exportOptions.plist");
            var document = new PlistDocument();
            if (fileExists)
            {
                document.ReadFromFile(EXPORT_OPTIONS_PATH + "exportOptions.plist");
            }
            else
            {
                document = new PlistDocument();
                document.Create();
                document.WriteToFile(EXPORT_OPTIONS_PATH + "exportOptions.plist");
            }

            var rootDict = document.root;
            rootDict.SetString("teamID", iosBuildSettings.OrgTeamId);
            rootDict.SetString("method", "app-store");
            rootDict.SetBoolean("uploadSymbols", true);
            rootDict.CreateDict("provisioningProfiles");

            var provisionalDictionary = rootDict.values["provisioningProfiles"].AsDict();

            #region ProvisionalProfile Logic
            
            string OrgString = iosBuildSettings.OrgTeamId + ".";
            string UUIDKey_MobileProvisional = "UUID";
            string IdentifierKey_MobileProvisional = "application-identifier";
            string UUID = GetValueFromProvisionalProfile(iosBuildSettings.ProvisioningProfileName, UUIDKey_MobileProvisional);
            string ApplicationIdentifier = GetValueFromProvisionalProfile(iosBuildSettings.ProvisioningProfileName, IdentifierKey_MobileProvisional);
            ApplicationIdentifier = ApplicationIdentifier.Replace(OrgString, "");
            provisionalDictionary.SetString(ApplicationIdentifier, UUID);
            if (iosBuildSettings.usingOneSignal)
            {
                if (!string.IsNullOrEmpty(iosBuildSettings.oneSignalProvisionalProfileName))
                {
                    string UUIDOneSignal = GetValueFromProvisionalProfile(iosBuildSettings.oneSignalProvisionalProfileName, UUIDKey_MobileProvisional);
                    string ApplicationIdentifierOneSignal = GetValueFromProvisionalProfile(iosBuildSettings.oneSignalProvisionalProfileName, IdentifierKey_MobileProvisional);
                    if (ApplicationIdentifierOneSignal.Contains(OrgString))
                    {
                        ApplicationIdentifierOneSignal = ApplicationIdentifierOneSignal.Replace(OrgString, "");
                        oneSignalProductIdentifier = ApplicationIdentifierOneSignal;
                    }
                    provisionalDictionary.SetString(ApplicationIdentifierOneSignal, UUIDOneSignal);
                }
            }
            #endregion

            document.WriteToFile(EXPORT_OPTIONS_PATH + "exportOptions.plist");
        }

        private static string GetValueFromProvisionalProfile(string ProvisionalProfileName, string KeyString)
        {
            string UUIDValue = "";
            bool fileExists = File.Exists(PROVISIONING_PROFILE_PATH + ProvisionalProfileName + ".mobileprovision");
            if (fileExists)
            {
                string[] data = File.ReadAllLines(PROVISIONING_PROFILE_PATH + ProvisionalProfileName + ".mobileprovision");
                string UUIDLineValue = "";
                for (int i = 0; i < data.Length; i++)
                {
                    if (data[i].Contains(KeyString))
                    {
                        UUIDLineValue = data[i + 1];
                    }
                }

                if (!string.IsNullOrEmpty(UUIDLineValue))
                {
                    string regularExpressionPattern1 = @"<string>(.*?)<\/string>";
                    Regex regex = new Regex(regularExpressionPattern1, RegexOptions.Singleline);
                    MatchCollection collection = regex.Matches(UUIDLineValue);
                    Match m = collection[0];
                    UUIDValue = m.Groups[1].Value;
                }
            }
            else
            {
                Debug.LogError($"Put ProvisionalProfile Named {iosBuildSettings.ProvisioningProfileName} in {Application.dataPath + "/" + PROVISIONING_PROFILE_PATH}");
            }

            return UUIDValue;
        }

        #endregion

        #region Ios Support Methods

        public static HashSet<string> GetSKAdNetworkIds()
        {
            HashSet<string> skAdNetworkIdsHashSet = new HashSet<string>();

            string[] skAdNetworkIdsArr = File.ReadAllLines($"{SETTINGS_PATH}/SKAdNetworkIds.txt");
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
                    Debug.Log($"Couldn't find a SKAdNetworkId in: {skAdNetworkId}");
                }
            }

            return skAdNetworkIdsHashSet;
        }

        #endregion
        
#endif // UNITY_IOS
        
    }

}