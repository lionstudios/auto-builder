using AutomatedBuilding;
using UnityEditor;

public static class AutoBuilder
{
    private static Builder _builder;
    
    public static void BuildAndroid()
    {
        _builder = new AndroidBuilder(new RealCMDArgsProvider());
        _builder.Build();
    }
    
    public static void BuildIOS()
    {
        _builder = new IOSBuilder(new RealCMDArgsProvider());
        _builder.Build();
    }
    
    [MenuItem("Build/Android")]
    public static void BuildAndroidTest()
    {
        _builder = new AndroidBuilder(new FakeCMDArgsProvider());
        _builder.Build();
    }

    [MenuItem("Build/iOS")]
    public static void BuildIOSTest()
    {
        _builder = new IOSBuilder(new FakeCMDArgsProvider());
        _builder.Build();
    }
}