using UnityEngine;

namespace AutomatedBuilding
{
    [CreateAssetMenu(fileName = "IOSBuildSettings", menuName = "Build/CreateIOSBuildSettings")]
    public class IOSBuildSettings : ScriptableObject
    {
        public string TargetName = "Unity-iPhone";
        public string OrgName = "Lion Studios LLC";
        public string OrgCode = "4GT5PAZNM9";
        public string ProvisioningProfileName = "";
        public string AdditionalDefineSymbols;
        public string DefaultBuildFolder = "builds/ios";
        
        [Header("One Signal Provisional Data")]
        public bool usingOneSignal;
        public string oneSignalProductIdentifier = "<yourbundleid>.OneSignalNotificationServiceExtension";
        public string oneSignalProvisionalProfileName;

        [Header("Capabilities")] 
        public bool inAppPurchase;
        public bool RemoteNotifications;
    }
}