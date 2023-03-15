using UnityEditor;

namespace LionStudios.Editor.AutoBuilder
{
    public static class AutoBuilderDefault
    {
        private static IBuilder _builder;
        private static IBuilder _projectSpecificBuilderStub = new ProjectSpecificBuilderStub();

        public static void BuildAndroid()
        {
            _builder = new AndroidBuilder(new RealCMDArgsProvider(), _projectSpecificBuilderStub);
            _builder.Build();
        }

        public static void BuildIOS()
        {
            _builder = new IOSBuilder(new RealCMDArgsProvider(), _projectSpecificBuilderStub);
            _builder.Build();
        }

        [MenuItem("LionStudios/Build/Android")]
        public static void BuildAndroidTest()
        {
            var fakeCMDArgsProvider = AssetDatabase.LoadAssetAtPath<FakeCMDArgsProvider>(Builder.FAKE_CMD_ARGS_PATH);

            _builder = new AndroidBuilder(fakeCMDArgsProvider, _projectSpecificBuilderStub);
            _builder.Build();
        }

        [MenuItem("LionStudios/Build/iOS")]
        public static void BuildIOSTest()
        {
            var fakeCMDArgsProvider = AssetDatabase.LoadAssetAtPath<FakeCMDArgsProvider>(Builder.FAKE_CMD_ARGS_PATH);
            _builder = new IOSBuilder(fakeCMDArgsProvider, _projectSpecificBuilderStub);
            _builder.Build();
        }
    }
}