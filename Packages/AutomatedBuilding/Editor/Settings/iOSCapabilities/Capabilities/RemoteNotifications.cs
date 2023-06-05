using System;

#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif

namespace LionStudios.Editor.AutoBuilder
{

    [Serializable]
    public class RemoteNotificationsCapabilitySettings : CapabilitySettings
    {
        public bool usingOneSignal;
        public string oneSignalProvisionalProfileName;
    }

    [Serializable]
    public class RemoteNotificationsCapability : Capability<RemoteNotificationsCapabilitySettings>
    {
#if UNITY_IOS
        public override void AddCapability(ProjectCapabilityManager capabilityManager)
        {
            capabilityManager.AddPushNotifications(true);
            capabilityManager.AddBackgroundModes(BackgroundModesOptions.RemoteNotifications);
        }
#endif
    }

}