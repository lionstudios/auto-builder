using System;

#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif

namespace LionStudios.Editor.AutoBuilder
{

    [Serializable]
    public class MapsCapabilitySettings : CapabilitySettings
    {
#if UNITY_IOS
        public MapsOptions mapOptions;
#else
        public string msg = "You need to switch to iOS platform in order to set these settings";
#endif
    }

    [Serializable]
    public class MapsCapability : Capability<MapsCapabilitySettings>
    {
        public MapsCapability(bool enabled) : base(enabled) {}
        
#if UNITY_IOS
        public override void AddCapability(ProjectCapabilityManager capabilityManager)
        {
            capabilityManager.AddMaps(settings.mapOptions);
        }
#endif
    }

}