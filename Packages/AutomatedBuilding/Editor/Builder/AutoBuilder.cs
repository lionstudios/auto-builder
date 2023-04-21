using System;
using LionStudios.Editor.AutoBuilder;
using UnityEditor;

public static class AutoBuilder
{
    private static Builder _builder;

    public static event Action<BuildTargetGroup> OnBuildTriggered;

    public static void BuildAndroid()
    {
        OnBuildTriggered?.Invoke(BuildTargetGroup.Android);
        _builder = new AndroidBuilder(new RealCMDArgsProvider());
        _builder.Build();
    }

    public static void BuildIOS()
    {
        OnBuildTriggered?.Invoke(BuildTargetGroup.iOS);
        _builder = new IOSBuilder(new RealCMDArgsProvider());
        _builder.Build();
    }
    
    [MenuItem("LionStudios/Build/Android")]
    public static void BuildAndroidTest()
    {
        OnBuildTriggered?.Invoke(BuildTargetGroup.Android);
        var fakeCMDArgsProvider = AssetDatabase.LoadAssetAtPath<FakeCMDArgsProvider>(Builder.FAKE_CMD_ARGS_PATH);
        _builder = new AndroidBuilder(fakeCMDArgsProvider);
        _builder.Build();
    }

    [MenuItem("LionStudios/Build/iOS")]
    public static void BuildIOSTest()
    {
        OnBuildTriggered?.Invoke(BuildTargetGroup.iOS);
        var fakeCMDArgsProvider = AssetDatabase.LoadAssetAtPath<FakeCMDArgsProvider>(Builder.FAKE_CMD_ARGS_PATH);
        _builder = new IOSBuilder(fakeCMDArgsProvider);
        _builder.Build();
    }
}