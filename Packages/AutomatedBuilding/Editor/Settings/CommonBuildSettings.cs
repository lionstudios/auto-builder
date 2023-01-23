using UnityEngine;

namespace AutomatedBuilding
{
    [CreateAssetMenu(fileName = "CommonBuildSettings", menuName = "Build/CreateCommonBuildSettings")]
    public class CommonBuildSettings : ScriptableObject
    {
        public string DevAdditionalDefSymbols = "DEV";
        public string ScriptsFolder = "Assets/Scripts/";

    }
}