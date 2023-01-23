using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AutomatedBuilding;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public abstract class Builder
{
    protected const string SETTINGS_PATH = "Assets/LionStudios/AutomatedBuilding/Editor";
    protected static readonly string COMMON_SETTINGS_PATH = $"{SETTINGS_PATH}/CommonBuildSettings.asset";
    
    private readonly ICMDArgsProvider _cmdArgsProvider;
    private readonly bool _isTestEditorBuild;
    
    protected static CommonBuildSettings CommonBuildSettings;

    protected abstract string DefineSymbols { get; }
    protected abstract string DefaultBuildFolder { get; }
    protected abstract BuildTargetGroup BuildTargetGroup { get; }
    protected abstract ScriptingImplementation ScriptingImplementation { get; }
    
    protected abstract BuildPlayerOptions InitializeSpecific(IDictionary<string, string> cmdParamsMap,
        bool isProduction, string buildLocation);
    
    protected abstract void CreateBuildDirectory(string path);

    protected Builder(ICMDArgsProvider cmdArgsProvider)
    {
        _cmdArgsProvider = cmdArgsProvider;
        _isTestEditorBuild = cmdArgsProvider is FakeCMDArgsProvider;

        CommonBuildSettings = AssetDatabase.LoadAssetAtPath<CommonBuildSettings>(COMMON_SETTINGS_PATH);
    }

    ~Builder()
    {
        CommonBuildSettings = null;
    }

    public void Build()
    {
        AssetDatabase.StartAssetEditing();
        
        var buildPlayerOptions = Initialize(BuildTargetGroup, DefineSymbols, ScriptingImplementation,
            out var isProduction, out var buildLocation);
        
        var report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        
         if (report.summary.result == BuildResult.Succeeded)
         {
             OpenFolder(buildLocation);
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
        
        AssetDatabase.StopAssetEditing();
    }

    [InitializeOnLoadMethod]
    private static void SetupProject()
    {
        if (AssetDatabase.LoadAssetAtPath<CommonBuildSettings>(COMMON_SETTINGS_PATH) == null)
        {
            Directory.CreateDirectory(SETTINGS_PATH);
            AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<CommonBuildSettings>(), COMMON_SETTINGS_PATH);
        }
    }

    protected static string[] GetScenes()
    {
        string[] res = EditorBuildSettings.scenes.Select(s => s.path).ToArray();
        Debug.Log($"Scenes to build: {string.Join(", ", res)}");
        return res;
    }

    private BuildPlayerOptions Initialize(BuildTargetGroup buildTargetGroup, string defineSymbols,
        ScriptingImplementation scriptingImplementation, out bool isProduction, out string buildLocation)
    {
        try
        {
            var cmdArgs = _cmdArgsProvider.Args;
            var cmdParamsMap = new Dictionary<string, string>
            {
                {"environment", "development"},
                {"versionNumber", "1"},
                {"buildNumber", string.Empty},
                {"buildName", string.Empty},
                {"buildPath", string.Empty},
                {"jdkPath", string.Empty},
                {"reimportAssets", string.Empty},
            };
        
            SetCmdParamsMap(cmdArgs, cmdParamsMap);

            if (string.IsNullOrEmpty(cmdParamsMap["buildPath"]))
            {
                cmdParamsMap["buildPath"] = DefaultBuildFolder;
            }
            
            isProduction = cmdParamsMap["environment"].Equals("production", StringComparison.InvariantCultureIgnoreCase);
            buildLocation = GetBuildLocation(cmdParamsMap);
        
            CreateBuildDirectory(buildLocation);
        
            Debug.unityLogger.filterLogType = isProduction ? LogType.Error : LogType.Log;
        
            var currentDefineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
            currentDefineSymbols = GetDefineSymbolsStr(defineSymbols, currentDefineSymbols, 
                CommonBuildSettings.DevAdditionalDefSymbols, isProduction);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, currentDefineSymbols); 
            PlayerSettings.SetScriptingBackend(buildTargetGroup, scriptingImplementation);

            CheckIfReimportRequired(cmdParamsMap);
            var buildPlayerOptions = InitializeSpecific(cmdParamsMap, isProduction, buildLocation);
            buildPlayerOptions.options = isProduction ? BuildOptions.None : BuildOptions.Development;

            return buildPlayerOptions;
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
            throw;
        }
    }

    private static string GetDefineSymbolsStr(string newDefineSymbols, string currentDefineSymbols, 
        string devDefineSymbols, bool isProduction)
    {
        var currentSymbolsArr = currentDefineSymbols.Split(';');
        var newDefineSymbolsArr = newDefineSymbols.Split(';');
        var devDefineSymbolsArr = devDefineSymbols.Split(';');
        var set = new HashSet<string>();
        var sb = new StringBuilder();

        foreach (var symbol in currentSymbolsArr)
        {
            set.Add(symbol);
        }

        foreach (var symbol in newDefineSymbolsArr)
        {
            set.Add(symbol);
        }

        if (isProduction)
        {
            foreach (var symbol in devDefineSymbolsArr)
            {
                set.Remove(symbol);
            }
        }
        else
        {
            foreach (var symbol in devDefineSymbolsArr)
            {
                set.Add(symbol);
            }
        }

        foreach (var symbol in set)
        {
            sb.Append($"{symbol};");
        }

        return sb.ToString();
    }

    private static void CheckIfReimportRequired(IDictionary<string, string> cmdParamsMap)
    {
        if (bool.TryParse(cmdParamsMap["reimportAssets"], out var isReimport) && isReimport)
        {
            AssetDatabase.ImportAsset(CommonBuildSettings.ScriptsFolder, ImportAssetOptions.ImportRecursive |
                                                         ImportAssetOptions.DontDownloadFromCacheServer);
        }
    }

    protected virtual string GetBuildLocation(IDictionary<string, string> cmdParamsMap)
    {
        return Path.Combine(cmdParamsMap["buildPath"], cmdParamsMap["environment"]);
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