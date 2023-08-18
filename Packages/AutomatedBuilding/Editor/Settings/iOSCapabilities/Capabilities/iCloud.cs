using System;

#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif

namespace LionStudios.Editor.AutoBuilder
{

    [Serializable]
    public class iCloudCapabilitySettings : CapabilitySettings
    {
        public bool enableKeyValueStorage;
        public bool enableICloudDocument;
        public bool enableCloudKit = true;
        public bool addDefaultContainers = true;
        public string[] customContainers;
    }

    [Serializable]
    public class iCloudCapability : Capability<iCloudCapabilitySettings>
    {
        public iCloudCapability(bool enabled) : base(enabled) {}
        
#if UNITY_IOS
        public override void AddCapability(ProjectCapabilityManager capabilityManager)
        {
            capabilityManager.AddiCloud(settings.enableKeyValueStorage, settings.enableICloudDocument, settings.enableCloudKit, settings.addDefaultContainers, settings.customContainers);
        }
#endif
    }

}