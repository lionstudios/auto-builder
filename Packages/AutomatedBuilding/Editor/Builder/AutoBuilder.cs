using System;
using LionStudios.Editor.AutoBuilder;
using UnityEditor;

public static class AutoBuilder
{
    private static Builder _builder;

    public static event Action<BuildTargetGroup> OnBuildTriggered;

    public static async void BuildAndroid()
    {
        OnBuildTriggered?.Invoke(BuildTargetGroup.Android);
        _builder = new AndroidBuilder(new RealCMDArgsProvider());
        await _builder.Build();
    }

    public static async void BuildIOS()
    {
        OnBuildTriggered?.Invoke(BuildTargetGroup.iOS);
        _builder = new IOSBuilder(new RealCMDArgsProvider());
        await _builder.Build();
    }
    
    [MenuItem("LionStudios/Build/Android")]
    public static async void BuildAndroidTest()
    {
        OnBuildTriggered?.Invoke(BuildTargetGroup.Android);
        var fakeCMDArgsProvider = AssetDatabase.LoadAssetAtPath<FakeCMDArgsProvider>(Builder.FAKE_CMD_ARGS_PATH);
        _builder = new AndroidBuilder(fakeCMDArgsProvider);
        await _builder.Build();
    }

    [MenuItem("LionStudios/Build/iOS")]
    public static async void BuildIOSTest()
    {
        OnBuildTriggered?.Invoke(BuildTargetGroup.iOS);
        var fakeCMDArgsProvider = AssetDatabase.LoadAssetAtPath<FakeCMDArgsProvider>(Builder.FAKE_CMD_ARGS_PATH);
        _builder = new IOSBuilder(fakeCMDArgsProvider);
        await _builder.Build();
    }
}