using LionStudios.Suite.Core;
using UnityEngine;

namespace LionStudios.Editor.AutoBuilder
{
    public class AutoBuilderSettings : ILionSettingsInfo
    {
        public CommonBuildSettings common;
        public AndroidBuildSettings android;
        public IOSBuildSettings iOS;
        public FakeCMDArgsProvider local;
    }
}