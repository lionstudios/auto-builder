using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace LionStudios.Editor.AutoBuilder.AdapterStabilizer
{

    public class AdNetwork
    {
        public string NetworkName { get; private set; }
        public string NetworkCodeName { get; private set; }
        public Version AndroidBuild { get; private set; }
        public Version IOSBuild { get; private set; }
        public List<Version> AndroidBrokens { get; private set; }
        public List<Version> IOSBrokens { get; private set; }
        public string PrivacyLinks { get; private set; }

        public Version InstalledAndroidVersion { get; private set; }
        public Version InstalledIosVersion { get; private set; }

        public AdNetwork(string networkName, string networkCodeName, string androidBuild, string iosBuild, string androidBrokens, string iOSBrokens, string privacyLinks)
        {
            NetworkName = networkName;
            NetworkCodeName = networkCodeName;
            if (!string.IsNullOrEmpty(androidBuild))
                AndroidBuild = new Version(androidBuild);
            if (!string.IsNullOrEmpty(iosBuild))
                IOSBuild = new Version(iosBuild);
            AndroidBrokens = StringToVersionList(androidBrokens);
            IOSBrokens = StringToVersionList(iOSBrokens);
            PrivacyLinks = privacyLinks;
        }

        List<Version> StringToVersionList(string s)
        {
            List<Version> res = new List<Version>();
            if (string.IsNullOrEmpty(s))
                return res;
            foreach (string item in s.Split(new char[] { ';', '-' }))
            {
                try
                {
                    res.Add(new Version(item));
                }
                catch { }
            }
            return res;
        }

        public void SetInstalledVersions(string android, string ios)
        {
            if (!string.IsNullOrEmpty(android))
                InstalledAndroidVersion = new Version(android);
            if (!string.IsNullOrEmpty(ios))
                InstalledIosVersion = new Version(ios);
        }

        public bool IsMatchingLatestStableIOS()
        {
            return (IOSBuild == null ||
                    InstalledIosVersion == null ||
                    IOSBuild <= InstalledIosVersion && !IOSBrokens.Contains(InstalledIosVersion));
        }

        public bool IsMatchingLatestStableAndroid()
        {
            return (AndroidBuild == null ||
                    InstalledAndroidVersion == null ||
                    AndroidBuild <= InstalledAndroidVersion && !AndroidBrokens.Contains(InstalledAndroidVersion));
        }

        public bool IsMatchingLatestStable()
        {
            return IsMatchingLatestStableAndroid() && IsMatchingLatestStableIOS();
        }

        public override string ToString()
        {
            return NetworkName + " (" + NetworkCodeName + ")   ->   Android: " + (InstalledAndroidVersion?.ToString() ?? "none") + "    ;   iOS: " + (InstalledIosVersion?.ToString() ?? "none") + "   ;   Recommended   ->   Android: " + (AndroidBuild?.ToString() ?? "none") + "    ;   iOS: " + (IOSBuild?.ToString() ?? "none")  + "   ;   Broken   ->   Android: " + GetAndroidBrokensString() + "    ;   iOS: " + GetIOSBrokensString();
        }

        public string GetAndroidBrokensString()
        {
            if (AndroidBrokens == null)
                return string.Empty;
            return string.Join(" ; ", AndroidBrokens.Select(v => v.ToString()));
        }
        
        public string GetIOSBrokensString()
        {
            if (IOSBrokens == null)
                return string.Empty;
            return string.Join(" ; ", IOSBrokens.Select(v => v.ToString()));
        }
        
    }

    public static class LionMaxAdapterStabiliser
    {
        
        private const string ADAPTERS_LIST_URL = "https://docs.google.com/spreadsheets/d/e/2PACX-1vRwPfavl63ZqYVo-ChQ81em5zRtJMeSyE5c-7QWcN4qdnu_zCiQGAebk7_a2n22p_1WT6A7ELAfW8-f/pub?gid=0&single=true&output=csv";
        
        public static List<AdNetwork> AdNetworks;

        public static List<AdNetwork> GetNonMatchingNetworks()
        {
            if (AdNetworks == null)
            {
                Debug.LogError("Trying to get non matching ad network versions, but MaxAdapterStabiliser is not initialised");
                return new List<AdNetwork>();
            }
            List<AdNetwork> result = new List<AdNetwork>();
            foreach (AdNetwork adNetwork in AdNetworks)
            {
                result.Add(adNetwork);
            }
            return result;
        }

        [InitializeOnLoadMethod]
        static async void OnLoad()
        {
            if (!Application.isBatchMode)
            {
                bool initResult = await InitAdNetworkData();
                if (initResult)
                {
                    DisplayDialogIfMismatchFound();
                }
            }
        }

        public static async Task<bool> InitAdNetworkData()
        {
            try
            {
                string csvText = await DownloadCSVFileAsync(ADAPTERS_LIST_URL);
                if (string.IsNullOrEmpty(csvText))
                {
                    return false;
                }

                AdNetworks = ParseCSV(csvText);
                Debug.Log("Parsed ad network data: " + AdNetworks.Count + " entries found.");

                LoadInstalledNetworkVersions(false);

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                return false;
            }
        }

        public static List<string> GetMismatchIosAdapterNames()
        {
            List<string> result = new List<string>();
            foreach (AdNetwork adNetwork in AdNetworks)
            {
                if (!adNetwork.IsMatchingLatestStableIOS())
                {
                    result.Add(adNetwork.NetworkCodeName);
                }
            }
            return result;
        }

        public static List<string> GetMismatchAndroidAdapterNames()
        {
            List<string> result = new List<string>();
            foreach (AdNetwork adNetwork in AdNetworks)
            {
                if (!adNetwork.IsMatchingLatestStableAndroid())
                {
                    result.Add(adNetwork.NetworkCodeName);
                }
            }
            return result;
        }

        static void DisplayDialogIfMismatchFound()
        {
            bool hasVersionsMismatch = false;
            foreach (AdNetwork adNetwork in AdNetworks)
            {
                if (adNetwork.IsMatchingLatestStable() == false)
                {
                    hasVersionsMismatch = true;
                    break;
                }
            }
            if (hasVersionsMismatch)
            {
                bool showDialog = !SessionState.GetBool("AdNetworkVersionsMismatchOptOut", false);
                if (showDialog)
                {
                    int result = EditorUtility.DisplayDialogComplex("Outdated or broken ad adapters",
                        "Some adapters are out of date. Do you want to update them?",
                        "Review & Update",
                        "Ignore",
                        "Ignore - Do not ask again for this session");
                    if (result == 0)
                    {
                        LionMaxAdapterServiceWindow.ShowWindow();
                    }
                    else if (result == 2)
                    {
                        SessionState.SetBool("AdNetworkVersionsMismatchOptOut", true);
                    }
                }
            }
        }

        static async Task<string> DownloadCSVFileAsync(string url)
        {
            using (UnityWebRequest www = UnityWebRequest.Get(url))
            {

                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                
                www.SetRequestHeader("Content-Type", "text/plain");

                var operation = www.SendWebRequest();
                //wait for complete
                while (!operation.isDone)
                    await Task.Yield();

#if UNITY_2020_3_OR_NEWER
                if (www.result != UnityWebRequest.Result.Success)
#else
            if (www.isNetworkError || www.isHttpError || !string.IsNullOrWhiteSpace(www.error))
#endif

                {
                    Debug.LogError("Failed to download CSV file: " + www.error);
                    return null;
                }
                else
                {
                    // Get the downloaded CSV text
                    string csvText = www.downloadHandler.text;
                    return csvText;
                }
            }
        }

        static List<AdNetwork> ParseCSV(string csvText)
        {
            List<AdNetwork> adNetworks = new List<AdNetwork>();

            using (StringReader reader = new StringReader(csvText))
            {
                for (int i = 0; i < 5; i++)
                {
                    reader.ReadLine();
                }

                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    try
                    {
                        string[] values = line.Split(',');
                        string networkName = values[0];
                        string networkCodeName = values[1];
                        string androidBuild = values[2];
                        string iosBuild = values[3];
                        string androidBrokens = values[4];
                        string iosBrokens = values[5];
                        string privacyLinks = values[6];

                        if (string.IsNullOrEmpty(networkCodeName))
                            continue;

                        AdNetwork adNetwork = new AdNetwork(networkName, networkCodeName, androidBuild, iosBuild, androidBrokens, iosBrokens, privacyLinks);
                        adNetworks.Add(adNetwork);
                    }
                    catch (Exception ex)
                    {
                        continue;
                    }
                }
            }

            return adNetworks;
        }

        static Type GetType(string typeName)
        {
            var type = Type.GetType(typeName);
            if (type != null) return type;

            foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = a.GetType(typeName);
                if (type != null)
                    return type;
            }
            
            return null;
        }

        public static void AddPrivacyLinks()
        {
            Type applovinInternalSettingsType = GetType("AppLovinMax.Scripts.IntegrationManager.Editor.AppLovinInternalSettings");
            if (applovinInternalSettingsType == null)
            {
                Debug.LogError("Error adding privacy links: AppLovinInternalSettings class not found");
                return;
            }

            PropertyInfo instanceProperty = applovinInternalSettingsType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
            if (instanceProperty == null)
            {
                Debug.LogError("Error adding privacy links: AppLovinInternalSettings.Instance property not found");
                return;
            }

            object applovinInternalSettings = instanceProperty.GetValue(null, null);

            FieldInfo field = applovinInternalSettingsType.GetField("consentFlowAdvertisingPartnerUrls", BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null)
            {
                Debug.LogError("Error adding privacy links: AppLovinInternalSettings.Instance.consentFlowAdvertisingPartnerUrls variable not found");
                return;
            }
            
            List<string> privacyLinks = new List<string>();
            AdNetwork applovinNetwork = AdNetworks.FirstOrDefault(x => x.NetworkCodeName == "AppLovin");
            if(applovinNetwork != null)
            {
                privacyLinks.Add(applovinNetwork.PrivacyLinks);
            }

            Type amazonAdsType = GetType("AmazonAds.Amazon");
            if(amazonAdsType != null)
            {
                AdNetwork amazonNetwork = AdNetworks.FirstOrDefault(x => x.NetworkCodeName == "Amazon");
                if(amazonNetwork != null)
                {
                    privacyLinks.Add(amazonNetwork.PrivacyLinks);
                }
            }

            foreach (AdNetwork adNetwork in AdNetworks)
            {
                if (adNetwork.InstalledAndroidVersion == null && adNetwork.InstalledIosVersion == null)
                {
                    continue;
                }
                if(string.IsNullOrEmpty(adNetwork.PrivacyLinks))
                {
                    continue;
                }
                privacyLinks.Add(adNetwork.PrivacyLinks);
            }
            string combinedPrivacyLinks = string.Join(",", privacyLinks.ToArray());
            string currentPrivacyLinks = (string)field.GetValue(applovinInternalSettings);
            field.SetValue(applovinInternalSettings, combinedPrivacyLinks);

            MethodInfo saveMethod = applovinInternalSettingsType.GetMethod("Save", BindingFlags.Public | BindingFlags.Instance);
            if (saveMethod == null)
            {
                Debug.LogError("Error adding privacy links: AppLovinInternalSettings.Instance.Save method not found");
                return;
            }
            saveMethod.Invoke(applovinInternalSettings, null);
        }

        public static void LoadInstalledNetworkVersions(bool fix)
        {
            string mediationFolderPath = "/MaxSdk/Mediation";
            string mediationFolderFullPath = Application.dataPath + mediationFolderPath;
            if (!Directory.Exists(mediationFolderFullPath))
            {
                Debug.LogWarning("Max sdk not found");
                return;
            }

            DirectoryInfo projectDir = new DirectoryInfo(Application.dataPath).Parent;
            string[] directoryPaths = Directory.GetDirectories(mediationFolderFullPath, "*", SearchOption.TopDirectoryOnly);

            foreach (string directoryPath in directoryPaths)
            {
                string adNetworkName = Path.GetFileName(directoryPath);

                string editorFolderPath = Path.Combine(directoryPath, "Editor");
                string dependenciesFilePath = Path.Combine(editorFolderPath, "Dependencies.xml");

                string installedAndroidVersion = "";
                string installedIosVersion = "";

                if (File.Exists(dependenciesFilePath))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(dependenciesFilePath);

                    XmlNode androidPackagesNode = doc.SelectSingleNode("//androidPackages");
                    installedAndroidVersion = ProcessAndroidPackagesNode(androidPackagesNode, adNetworkName, fix);

                    XmlNode iosPodsNode = doc.SelectSingleNode("//iosPods");
                    installedIosVersion = ProcessIosPodsNode(iosPodsNode, adNetworkName, fix);

                    if (fix)
                    {
                        doc.Save(dependenciesFilePath);
                    }
                }

                AdNetwork currentNetwork = AdNetworks.Find(x => x.NetworkCodeName == adNetworkName);
                if (currentNetwork == null)
                {
                    currentNetwork = new AdNetwork(adNetworkName, adNetworkName, "", "", "", "", "");
                    AdNetworks.Add(currentNetwork);
                }
                currentNetwork.SetInstalledVersions(installedAndroidVersion, installedIosVersion);
            }

            // The Applovin part below was commented out because it can cause issues with mismatches between MAX version and Applovin adapter version
            // If a solution to these issues is figured out, you can uncomment this code

            //Applovin is handled separately
            // string applovinDependenciesFilePath = Application.dataPath + "/MaxSdk/AppLovin/Editor/Dependencies.xml";
            // if (File.Exists(applovinDependenciesFilePath))
            // {
            //     XmlDocument doc = new XmlDocument();
            //     doc.Load(applovinDependenciesFilePath);
            //
            //     string installedAndroidVersion = "";
            //     string installedIosVersion = "";
            //
            //     XmlNode androidPackagesNode = doc.SelectSingleNode("//androidPackages");
            //     installedAndroidVersion = ProcessAndroidPackagesNode(androidPackagesNode, "AppLovin", fix);
            //
            //     XmlNode iosPodsNode = doc.SelectSingleNode("//iosPods");
            //     installedIosVersion = ProcessIosPodsNode(iosPodsNode, "AppLovin", fix);
            //
            //     if (fix)
            //     {
            //         doc.Save(applovinDependenciesFilePath);
            //     }
            //
            //     AdNetwork currentNetwork = AdNetworks.Find(x => x.NetworkCodeName == "AppLovin");
            //     if (currentNetwork == null)
            //     {
            //         currentNetwork = new AdNetwork("AppLovin", "AppLovin", "", "", DateTime.Now, "");
            //         AdNetworks.Add(currentNetwork);
            //     }
            //     currentNetwork.SetInstalledVersions(installedAndroidVersion, installedIosVersion);
            // }

            string recap = "Installed ad network versions:\n";
            foreach (AdNetwork adNetwork in AdNetworks)
            {
                recap += adNetwork.ToString() + "\n";
            }
            Debug.Log(recap);
        }

        private static string ProcessIosPodsNode(XmlNode iosPodsNode, string adNetworkName, bool fix)
        {
            string installedIosVersion = "";
            if (iosPodsNode != null)
            {
                foreach (XmlNode podNode in iosPodsNode.ChildNodes)
                {
                    if (podNode.Name != "iosPod")
                    {
                        continue;
                    }
                    if (podNode.Attributes["name"] != null && podNode.Attributes["version"] != null)
                    {
                        string name = podNode.Attributes["name"].Value;
                        string version = podNode.Attributes["version"].Value;

                        if (fix)
                        {
                            AdNetwork currentAdNetwork = AdNetworks.Find(x => x.NetworkCodeName == adNetworkName);
                            if (currentAdNetwork != null && currentAdNetwork.IOSBuild != null)
                            {
                                version = currentAdNetwork.IOSBuild.ToString();
                                podNode.Attributes["version"].Value = version;
                            }
                        }

                        installedIosVersion = version;
                    }
                }
            }
            return installedIosVersion;
        }

        private static string ProcessAndroidPackagesNode(XmlNode androidPackagesNode, string adNetworkName, bool fix)
        {
            string installedAndroidVersion = "";
            if (androidPackagesNode != null)
            {
                foreach (XmlNode packageNode in androidPackagesNode.ChildNodes)
                {
                    if (packageNode.Name != "androidPackage")
                    {
                        continue;
                    }
                    if (packageNode.Attributes["spec"] != null)
                    {
                        string spec = packageNode.Attributes["spec"].Value;
                        string[] parts = spec.Split(':');
                        if (parts.Length >= 3)
                        {
                            string codeName = parts[1];
                            string version = parts[2];
                            string realVersion = version;
                            // Some networks write version in brackets. Handle these.
                            if (version.StartsWith("[") && version.EndsWith("]"))
                            {
                                realVersion = version.Substring(1, version.Length - 2);
                            }

                            if (fix)
                            {
                                AdNetwork currentAdNetwork = AdNetworks.Find(x => x.NetworkCodeName == adNetworkName);
                                if (currentAdNetwork != null && currentAdNetwork.AndroidBuild != null)
                                {
                                    realVersion = currentAdNetwork.AndroidBuild.ToString();
                                    if (version.StartsWith("[") && version.EndsWith("]"))
                                    {
                                        version = "[" + realVersion + "]";
                                    }
                                    else
                                    {
                                        version = realVersion;
                                    }
                                    string updatedSpec = parts[0] + ":" + codeName + ":" + version;
                                    packageNode.Attributes["spec"].Value = updatedSpec;
                                }
                            }
                            installedAndroidVersion = realVersion;
                        }
                        break;
                    }
                }
            }
            return installedAndroidVersion;
        }
    }

}
