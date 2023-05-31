using UnityEngine;

namespace LionStudios.Editor.AutoBuilder
{
    public enum AdAdapterSettings
    {
        FailIfNotStableVersion,
        Ignore,
    }
    public enum CsvFetchFailOptions
    {
        FailBuild,
        Continue,
    }
    public class CommonBuildSettings : ScriptableObject
    {
        public string[] DevAdditionalDefSymbols = {"DEV"};
        public string ScriptsFolder = "Assets/Scripts/";

        public AdAdapterSettings AdAdapterSettings;
        public CsvFetchFailOptions CsvFetchFailOptions;

    }
}