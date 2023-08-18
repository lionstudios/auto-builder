using System;

#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif

namespace LionStudios.Editor.AutoBuilder
{

    [Serializable]
    public class KeychainSharingSettings : CapabilitySettings
    {
        public string[] accessGroups;
    }

    [Serializable]
    public class KeychainSharingCapability : Capability<KeychainSharingSettings>
    {
        public KeychainSharingCapability(bool enabled) : base(enabled)
        {
        }
        
#if UNITY_IOS
        public override void AddCapability(ProjectCapabilityManager capabilityManager)
        {
            capabilityManager.AddKeychainSharing(settings.accessGroups);
        }
#endif

    }

}
