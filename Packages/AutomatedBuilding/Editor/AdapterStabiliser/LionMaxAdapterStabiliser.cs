using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Xml;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;


public class AdNetwork
{
    public string NetworkName { get; private set; }
    public string NetworkCodeName { get; private set; }
    public string AndroidBuild { get; private set; }
    public string IOSBuild { get; private set; }
    public DateTime DateUpdated { get; private set; }
    public string Comments { get; private set; }

    public string InstalledAndroidVersion { get; private set; }
    public string InstalledIosVersion { get; private set; }

    public AdNetwork(string networkName, string networkCodeName, string androidBuild, string iosBuild, DateTime dateUpdated, string comments)
    {
        NetworkName = networkName;
        NetworkCodeName = networkCodeName;
        AndroidBuild = androidBuild;
        IOSBuild = iosBuild;
        DateUpdated = dateUpdated; 
        Comments = comments;
    }

    public void SetInstalledVersions(string android, string ios)
    {
        InstalledAndroidVersion = android;
        InstalledIosVersion = ios;
    }

    public bool IsMatchingLatestStable()
    {
        if (!string.IsNullOrEmpty(AndroidBuild) &&
            !string.IsNullOrEmpty(InstalledAndroidVersion) &&
            AndroidBuild != InstalledAndroidVersion)
            return false;
        if (!string.IsNullOrEmpty(IOSBuild) &&
            !string.IsNullOrEmpty(InstalledIosVersion) &&
            IOSBuild != InstalledIosVersion) 
            return false;
        return true;
    }
}

public static class LionMaxAdapterStabiliser
{
    public class CertificateCheckBypasser : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certificateData)
        {
            return true; 
        }
    }
    public static List<AdNetwork> AdNetworks;

    public static List<AdNetwork> GetNonMatchingNetworks()
    {
        if(AdNetworks == null)
        {
            Debug.LogError("Trying to get non matching ad network versions, but MaxAdapterStabiliser is not initialised");
            return new List<AdNetwork>();
        }
        List<AdNetwork> result = new List<AdNetwork>();
        foreach(AdNetwork adNetwork in AdNetworks)
        {
            result.Add(adNetwork);
        }
        return result;
    }

    [InitializeOnLoadMethod]
    static async void OnLoad()
    {
        if(!Application.isBatchMode)
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
            // Use this url for testing.
            string testUrl = Application.dataPath;
            testUrl = "file://" + testUrl.Replace("/Assets", "/Packages") + "/LionSuite-SDKService/Resources/TestStableAdapterList.csv";

            // Real google docs url
            string url = "https://docs.google.com/spreadsheets/d/e/2PACX-1vRwPfavl63ZqYVo-ChQ81em5zRtJMeSyE5c-7QWcN4qdnu_zCiQGAebk7_a2n22p_1WT6A7ELAfW8-f/pub?gid=0&single=true&output=csv";

            string csvText = await DownloadCSVFileAsync(url);
            if (string.IsNullOrEmpty(csvText))
            {
                return false;
            }

            AdNetworks = ParseCSV(csvText);
            Debug.Log("Parsed ad network data: " + AdNetworks.Count + " entries found.");

            LoadInstalledNetworkVersions(false);

            return true;
        }
        catch(Exception e)
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
            if(!string.IsNullOrEmpty(adNetwork.InstalledIosVersion) &&
                !string.IsNullOrEmpty(adNetwork.IOSBuild) &&
                adNetwork.InstalledIosVersion != adNetwork.IOSBuild)
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
            if (!string.IsNullOrEmpty(adNetwork.InstalledIosVersion) &&
                !string.IsNullOrEmpty(adNetwork.IOSBuild) &&
                adNetwork.InstalledIosVersion != adNetwork.IOSBuild)
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
            if(adNetwork.IsMatchingLatestStable() == false)
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
                int result = EditorUtility.DisplayDialogComplex("Ad Network Versions Mismatch",
                                                          "The adapters are not in the latest stable versions defined by Lion QA. Do you want to update the adapters?",
                                                          "Update",
                                                          "Cancel",
                                                          "Cancel - Do not ask again for this session");
                if (result == 0)
                {
                    LoadInstalledNetworkVersions(true);
                }
                else if(result == 2) 
                {
                    SessionState.SetBool("AdNetworkVersionsMismatchOptOut", true);
                }
            }
        }
    }

    public static void WriteTestCsv()
    {
        List<string> allLines = new List<string>();
        string filePath = Application.dataPath.Replace("/Assets", "/Packages") + "/LionSuite-SDKService/Resources/TestList.csv";

        for (int i = 0; i < 5; i++)
        {
            allLines.Add("SomeData");
        }

        foreach(AdNetwork adNetwork in AdNetworks)
        {
            string line = adNetwork.NetworkName + "," + adNetwork.NetworkCodeName + "," + adNetwork.InstalledAndroidVersion + ".TEST" + "," + adNetwork.InstalledIosVersion + ".TEST" + "," + adNetwork.DateUpdated.ToShortDateString() + "," + adNetwork.Comments;
            allLines.Add(line);
        }


        File.WriteAllLines(filePath, allLines);

    }

    static async Task<string> DownloadCSVFileAsync(string url)
    {
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            www.certificateHandler = new CertificateCheckBypasser(); 
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
                string[] values = line.Split(',');
                if(values.Length == 1)
                {
                    // Continuation from previous line, probably newline in comments cell. Ignore
                    continue;
                }

                string networkName = values[0];
                string networkCodeName = values[1];
                string androidBuild = values[2];
                string iosBuild = values[3]; 

                // This line is an exception
                if (values[4] == "Date Updated")
                {
                    continue;
                }
                try
                {
                    DateTime dateUpdated = DateTime.ParseExact(values[4], "M/d/yyyy", CultureInfo.InvariantCulture);
                    string comments = values[5];

                    AdNetwork adNetwork = new AdNetwork(networkName, networkCodeName, androidBuild, iosBuild, dateUpdated, comments);
                    adNetworks.Add(adNetwork);
                }
                catch(FormatException ex)
                {
                    continue;
                }
            }
        }

        return adNetworks;
    } 

    public static void LoadInstalledNetworkVersions(bool fix)
    {
        string mediationFolderPath = "/MaxSdk/Mediation";
        string mediationFolderFullPath = Application.dataPath + mediationFolderPath;
        if(!Directory.Exists(mediationFolderFullPath))
        {
            Debug.LogError("Max sdk not found");
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

                if(fix)
                {
                    doc.Save(dependenciesFilePath);
                }
            }

            AdNetwork currentNetwork = AdNetworks.Find(x => x.NetworkCodeName == adNetworkName);
            if(currentNetwork == null)
            {
                currentNetwork = new AdNetwork(adNetworkName, adNetworkName, "", "", DateTime.Now, "");
                AdNetworks.Add(currentNetwork);
            }
            currentNetwork.SetInstalledVersions(installedAndroidVersion, installedIosVersion);
        }

        //Applovin is handled separately
        string applovinDependenciesFilePath = Application.dataPath + "/MaxSdk/AppLovin/Editor/Dependencies.xml";
        if (File.Exists(applovinDependenciesFilePath))
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(applovinDependenciesFilePath);

            string installedAndroidVersion = "";
            string installedIosVersion = "";

            XmlNode androidPackagesNode = doc.SelectSingleNode("//androidPackages");
            installedAndroidVersion = ProcessAndroidPackagesNode(androidPackagesNode, "AppLovin", fix);

            XmlNode iosPodsNode = doc.SelectSingleNode("//iosPods");
            installedIosVersion = ProcessIosPodsNode(iosPodsNode, "AppLovin", fix);

            if (fix)
            {
                doc.Save(applovinDependenciesFilePath);
            }

            AdNetwork currentNetwork = AdNetworks.Find(x => x.NetworkCodeName == "AppLovin");
            if (currentNetwork == null)
            {
                currentNetwork = new AdNetwork("AppLovin", "AppLovin", "", "", DateTime.Now, "");
                AdNetworks.Add(currentNetwork);
            }
            currentNetwork.SetInstalledVersions(installedAndroidVersion, installedIosVersion);
        }


        Debug.Log("Installed ad network versions:");
        foreach (AdNetwork adNetwork in AdNetworks)
        {
            Debug.Log(adNetwork.NetworkName + " " + adNetwork.NetworkCodeName + " Android: " + adNetwork.InstalledAndroidVersion + " iOS: " + adNetwork.InstalledIosVersion);
        }
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
                        if (currentAdNetwork != null && !string.IsNullOrEmpty(currentAdNetwork.IOSBuild))
                        {
                            version = currentAdNetwork.IOSBuild;
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
                            if (currentAdNetwork != null && !string.IsNullOrEmpty(currentAdNetwork.AndroidBuild))
                            {
                                realVersion = currentAdNetwork.AndroidBuild;
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
