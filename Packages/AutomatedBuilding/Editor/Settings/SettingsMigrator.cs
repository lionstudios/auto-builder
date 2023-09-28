using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using LionStudios.Suite.Core;
using System.Linq;

namespace LionStudios.Editor.AutoBuilder
{
    public class SettingsMigrator
    {
        const string SETTINGS_PATH = "Assets/LionStudios/AutomatedBuilding/Editor";

        static readonly string FAKE_CMD_ARGS_PATH = $"{SETTINGS_PATH}/FakeCMDArgs.asset";//
        static readonly string COMMON_SETTINGS_PATH = $"{SETTINGS_PATH}/CommonBuildSettings.asset";
        static readonly string IOS_SETTINGS_PATH = $"{SETTINGS_PATH}/IOSBuildSettings.asset";
        static readonly string ANDROID_SETTINGS_PATH = $"{SETTINGS_PATH}/AndroidBuildSettings.asset";

        //[InitializeOnLoadMethod]
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
            settingsProvider.ApplySettings(settings); 
            EditorUtility.SetDirty(settings); 
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
