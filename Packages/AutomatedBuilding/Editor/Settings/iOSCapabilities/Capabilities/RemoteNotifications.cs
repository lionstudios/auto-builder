using System;
using UnityEditor.iOS.Xcode;

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
        public override void AddCapability(ProjectCapabilityManager capabilityManager)
        {
            capabilityManager.AddPushNotifications(true);
            capabilityManager.AddBackgroundModes(BackgroundModesOptions.RemoteNotifications);
        }
    }

}