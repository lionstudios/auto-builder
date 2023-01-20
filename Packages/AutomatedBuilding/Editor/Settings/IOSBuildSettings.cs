using UnityEngine;

namespace AutomatedBuilding
{
    [CreateAssetMenu(fileName = "IOSBuildSettings", menuName = "Build/CreateIOSBuildSettings")]
    public class IOSBuildSettings : ScriptableObject
    {
        public string TargetName = "Unity-iPhone";
        public string OrgName = "Lion Studios LLC";
        public string OrgCode = "4GT5PAZNM9";
        public string PovisioningProfileName = "";
        public string AdditionalDefineSymbols;
        public string DefaultBuildFolder = "builds/ios";
    }
}