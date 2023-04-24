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
    public class RemoteNotificationSettings
    {
        [Header("One Signal Provisional Data")]
        public bool usingOneSignal;
        public string oneSignalProvisionalProfileName;
    }
    
    [System.Serializable]
    public class AppGroupSettings
    {
        public string[] appGroupIdentifiers;
    }

    [System.Serializable]
    public class CapabilitySettings
    {
        [Header("In-App")]
        public bool addInAppPurchase;
        
        [Header("Data Protection")]
        [Space(5)]
        public bool addDataProtection;
        
        [Header("Game Center")]
        [Space(5)]
        public bool addGameCenter;
        
        [Header("HealthKit")]
        [Space(5)]
        public bool addHealthKit;
        
        [Header("HomeKit")]
        [Space(5)]
        public bool addHomeKit;
        
        [Header("WirelessConfig")]
        [Space(5)]
        public bool addWirelessAccessoryConfig;
        
        [Header("WirelessInfo")]
        [Space(5)]
        public bool addAccessWifiInfo;
        
        [Header("PersonalVPN")]
        [Space(5)]
        public bool addPersonalVPN;
        
        [Header("InterAppAudio")]
        [Space(5)]
        public bool addInterAppAudio;
        
        [Header("SignInApple")]
        [Space(5)]
        public bool addSignInWithApple;
        
        [Header("Siri")]
        [Space(5)]
        public bool addSiri;
        
        [Header("Remote Notification")]
        [Space(20)] public bool addRemoteNotification;
        public RemoteNotificationSettings remoteNotificationSettings;

        [Header("App Group")]
        [Space(20)] public bool addAppGroup;
        public AppGroupSettings appGroupSettings;
        
        [Header("iCloud")]
        [Space(20)] public bool addiCloud;
        public iCloudSettings iCloudSettings;
        
        [Header("Maps")]
        [Space(20)] public bool addMaps;
        public mapSettings mapSettings;

        [Header("Wallet")]
        [Space(20)] public bool addWallet;
        public walletSettings walletSettings;

        
        [Header("Apple Pay")]
        [Space(20)] public bool addApplePay;
        public ApplePaySettings applePaySettings;
        
        
        [Header("Associated Domains")]
        [Space(20)] public bool addAssociatedDomains;
        public AssociatedDomainSettings associatedDomainSettings;

        [Header("Keychain Share")]
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

        [Header("Capabilities")] public CapabilitySettings AllCapabilities;

        [Header("SK Ad Network IDs")]
        public List<string> skAdIds = new List<string>();


        public string OrgName => ORGS_INFOS[_organization].Name;
        public string OrgTeamId => ORGS_INFOS[_organization].TeamId;
    }
}