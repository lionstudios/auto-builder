using System;
using UnityEditor.iOS.Xcode;

namespace LionStudios.Editor.AutoBuilder
{

    [Serializable]
    public class MapsCapabilitySettings : CapabilitySettings
    {
        public MapsOptions mapOptions;
    }

    [Serializable]
    public class MapsCapability : Capability<MapsCapabilitySettings>
    {
        public override void AddCapability(ProjectCapabilityManager capabilityManager)
        {
            capabilityManager.AddMaps(settings.mapOptions);
        }
    }

}