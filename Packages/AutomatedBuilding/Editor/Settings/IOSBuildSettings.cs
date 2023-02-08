using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace LionStudios.Editor.AutoBuilder
{
    
    public class IOSBuildSettings : ScriptableObject
    {
        private enum Organization { LionStudios, LionStudiosPlus, Hippotap }

        private class OrgInfo 
        { 
            public string Name;
            public string TeamId;
        }

        private static readonly Dictionary<Organization, OrgInfo> ORGS_INFOS = new Dictionary<Organization, OrgInfo>()
        {
            { Organization.LionStudios, new OrgInfo { Name = "Lion Studios LLC", TeamId = "4GT5PAZNM9" } },
            { Organization.LionStudiosPlus, new OrgInfo { Name = "Lion Studios Plus LLC", TeamId = "34L8NRZ6AB" } },
            { Organization.Hippotap, new OrgInfo { Name = "HippoTap, LLC", TeamId = "4QA754R8JN" } }
        };

        public string TargetName = "Unity-iPhone";

        [SerializeField] Organization _organization;
        public string ProvisioningProfileName = "";

        [Header("One Signal Provisional Data")]
        public bool usingOneSignal;
        public string oneSignalProductIdentifier = "<yourbundleid>.OneSignalNotificationServiceExtension";
        public string oneSignalProvisionalProfileName;

        [Header("Capabilities")] 
        public bool inAppPurchase;
        public bool RemoteNotifications;
        
        public string OrgName => ORGS_INFOS[_organization].Name;
        public string OrgTeamId => ORGS_INFOS[_organization].TeamId;
    }
}