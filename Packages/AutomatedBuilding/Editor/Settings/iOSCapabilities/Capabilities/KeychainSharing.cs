using System;
using UnityEditor.iOS.Xcode;

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

        public override void AddCapability(ProjectCapabilityManager capabilityManager)
        {
            capabilityManager.AddKeychainSharing(settings.accessGroups);
        }

    }

}
