using System;

#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif

namespace LionStudios.Editor.AutoBuilder
{

    [Serializable]
    public class AccessWifiInformationCapability : Capability<EmptyCapabilitySettings>
    {
        public AccessWifiInformationCapability(bool enabled) : base(enabled)
        {
        }
        
#if UNITY_IOS
        public override void AddCapability(ProjectCapabilityManager capabilityManager)
        {
            capabilityManager.AddAccessWiFiInformation();
        }
#endif
    }

}