using System;

#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif

namespace LionStudios.Editor.AutoBuilder
{

    [Serializable]
    public class InAppPurchaseCapability : Capability<EmptyCapabilitySettings>
    {
        public InAppPurchaseCapability(bool enabled) : base(enabled)
        {
            
        }

#if UNITY_IOS
        public override void AddCapability(ProjectCapabilityManager capabilityManager)
        {
            capabilityManager.AddInAppPurchase();
        }
#endif
    }

}