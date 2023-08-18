using System;

#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif

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
        public WalletCapability(bool enabled) : base(enabled)
        {
        }
        
#if UNITY_IOS
        public override void AddCapability(ProjectCapabilityManager capabilityManager)
        {
            capabilityManager.AddWallet(settings.passSubsets);
        }
#endif
    }

}