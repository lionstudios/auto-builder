using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace LionStudios.Editor.AutoBuilder.Legacy
{
    public class AndroidBuildSettings : LegacySettingBase
    {
        [FormerlySerializedAs("KEY_STORE_PATH")]
        public string KeystorePath = "keystore/keystore.keystore";
        [FormerlySerializedAs("KEY_STORE_PASS")]
        public string KeystorePassword = "";
        [FormerlySerializedAs("KEY_STORE_ALIAS")]
        public string KeystoreAlias = "";
        [FormerlySerializedAs("KEY_STORE_ALIAS_PASS")]
        public string KeystoreAliasPassword = "";
    }
}