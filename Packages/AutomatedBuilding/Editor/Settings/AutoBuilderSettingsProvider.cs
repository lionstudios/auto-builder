using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LionStudios.Suite.Core;
using System;
using UnityEngine.Scripting;

[assembly: Preserve]
namespace LionStudios.Editor.AutoBuilder
{

    public class AutoBuilderSettingsProvider : ILionSettingsProvider
    {
        public string Name => "Builder";

        private int minAutoSaveInterval = 60;

        public ILionSettingsInfo GetSettings()
        {
            return Settings;
        }

        public void ApplySettings(ILionSettingsInfo newSettings)
        {
            Settings = (AutoBuilderSettings)newSettings;
        }

        public static AutoBuilderSettings Settings { get; private set; }

        public AutoBuilderSettingsProvider()
        {
            Settings = ScriptableObject.CreateInstance<AutoBuilderSettings>();
        }
    }
}