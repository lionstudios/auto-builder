using System;
using UnityEditor.iOS.Xcode;

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
        public override void AddCapability(ProjectCapabilityManager capabilityManager)
        {
            capabilityManager.AddAssociatedDomains(settings.domains);
        }
    }

}