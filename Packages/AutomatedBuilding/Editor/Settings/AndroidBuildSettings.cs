using UnityEngine;

namespace AutomatedBuilding
{
    [CreateAssetMenu(fileName = "AndroidBuildSettings", menuName = "Build/CreateAndroidBuildSettings")]
    public class AndroidBuildSettings : ScriptableObject
    {
        public string KEY_STORE_PATH = "keystore/keystore.keystore";
        public string KEY_STORE_PASS = "";
        public string KEY_STORE_ALIAS = "";
        public string KEY_STORE_ALIAS_PASS = "";
        
        public string AdditionalDefineSymbols;
    }
}