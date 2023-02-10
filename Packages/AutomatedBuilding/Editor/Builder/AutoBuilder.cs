using LionStudios.Editor.AutoBuilder;
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

    [MenuItem("LionStudios/Build/Android")]
    public static void BuildAndroidTest()
    {
        var fakeCMDArgsProvider = AssetDatabase.LoadAssetAtPath<FakeCMDArgsProvider>(Builder.FAKE_CMD_ARGS_PATH);

        _builder = new AndroidBuilder(fakeCMDArgsProvider);
        _builder.Build();
    }

    [MenuItem("LionStudios/Build/iOS")]
    public static void BuildIOSTest()
    {
        var fakeCMDArgsProvider = AssetDatabase.LoadAssetAtPath<FakeCMDArgsProvider>(Builder.FAKE_CMD_ARGS_PATH);
        _builder = new IOSBuilder(fakeCMDArgsProvider);
        _builder.Build();
    }
}