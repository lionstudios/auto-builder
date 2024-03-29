using System;
using System.Collections.Generic;

namespace LionStudios.Editor.AutoBuilder
{

    [Serializable]
    public class Capabilities
    {

        public InAppPurchaseCapability inAppPurchase;

        public DataProtectionCapability dataProtection;

        public GameCenterCapability gameCenter;

        public HealthKitCapability healthKit;

        public HomeKitCapability homeKit;

        public WirelessAccessoryConfigurationCapability wirelessAccessoryConfiguration;

        public AccessWifiInformationCapability accessWifiInformation;

        public PersonalVPNCapability personalVPN;

        public InterAppAudioCapability interAppAudio;

        public SignInWithAppleCapability signInWithApple;

        public SiriCapability siri;

        public RemoteNotificationsCapability remoteNotifications;

        public iCloudCapability iCloud;

        public AppGroupsCapability appGroups;

        public MapsCapability maps;

        public WalletCapability wallet;

        public ApplePayCapability applePay;

        public AssociatedDomainsCapability associatedDomains;

        public KeychainSharingCapability keychainSharing;

        public List<ICapability> AllCapabilities => new List<ICapability>()
        {
            inAppPurchase,
            dataProtection,
            gameCenter,
            healthKit,
            homeKit,
            wirelessAccessoryConfiguration,
            accessWifiInformation,
            personalVPN,
            interAppAudio,
            signInWithApple,
            siri,
            remoteNotifications,
            iCloud,
            appGroups,
            maps,
            wallet,
            applePay,
            associatedDomains,
            keychainSharing
        };
    }

}