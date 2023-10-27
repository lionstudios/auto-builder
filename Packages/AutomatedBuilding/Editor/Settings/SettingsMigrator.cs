using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using LionStudios.Suite.Core;
using System.Linq;
using LionStudios.Suite.Editor;
using EditorUtility = UnityEditor.EditorUtility;

namespace LionStudios.Editor.AutoBuilder
{
    public class SettingsMigrator
    {
        
        const string SETTINGS_PATH_BASE = "Assets/LionStudios/AutomatedBuilding";
        
        static readonly string SETTINGS_PATH = $"{SETTINGS_PATH_BASE}/Editor";

        static readonly string FAKE_CMD_ARGS_PATH = $"{SETTINGS_PATH}/FakeCMDArgs.asset";
        static readonly string COMMON_SETTINGS_PATH = $"{SETTINGS_PATH}/CommonBuildSettings.asset";
        static readonly string IOS_SETTINGS_PATH = $"{SETTINGS_PATH}/IOSBuildSettings.asset";
        static readonly string ANDROID_SETTINGS_PATH = $"{SETTINGS_PATH}/AndroidBuildSettings.asset";

        [InitializeOnLoadMethod]
        static void OnLoad()
        {
            EditorApplication.delayCall += CheckForOldSettings;
        }
        
        static void CheckForOldSettings()
        {
            if (AssetDatabase.LoadAssetAtPath<LionStudios.Editor.AutoBuilder.Legacy.FakeCMDArgsProvider>(FAKE_CMD_ARGS_PATH) != null
                || AssetDatabase.LoadAssetAtPath<LionStudios.Editor.AutoBuilder.Legacy.CommonBuildSettings>(COMMON_SETTINGS_PATH) != null
                || AssetDatabase.LoadAssetAtPath<LionStudios.Editor.AutoBuilder.Legacy.IOSBuildSettings>(IOS_SETTINGS_PATH) != null
                || AssetDatabase.LoadAssetAtPath<LionStudios.Editor.AutoBuilder.Legacy.AndroidBuildSettings>(ANDROID_SETTINGS_PATH) != null)
            {
                bool showDialog = !SessionState.GetBool("Lion_BuildSettingsMigrationOptOut", false);
                if (showDialog)
                {
                    int result = EditorUtility.DisplayDialogComplex("Outdated Build Settings Files",
                        $"You have deprecated settings files in \n {SETTINGS_PATH_BASE}.\n\n" +
                        $"Build Settings are now at \n LionStudios -> Settings Manager -> Builder.\n\n" +
                        $"Migrate the old settings and delete the deprecated files?",
                        "Migrate",
                        "Ignore",
                        "Ignore - Do not ask again for this session");
                    if (result == 0)
                    {
                        Migrate();
                    }
                    else if (result == 2)
                    {
                        SessionState.SetBool("Lion_BuildSettingsMigrationOptOut", true);
                    }
                }
            }
        }
        
        public static void Migrate()
        {
            LionSettingsService.InitializeService();

            AutoBuilderSettingsProvider settingsProvider = LionSettingsService.GetAllProviders().FirstOrDefault(x => x is AutoBuilderSettingsProvider) as AutoBuilderSettingsProvider;
            AutoBuilderSettings settings = settingsProvider.GetSettings() as AutoBuilderSettings;

            var fakeCMDArgsProvider = AssetDatabase.LoadAssetAtPath<LionStudios.Editor.AutoBuilder.Legacy.FakeCMDArgsProvider>(FAKE_CMD_ARGS_PATH);
            if(fakeCMDArgsProvider != null)
            {
                string jsonString = JsonUtility.ToJson(fakeCMDArgsProvider); 
                settings.local = JsonUtility.FromJson<FakeCMDArgsProvider>(jsonString); 
                //AssetDatabase.DeleteAsset(FAKE_CMD_ARGS_PATH);
            }

            var commonSettings = AssetDatabase.LoadAssetAtPath<LionStudios.Editor.AutoBuilder.Legacy.CommonBuildSettings>(COMMON_SETTINGS_PATH); 
            if (commonSettings != null)
            {
                string jsonString = JsonUtility.ToJson(commonSettings);
                settings.common = JsonUtility.FromJson<CommonBuildSettings>(jsonString);
                //AssetDatabase.DeleteAsset(COMMON_SETTINGS_PATH);
            }

            var iosSettings = AssetDatabase.LoadAssetAtPath<LionStudios.Editor.AutoBuilder.Legacy.IOSBuildSettings>(IOS_SETTINGS_PATH);
            if (iosSettings != null)
            {
                string jsonString = JsonUtility.ToJson(iosSettings);
                settings.iOS = JsonUtility.FromJson<IOSBuildSettings>(jsonString);
                //AssetDatabase.DeleteAsset(IOS_SETTINGS_PATH);
            }

            var androidSettings = AssetDatabase.LoadAssetAtPath<LionStudios.Editor.AutoBuilder.Legacy.AndroidBuildSettings>(ANDROID_SETTINGS_PATH);
            if (androidSettings != null)
            {
                string jsonString = JsonUtility.ToJson(androidSettings);
                settings.android = JsonUtility.FromJson<AndroidBuildSettings>(jsonString);
                //AssetDatabase.DeleteAsset(ANDROID_SETTINGS_PATH);
            }
            AssetDatabase.DeleteAsset(SETTINGS_PATH_BASE);
            settingsProvider.ApplySettings(settings); 
            EditorUtility.SetDirty(settings); 
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            LionSettingsManagerWindow.OpenManagerWindowAtTab("Builder");
        }
    }
}
