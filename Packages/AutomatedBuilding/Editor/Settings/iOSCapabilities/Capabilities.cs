using System;
using System.Collections.Generic;

namespace LionStudios.Editor.AutoBuilder
{

    [Serializable]
    public class Capabilities
    {

        public InAppPurchaseCapability inAppPurchase = new InAppPurchaseCapability(true);

        public DataProtectionCapability dataProtection = new DataProtectionCapability(false);

        public GameCenterCapability gameCenter = new GameCenterCapability(false);

        public HealthKitCapability healthKit = new HealthKitCapability(false);

        public HomeKitCapability homeKit = new HomeKitCapability(false);

        public WirelessAccessoryConfigurationCapability wirelessAccessoryConfiguration = new WirelessAccessoryConfigurationCapability(false);

        public AccessWifiInformationCapability accessWifiInformation= new AccessWifiInformationCapability(false);

        public PersonalVPNCapability personalVPN = new PersonalVPNCapability(false);

        public InterAppAudioCapability interAppAudio = new InterAppAudioCapability(false);

        public SignInWithAppleCapability signInWithApple = new SignInWithAppleCapability(false);

        public SiriCapability siri = new SiriCapability(false);

        public RemoteNotificationsCapability remoteNotifications = new RemoteNotificationsCapability(false);

        public iCloudCapability iCloud = new iCloudCapability(false);

        public AppGroupsCapability appGroups = new AppGroupsCapability(false);

        public MapsCapability maps = new MapsCapability(false);

        public WalletCapability wallet = new WalletCapability(false);

        public ApplePayCapability applePay = new ApplePayCapability(false);

        public AssociatedDomainsCapability associatedDomains = new AssociatedDomainsCapability(false);

        public KeychainSharingCapability keychainSharing = new KeychainSharingCapability(false);

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