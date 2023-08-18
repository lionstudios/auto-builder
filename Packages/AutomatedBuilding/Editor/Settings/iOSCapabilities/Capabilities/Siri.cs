using System;

#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif

namespace LionStudios.Editor.AutoBuilder
{

    [Serializable]
    public class SiriCapability : Capability<EmptyCapabilitySettings>
    {
        public SiriCapability(bool enabled) : base(enabled) {}
        
#if UNITY_IOS
        public override void AddCapability(ProjectCapabilityManager capabilityManager)
        {
            capabilityManager.AddSiri();
        }
#endif
    }

}