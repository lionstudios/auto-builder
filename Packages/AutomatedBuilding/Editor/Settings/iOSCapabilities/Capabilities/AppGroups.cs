using System;
using UnityEditor.iOS.Xcode;

namespace LionStudios.Editor.AutoBuilder
{

    [Serializable]
    public class AppGroupsCapabilitySettings : CapabilitySettings
    {
        public string[] appGroupIdentifiers;
    }

    [Serializable]
    public class AppGroupsCapability : Capability<AppGroupsCapabilitySettings>
    {
        public override void AddCapability(ProjectCapabilityManager capabilityManager)
        {
            capabilityManager.AddAppGroups(settings.appGroupIdentifiers);
        }
    }

}