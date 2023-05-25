using UnityEngine;

namespace LionStudios.Editor.AutoBuilder
{
    public enum AdAdapterSettings
    {
        Ignore,
        FailIfNotStableVersion,
        AutoFixIfNotStableVersion,
    }
    public enum CsvFetchFailOptions
    {
        Continue,
        FailBuild,
    }
    public class CommonBuildSettings : ScriptableObject
    {
        public string[] DevAdditionalDefSymbols = {"DEV"};
        public string ScriptsFolder = "Assets/Scripts/";

        public AdAdapterSettings AdAdapterSettings;
        public CsvFetchFailOptions CsvFetchFailOptions;

    }
}