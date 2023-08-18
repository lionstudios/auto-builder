using System;

#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif

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
        public AppGroupsCapability(bool enabled) : base(enabled)
        {
        }
        
#if UNITY_IOS
        public override void AddCapability(ProjectCapabilityManager capabilityManager)
        {
            capabilityManager.AddAppGroups(settings.appGroupIdentifiers);
        }
#endif
    }

}