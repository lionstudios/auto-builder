using UnityEngine;

namespace LionStudios.Editor.AutoBuilder
{
    [CreateAssetMenu(fileName = "CommonBuildSettings", menuName = "Build/CreateCommonBuildSettings")]
    public class CommonBuildSettings : ScriptableObject
    {
        public string DevAdditionalDefSymbols = "DEV";
        public string ScriptsFolder = "Assets/Scripts/";

    }
}