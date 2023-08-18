using System;

#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif

namespace LionStudios.Editor.AutoBuilder
{

    [Serializable]
    public class ApplePayCapabilitySettings : CapabilitySettings
    {
        public string[] merchants;
    }

    [Serializable]
    public class ApplePayCapability : Capability<ApplePayCapabilitySettings>
    {
        public ApplePayCapability(bool enabled) : base(enabled)
        {
        }
        
#if UNITY_IOS
        public override void AddCapability(ProjectCapabilityManager capabilityManager)
        {
            capabilityManager.AddApplePay(settings.merchants);
        }
#endif
    }

}