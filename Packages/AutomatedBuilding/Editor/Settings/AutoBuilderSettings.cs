using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LionStudios.Suite.Core;
using UnityEngine.Scripting;

namespace LionStudios.Editor.AutoBuilder
{
    public class AutoBuilderSettings : ILionSettingsInfo
    {
        public CommonBuildSettings commonBuildSettings;
        public AndroidBuildSettings androidBuildSettings;
        public IOSBuildSettings iosBuildSettings;
        public FakeCMDArgsProvider localBuildSettings;
    }
}