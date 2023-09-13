﻿using System;
using UnityEngine;

namespace LionStudios.Editor.AutoBuilder
{
    [Serializable]
    public class CommonBuildSettings
    {
        public string[] DevAdditionalDefSymbols = { "DEV" };
        public string ScriptsFolder = "Assets/Scripts/";

    }
}