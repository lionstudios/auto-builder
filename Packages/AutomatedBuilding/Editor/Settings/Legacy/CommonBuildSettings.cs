using System;
using UnityEngine;

namespace LionStudios.Editor.AutoBuilder.Legacy
{
    public class CommonBuildSettings : ScriptableObject
    {
        public string[] DevAdditionalDefSymbols = { "DEV" };
        public string ScriptsFolder = "Assets/Scripts/";
    }
}