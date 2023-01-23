﻿using UnityEngine;
using UnityEngine.Serialization;

namespace AutomatedBuilding
{
    [CreateAssetMenu(fileName = "AndroidBuildSettings", menuName = "Build/CreateAndroidBuildSettings")]
    public class AndroidBuildSettings : ScriptableObject
    {
        [FormerlySerializedAs("KEY_STORE_PATH")]
        public string KeystorePath = "keystore/keystore.keystore";
        [FormerlySerializedAs("KEY_STORE_PASS")]
        public string KeystorePassword = "";
        [FormerlySerializedAs("KEY_STORE_ALIAS")]
        public string KeystoreAlias = "";
        [FormerlySerializedAs("KEY_STORE_ALIAS_PASS")]
        public string KeystoreAliasPassword = "";
        
        public string AdditionalDefineSymbols;
    }
}