using UnityEngine;

namespace LionStudios.Editor.AutoBuilder
{
    
    public class FakeCMDArgsProvider : ScriptableObject , ICMDArgsProvider
    {
        public enum Environment
        { 
            Production,
            Development
        }

        public Environment environment;
        public string versionNumber = "2.2.4";
        public string buildNumber = "159";

        string[] ICMDArgsProvider.Args
        {
            get
            {
                return new[]
                {
                    "-blah",
                    "_batchmode",
                    $"-Args:environment={environment};versionNumber={versionNumber};buildNumber={buildNumber};buildName={versionNumber}({buildNumber});reimportAssets=true;"
                };
            }
        }
    }
}