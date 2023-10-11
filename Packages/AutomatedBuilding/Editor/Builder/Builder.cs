using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LionStudios.Editor.AutoBuilder.AdapterStabilizer;
using LionStudios.Suite.Core;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace LionStudios.Editor.AutoBuilder
{
    public abstract class Builder
    {
        protected const string SETTINGS_PATH = "Assets/LionStudios/AutomatedBuilding/Editor";
        protected const string DATE_FORMAT = "yy-MM-dd-HH-mm";

        public static readonly string COMMON_SETTINGS_PATH = $"{SETTINGS_PATH}/CommonBuildSettings.asset";
        public static readonly string FAKE_CMD_ARGS_PATH = $"{SETTINGS_PATH}/FakeCMDArgs.asset"; //

        private readonly ICMDArgsProvider _cmdArgsProvider;
        private readonly bool _isTestEditorBuild;

        protected static CommonBuildSettings _commonBuildSettings;

        protected static CommonBuildSettings CommonBuildSettings
        {
            get
            {
                if (_commonBuildSettings == null)
                {
                    _commonBuildSettings = LionSettingsService.GetSettings<AutoBuilderSettings>().common;
                }

                return _commonBuildSettings;
            }
        }

        protected abstract string BuildLocation { get; }
        protected abstract BuildTargetGroup BuildTargetGroup { get; }
        protected abstract ScriptingImplementation ScriptingImplementation { get; }

        protected abstract BuildPlayerOptions InitializeSpecific(IDictionary<string, string> cmdParamsMap, bool isProduction);

        protected abstract void CreateBuildDirectory(string path);

        protected Builder(ICMDArgsProvider cmdArgsProvider)
        {
            _cmdArgsProvider = cmdArgsProvider;
            _isTestEditorBuild = cmdArgsProvider is FakeCMDArgsProvider;
        }

        ~Builder()
        {
            _commonBuildSettings = null;
        }

        void FailBuild(string message)
        {
            if (Application.isBatchMode)
            {
                Debug.LogError("\n\n" + message + "\n\n");
                EditorApplication.Exit(-1);
            }
            else
            {
                throw new ApplicationException(message);
            }
        }

        async Task HandleAdAdapters()
        {
            bool result = await LionMaxAdapterStabiliser.InitAdNetworkData();
            if (!result)
            {
                string message = "Failed to fetch stable ad adapters, possible connection error\nFailing the build.";
                FailBuild(message);
            }
            else
            {
                List<string> mismatchedAdapterNames;
                if (BuildTargetGroup == BuildTargetGroup.iOS)
                {
                    mismatchedAdapterNames = LionMaxAdapterStabiliser.GetMismatchIosAdapterNames();
                }
                else
                {
                    mismatchedAdapterNames = LionMaxAdapterStabiliser.GetMismatchAndroidAdapterNames();
                }

                if (mismatchedAdapterNames.Count != 0)
                {
                    string message = "Some adapters are out of date:";
                    foreach (string s in mismatchedAdapterNames)
                    {
                        message += "\n - " + s;
                    }

                    message += "\nFailing the build.";
                    FailBuild(message);
                }
            }

            Debug.Log("Ad adapter check complete, ad adapters are stable.");
        }

        public async Task Build()
        {
            try
            {
                AssetDatabase.StartAssetEditing();
                LionSettingsService.InitializeService();
                var buildPlayerOptions = Initialize(BuildTargetGroup, CommonBuildSettings.DevAdditionalDefSymbols,
                    ScriptingImplementation, out var isProduction, out var validateAdapters);

                if (validateAdapters)
                {
                    await HandleAdAdapters();
                }

                var report = BuildPipeline.BuildPlayer(buildPlayerOptions);

                if (report.summary.result == BuildResult.Succeeded)
                {
                    OpenFolder(BuildLocation);
                }
                else
                {
                    Debug.LogError($"Build Error -> {report.summary.result}");
                }

                if (_isTestEditorBuild)
                {
                    OpenFolder(buildPlayerOptions.locationPathName);
                }
                else
                {
                    EditorApplication.Exit(report.summary.result == BuildResult.Succeeded ? 0 : 2);
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }
        }

        protected static string[] GetScenes()
        {
            string[] res = EditorBuildSettings.scenes.Select(s => s.path).ToArray();
            Debug.Log($"Scenes to build: {string.Join(", ", res)}");
            return res;
        }

        private BuildPlayerOptions Initialize(BuildTargetGroup buildTargetGroup, string[] devAdditionalDefSymbols,
            ScriptingImplementation scriptingImplementation, out bool isProduction, out bool validateAdapters)
        {
            var cmdArgs = _cmdArgsProvider.Args;
            var cmdParamsMap = new Dictionary<string, string>
            {
                { "environment", "development" },
                { "versionNumber", "1" },
                { "buildNumber", string.Empty },
                { "buildName", string.Empty },
                { "reimportAssets", string.Empty },
                { "validateAdapters", "true" },
            };

            SetCmdParamsMap(cmdArgs, cmdParamsMap);

            isProduction = cmdParamsMap["environment"].Equals("production", StringComparison.InvariantCultureIgnoreCase);
            validateAdapters = cmdParamsMap["validateAdapters"].Equals("true", StringComparison.InvariantCultureIgnoreCase);

            CreateBuildDirectory(BuildLocation);

            Debug.unityLogger.filterLogType = isProduction ? LogType.Error : LogType.Log;

            if (devAdditionalDefSymbols != null && devAdditionalDefSymbols.Length > 0)
            {
                var currentDefineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
                currentDefineSymbols = GetDefineSymbolsStr(devAdditionalDefSymbols, currentDefineSymbols, isProduction);
                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, currentDefineSymbols);
            }

            PlayerSettings.SetScriptingBackend(buildTargetGroup, scriptingImplementation);
            CheckIfReimportRequired(cmdParamsMap);
            var buildPlayerOptions = InitializeSpecific(cmdParamsMap, isProduction);
            buildPlayerOptions.options = isProduction ? BuildOptions.None : BuildOptions.Development;

            return buildPlayerOptions;
        }

        private static string GetDefineSymbolsStr(string[] additionalSymbols, string currentDefineSymbols,
            bool isProduction)
        {
            var currentSymbolsArr = currentDefineSymbols.Split(';');
            var set = new HashSet<string>();
            var sb = new StringBuilder();

            foreach (var symbol in currentSymbolsArr)
            {
                set.Add(symbol);
            }

            foreach (var symbol in additionalSymbols)
            {
                set.Add(symbol);
            }

            foreach (var symbol in set)
            {
                sb.Append($"{symbol};");
            }

            return sb.ToString();
        }

        private void CheckIfReimportRequired(IDictionary<string, string> cmdParamsMap)
        {
            if (bool.TryParse(cmdParamsMap["reimportAssets"], out var isReimport) && isReimport)
            {
                AssetDatabase.ImportAsset(CommonBuildSettings.ScriptsFolder, ImportAssetOptions.ImportRecursive |
                                                                             ImportAssetOptions.DontDownloadFromCacheServer);
            }
        }

        private static string[] GetCustomArgs(string[] cmdArgs, string customArgsToken)
        {
            foreach (var cmdParam in cmdArgs)
            {
                var trimIndex = cmdParam.IndexOf(customArgsToken, 0, cmdParam.Length, StringComparison.Ordinal);

                if (trimIndex == -1)
                {
                    continue;
                }

                trimIndex += customArgsToken.Length;
                var customArgsStr = cmdParam.Substring(trimIndex);
                cmdArgs = customArgsStr.Split(';');
                return cmdArgs;
            }

            return null;
        }

        private static void SetCmdParamsMap(string[] cmdArgs,
            IDictionary<string, string> cmdParamsMap, string customArgsToken = "-Args:")
        {
            cmdArgs = GetCustomArgs(cmdArgs, customArgsToken);

            if (cmdArgs == null)
            {
                Debug.Log("Custom arguments were not found");
                return;
            }

            foreach (var cmdParam in cmdArgs)
            {
                if (string.IsNullOrEmpty(cmdParam))
                {
                    continue;
                }

                var kvp = cmdParam.Split('=');

                if (kvp.Length != 2)
                {
                    Debug.LogError($"command: {cmdParam} is malformed");
                    continue;
                }

                var paramName = kvp[0].Trim();
                var paramValue = kvp[1].Trim();

                if (!cmdParamsMap.ContainsKey(paramName))
                {
                    Debug.LogError($"unknown parameter: {paramName}");
                }

                cmdParamsMap[paramName] = paramValue;
            }
        }

#if UNITY_EDITOR_WIN
    private static void OpenFolder(string path)
    {
        var winPathDI = new DirectoryInfo(new DirectoryInfo(path).FullName);

        try
        {
            System.Diagnostics.Process.Start("explorer.exe", winPathDI.Exists ? winPathDI.FullName : $"/select,{winPathDI.FullName}");
        }
        catch (System.ComponentModel.Win32Exception e)
        {
            // tried to open win explorer in mac
            // just silently skip error
            // we currently have no platform define for the current OS we are in, so we resort to this
            e.HelpLink = ""; // do anything with this variable to silence warning about not using it
        }
    }
#endif

#if UNITY_EDITOR_OSX
        private static void OpenFolder(string path) //copied from match3
        {
            bool openInsidesOfFolder = false;

            // try mac
            string macPath = path.Replace("\\", "/"); // mac finder doesn't like backward slashes

            if (System.IO.Directory.Exists(macPath)) // if path requested is a folder, automatically open insides of that folder
            {
                openInsidesOfFolder = true;
            }

            if (!macPath.StartsWith("\""))
            {
                macPath = "\"" + macPath;
            }

            if (!macPath.EndsWith("\""))
            {
                macPath = macPath + "\"";
            }

            string arguments = (openInsidesOfFolder ? "" : "-R ") + macPath;

            try
            {
                System.Diagnostics.Process.Start("open", arguments);
            }
            catch (System.ComponentModel.Win32Exception e)
            {
                // tried to open mac finder in windows
                // just silently skip error
                // we currently have no platform define for the current OS we are in, so we resort to this
                e.HelpLink = ""; // do anything with this variable to silence warning about not using it
            }
        }
#endif
    }
}