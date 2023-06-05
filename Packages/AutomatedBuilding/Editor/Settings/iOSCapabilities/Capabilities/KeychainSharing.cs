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

#if UNITY_IOS
        public override void AddCapability(ProjectCapabilityManager capabilityManager)
        {
            capabilityManager.AddKeychainSharing(settings.accessGroups);
        }
#endif

    }

}
