using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace Optimization
{
    class RCManager
    {
        public static readonly string CachePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/AoTTG/RCAssets.unity3d";
        public static readonly string DirectoryPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/AoTTG/";
        public const string DownloadPath = "https://www.dropbox.com/s/tvbq6za3r11xtp8/RCAssets.unity3d?dl=1";
        
        public static AssetBundle Asset;
        public static ExitGames.Client.Photon.Hashtable boolVariables = new ExitGames.Client.Photon.Hashtable();
        public static ExitGames.Client.Photon.Hashtable floatVariables = new ExitGames.Client.Photon.Hashtable();
        public static int GameType = 3;
        public static ExitGames.Client.Photon.Hashtable heroHash = new ExitGames.Client.Photon.Hashtable();
        public static ExitGames.Client.Photon.Hashtable intVariables = new ExitGames.Client.Photon.Hashtable();
        public static bool Loaded { get; private set; } = false;
        public static ExitGames.Client.Photon.Hashtable playerVariables = new ExitGames.Client.Photon.Hashtable();
        public static ExitGames.Client.Photon.Hashtable stringVariables = new ExitGames.Client.Photon.Hashtable();
        //This is gametype. Change it in selection grid to switch between racing, killing, etc...
        public static ExitGames.Client.Photon.Hashtable titanVariables = new ExitGames.Client.Photon.Hashtable();
        public static Vector3 racingSpawnPoint = Caching.Vectors.zero;
        public static Quaternion racingSpawnPointRotation;
        public static bool racingSpawnPointSet;
        public static ExitGames.Client.Photon.Hashtable RCEvents = new ExitGames.Client.Photon.Hashtable();
        public static ExitGames.Client.Photon.Hashtable RCRegions = new ExitGames.Client.Photon.Hashtable();
        public static ExitGames.Client.Photon.Hashtable RCRegionTriggers = new ExitGames.Client.Photon.Hashtable();
        public static ExitGames.Client.Photon.Hashtable RCVariableNames = new ExitGames.Client.Photon.Hashtable();
        //Also changable variable. Use it to change titans amount on a custom map
        public static int SpawnCupCustom = 1;


        public static void Clear()
        {
            boolVariables.Clear();
            floatVariables.Clear();
            heroHash.Clear();
            intVariables.Clear();
            playerVariables.Clear();
            stringVariables.Clear();
            titanVariables.Clear();
            RCEvents.Clear();
            RCRegions.Clear();
            RCRegionTriggers.Clear();
            RCVariableNames.Clear();
        }

        public static IEnumerator DownloadAssets()
        {
            if (Loaded)
            {
                Caching.CacheGameObject.Find<UILabel>("VERSION").text = UIMainReferences.VersionShow;
                yield break;
            }
            if (File.Exists(CachePath))
            {
                var req = AssetBundle.CreateFromMemory(File.ReadAllBytes(CachePath));
                yield return req;
                if (req == null || req.assetBundle == null) { }
                else
                {
                    Asset = req.assetBundle;
                    Loaded = true;
                    Caching.CacheGameObject.Find<UILabel>("VERSION").text = UIMainReferences.VersionShow;
                    yield break;
                }
            }
            Caching.CacheGameObject.Find<UILabel>("VERSION").text = "Downloading RCAssets...";
            WWW www = new WWW(DownloadPath);
            yield return www;
            if (www.assetBundle != null)
            {
                Asset = www.assetBundle;
                if (!Directory.Exists(DirectoryPath))
                {
                    Directory.CreateDirectory(DirectoryPath);
                }
                File.WriteAllBytes(CachePath, www.bytes);
                Caching.CacheGameObject.Find<UILabel>("VERSION").text = "RCAssets loaded.";
                yield return new WaitForSeconds(2.0f);
                Caching.CacheGameObject.Find<UILabel>("VERSION").text = UIMainReferences.VersionShow;
                Loaded = true;
            }
        }

        public static GameObject Instantiate(string name)
        {
            return Caching.CacheResources.RCLoad(name);
        }

        public static UnityEngine.Object Load(string res)
        {
            return Asset.Load(res);
        }
    }
}
