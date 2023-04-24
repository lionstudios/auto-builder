using System;
using UnityEditor.iOS.Xcode;

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
        public override void AddCapability(ProjectCapabilityManager capabilityManager)
        {
            capabilityManager.AddApplePay(settings.merchants);
        }
    }

}