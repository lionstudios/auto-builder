using System;
using UnityEditor.iOS.Xcode;

namespace LionStudios.Editor.AutoBuilder
{

    [Serializable]
    public class InAppPurchaseCapability : Capability<EmptyCapabilitySettings>
    {
        public override void AddCapability(ProjectCapabilityManager capabilityManager)
        {
            capabilityManager.AddInAppPurchase();
        }
    }

}