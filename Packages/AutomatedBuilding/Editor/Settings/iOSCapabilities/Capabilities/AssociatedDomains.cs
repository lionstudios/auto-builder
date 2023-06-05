using System;

#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif

namespace LionStudios.Editor.AutoBuilder
{

    [Serializable]
    public class AssociatedDomainsCapabilitySettings : CapabilitySettings
    {
        public string[] domains;
    }

    [Serializable]
    public class AssociatedDomainsCapability : Capability<AssociatedDomainsCapabilitySettings>
    {
#if UNITY_IOS
        public override void AddCapability(ProjectCapabilityManager capabilityManager)
        {
            capabilityManager.AddAssociatedDomains(settings.domains);
        }
#endif
    }

}