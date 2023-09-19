using LionStudios.Suite.Core;
using UnityEngine;

namespace LionStudios.Editor.AutoBuilder
{
    public class AutoBuilderSettings : ILionSettingsInfo
    {
        [Space(10)]
        public CommonBuildSettings commonBuildSettings;
        [Space(10)]
        public AndroidBuildSettings androidBuildSettings;
        public IOSBuildSettings iosBuildSettings;
        [Space(10)]
        public FakeCMDArgsProvider localBuildSettings;
    }
}