using System;

namespace AutomatedBuilding
{
    public class RealCMDArgsProvider : ICMDArgsProvider
    {
        public string[] Args => Environment.GetCommandLineArgs();
    }
}