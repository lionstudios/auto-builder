using System;
using UnityEditor.iOS.Xcode;

namespace LionStudios.Editor.AutoBuilder
{

    [Serializable]
    public class WalletCapabilitySettings : CapabilitySettings
    {
        public string[] passSubsets;
    }

    [Serializable]
    public class WalletCapability : Capability<WalletCapabilitySettings>
    {
        public override void AddCapability(ProjectCapabilityManager capabilityManager)
        {
            capabilityManager.AddWallet(settings.passSubsets);
        }
    }

}