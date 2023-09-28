using System;
using UnityEngine;

namespace LionStudios.Editor.AutoBuilder
{
    [Serializable]
    public class FakeCMDArgsProvider : BuildSettingsSectionBase, ICMDArgsProvider
    {
        public enum Environment
        { 
            Production,
            Development
        }

        public Environment environment;
        public string versionNumber = "0.0.0";
        public string buildNumber = "1";

        string[] ICMDArgsProvider.Args
        {
            get
            {
                return new[]
                {
                    "-fake",
                    "_batchmode",
                    $"-Args:environment={environment};versionNumber={versionNumber};buildNumber={buildNumber};buildName={versionNumber}({buildNumber});reimportAssets=true;"
                };
            }
        }
    }
}