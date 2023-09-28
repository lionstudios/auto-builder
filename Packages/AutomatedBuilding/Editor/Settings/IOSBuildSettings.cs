using System;
using System.Collections.Generic;
using UnityEngine;

namespace LionStudios.Editor.AutoBuilder
{
    [Serializable]
    public class IOSBuildSettings : BuildSettingsSectionBase
    {
        private enum Organization
        {
            LionStudios,
            LionStudiosPlus,
            Hippotap
        }

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

        public Capabilities Capabilities;
        
        public List<string> skAdIds = new List<string>();


        public string OrgName => ORGS_INFOS[_organization].Name;
        public string OrgTeamId => ORGS_INFOS[_organization].TeamId;
    }
}