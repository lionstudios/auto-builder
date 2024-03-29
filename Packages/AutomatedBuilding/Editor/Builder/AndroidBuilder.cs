using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace LionStudios.Editor.AutoBuilder
{
    public class AndroidBuilder : Builder
    {
        protected const string ANDROID_BUILD_LOCATION = "builds/android";

        protected static readonly string ANDROID_SETTINGS_PATH = $"{SETTINGS_PATH}/AndroidBuildSettings.asset";

        private static AndroidBuildSettings _androidBuildSettings;

        protected static AndroidBuildSettings AndroidBuildSettings
        {
            get
            {
                if (_androidBuildSettings == null)
                    _androidBuildSettings = AssetDatabase.LoadAssetAtPath<AndroidBuildSettings>(ANDROID_SETTINGS_PATH);
                return _androidBuildSettings;
            }
        }

        protected override string BuildLocation => ANDROID_BUILD_LOCATION;
        protected override ScriptingImplementation ScriptingImplementation => ScriptingImplementation.IL2CPP;
        protected override BuildTargetGroup BuildTargetGroup => BuildTargetGroup.Android;

        public AndroidBuilder(ICMDArgsProvider cmdArgsProvider) : base(cmdArgsProvider)
        {
        }

        ~AndroidBuilder()
        {
            _androidBuildSettings = null;
        }

        protected override BuildPlayerOptions InitializeSpecific(IDictionary<string, string> cmdParamsMap, bool isProduction)
        {
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            }

            var bundleVersion = !string.IsNullOrEmpty(cmdParamsMap["versionNumber"])
                ? cmdParamsMap["versionNumber"]
                : PlayerSettings.bundleVersion;

            PlayerSettings.bundleVersion = bundleVersion;

            if (cmdParamsMap.TryGetValue("buildNumber", out var bundleVersionCode) &&
                int.TryParse(bundleVersionCode, out var bundleVersionCodeVal))
            {
                PlayerSettings.Android.bundleVersionCode = bundleVersionCodeVal;
            }

            EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            EditorUserBuildSettings.buildAppBundle = isProduction;
#if UNITY_2019_4
            EditorUserBuildSettings.androidCreateSymbolsZip = true;

#elif UNITY_2021_1_OR_NEWER
            EditorUserBuildSettings.androidCreateSymbols = AndroidCreateSymbols.Public;
#endif
            PlayerSettings.Android.useCustomKeystore = isProduction;
            SetSigningKeys();

            var buildName = GetBuildName(cmdParamsMap);
            buildName = isProduction ? $"{buildName}.aab" : $"{buildName}.apk";
            var locationPathName = Path.Combine(BuildLocation, cmdParamsMap["environment"], buildName);

            var buildPlayerOptions = new BuildPlayerOptions
            {
                locationPathName = locationPathName,
                scenes = GetScenes(),
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            return buildPlayerOptions;
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

            if (cmdParamsMap.TryGetValue("buildNumber", out var buildNumber))
            {
                return $"{Application.productName}-{DateTime.Now.ToString(DATE_FORMAT)}-{buildNumber}";
            }

            return $"{Application.productName}-{DateTime.Now.ToString(DATE_FORMAT)}";
        }

        private static void SetSigningKeys()
        {
            PlayerSettings.Android.keystoreName = AndroidBuildSettings.KeystorePath;
            PlayerSettings.Android.keyaliasName = AndroidBuildSettings.KeystoreAlias;
            PlayerSettings.Android.keystorePass = CryptoHelper.Decrypt(AndroidBuildSettings.KeystorePassword);
            PlayerSettings.Android.keyaliasPass = CryptoHelper.Decrypt(AndroidBuildSettings.KeystoreAliasPassword);
        }

        [InitializeOnLoadMethod]
        private static void SetupProject()
        {
            if (AssetDatabase.LoadAllAssetsAtPath(ANDROID_SETTINGS_PATH).Length == 0)
            {
                Directory.CreateDirectory(SETTINGS_PATH);
                AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<AndroidBuildSettings>(), ANDROID_SETTINGS_PATH);
            }
        }
    }
}