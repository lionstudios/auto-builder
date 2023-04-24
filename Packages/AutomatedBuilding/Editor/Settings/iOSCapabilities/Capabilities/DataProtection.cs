using System;
using UnityEditor.iOS.Xcode;

namespace LionStudios.Editor.AutoBuilder
{

    [Serializable]
    public class DataProtectionCapability : Capability<EmptyCapabilitySettings>
    {
        public override void AddCapability(ProjectCapabilityManager capabilityManager)
        {
            capabilityManager.AddDataProtection();
        }
    }

}