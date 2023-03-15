using LionStudios.Editor.AutoBuilder;
using UnityEditor;

public static class AutoBuilder
{
    private static IBuilder _builder;

    public static void BuildAndroid(IBuilder projectSpecificBuilder)
    {
        _builder = new AndroidBuilder(new RealCMDArgsProvider(), projectSpecificBuilder);
        _builder.Build();
    }

    public static void BuildIOS(IBuilder projectSpecificBuilder)
    {
        _builder = new IOSBuilder(new RealCMDArgsProvider(), projectSpecificBuilder);
        _builder.Build();
    }

    [MenuItem("LionStudios/Build/Android")]
    public static void BuildAndroidTest()
    {
        var fakeCMDArgsProvider = AssetDatabase.LoadAssetAtPath<FakeCMDArgsProvider>(Builder.FAKE_CMD_ARGS_PATH);

        _builder = new AndroidBuilder(fakeCMDArgsProvider, new ProjectSpecificBuilderStub());
        _builder.Build();
    }

    [MenuItem("LionStudios/Build/iOS")]
    public static void BuildIOSTest()
    {
        var fakeCMDArgsProvider = AssetDatabase.LoadAssetAtPath<FakeCMDArgsProvider>(Builder.FAKE_CMD_ARGS_PATH);
        _builder = new IOSBuilder(fakeCMDArgsProvider, new ProjectSpecificBuilderStub());
        _builder.Build();
    }
}