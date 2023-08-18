using System;

#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif

namespace LionStudios.Editor.AutoBuilder
{

    [Serializable]
    public class HomeKitCapability : Capability<EmptyCapabilitySettings>
    {
        public HomeKitCapability(bool enabled) : base(enabled)
        {
        }
        
#if UNITY_IOS
        public override void AddCapability(ProjectCapabilityManager capabilityManager)
        {
            capabilityManager.AddHomeKit();
        }
#endif
    }

}