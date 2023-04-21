using System.Collections.Generic;
using UnityEngine;
using UnityEditor.iOS.Xcode;

namespace LionStudios.Editor.AutoBuilder
{
    [System.Serializable]
    public class iCloudSettings
    {
        public bool enableKeyValueStorage;
        public bool enableICloudDocument;
        public bool enableCloudKit = true;
        public bool addDefaultContainers = true;
        public string[] customContainers;
    }

    [System.Serializable]
    public class mapSettings
    {
        public MapsOptions mapOptions;
    }

    [System.Serializable]
    public class walletSettings
    {
        public string[] passSubsets;
    }

    [System.Serializable]
    public class ApplePaySettings
    {
        public string[] merchants;
    }

    [System.Serializable]
    public class AssociatedDomainSettings
    {
        public string[] domains;
    }

    [System.Serializable]
    public class KeychainSharingSettings
    {
        public string[] accessGroups;
    }

    [System.Serializable]
    public class CapabilitySettings
    {
        public bool addInAppPurchase;
        public bool addRemoteNotification;
        public bool addDataProtection;
        public bool addGameCenter;
        public bool addHealthKit;
        public bool addHomeKit;
        public bool addWirelessAccessoryConfig;
        public bool addAccessWifiInfo;
        public bool addPersonalVPN;
        public bool addInterAppAudio;
        public bool addSignInWithApple;

        public bool addSiri;
        [Space(20)] public bool addiCloud;
        public iCloudSettings iCloudSettings;
        [Space(20)] public bool addMaps;
        public mapSettings mapSettings;

        [Space(20)] public bool addWallet;
        public walletSettings walletSettings;

        [Space(20)] public bool addApplePay;
        public ApplePaySettings applePaySettings;
        [Space(20)] public bool addAssociatedDomains;
        public AssociatedDomainSettings associatedDomainSettings;

        [Space(20)] public bool addKeychainSharing;
        public KeychainSharingSettings keychainSharingSettings;
    }

    public class IOSBuildSettings : ScriptableObject
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

        [Header("One Signal Provisional Data")]
        public bool usingOneSignal;

        public string oneSignalProvisionalProfileName;

        [Header("Capabilities")] public CapabilitySettings AllCapabilities;

        [Space(20)] [Header("SK Ad Network IDs")]
        public List<string> skAdIds = new List<string>();


        public string OrgName => ORGS_INFOS[_organization].Name;
        public string OrgTeamId => ORGS_INFOS[_organization].TeamId;
    }
}