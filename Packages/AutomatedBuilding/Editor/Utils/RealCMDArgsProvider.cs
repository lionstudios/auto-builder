using System;

namespace LionStudios.Editor.AutoBuilder
{
    public class RealCMDArgsProvider : ICMDArgsProvider
    {
        public string[] Args => Environment.GetCommandLineArgs();
    }
}