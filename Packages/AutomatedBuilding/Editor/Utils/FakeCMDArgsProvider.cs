namespace LionStudios.Editor.AutoBuilder
{
    public class FakeCMDArgsProvider : ICMDArgsProvider
    {
        private const string environment = "Production";
        private const string versionNumber = "2.2.4";
        private const string buildNumber = "159";
        private const string jdkPath = "";
        
        string[] ICMDArgsProvider.Args { get; } =
        {
            "-blah",
            "_batchmode",
            $"-Args:environment={environment};versionNumber={versionNumber};buildNumber={buildNumber};buildName={versionNumber}({buildNumber});jdkPath={jdkPath};reimportAssets=true;"
        };
    }
}