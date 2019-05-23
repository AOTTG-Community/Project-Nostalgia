using System.Collections.Generic;
using UnityEngine;

namespace Optimization.Caching
{
    internal static class CacheResources
    {
        private static Dictionary<string, Object> cache = new Dictionary<string, Object>();
        private static Dictionary<string, GameObject> cacheRC = new Dictionary<string, GameObject>();
        private static Dictionary<string, Component> cacheType = new Dictionary<string, Component>();

        internal static void ClearCache()
        {
            cache = new Dictionary<string, Object>();
            cache.Clear();
            cacheType = new Dictionary<string, Component>();
            cacheType.Clear();
        }

        internal static Object Load(string path)
        {
            cache.TryGetValue(path, out Object obj);
            if (obj != null) return obj;
            if (cache.ContainsKey(path)) cache[path] = Resources.Load(path);
            else cache.Add(path, Resources.Load(path));
            return cache[path];
        }

        internal static T Load<T>(string path) where T : Component
        {
            cacheType.TryGetValue(path, out Component obj);
            if (obj != null) return obj as T;
            var go = (GameObject)Load(path);
            if (go.GetComponent<T>() != null)
            {
                if (cacheType.ContainsKey(path)) cacheType[path] = go.GetComponent<T>();
                else cacheType.Add(path, go.GetComponent<T>());
                return go.GetComponent<T>();
            }
            return default(T);
        }

        //Old version
        //internal static T Load<T>(string path) where T : Component
        //{
        //    string fullName = typeof(T).Name + path;
        //    _cacheType.TryGetValue(fullName, out Component obj);
        //    if (obj != null && obj is T) return obj as T;
        //    var go = (GameObject)Load(path);
        //    if (go.GetComponent<T>() != null)
        //    {
        //        if (_cacheType.ContainsKey(fullName)) _cacheType[fullName] = go.GetComponent<T>();
        //        else _cacheType.Add(fullName, go.GetComponent<T>());
        //        return go.GetComponent<T>();
        //    }
        //    return default(T);
        //}

        public static GameObject RCLoad(string _name)
        {
            string name = _name.StartsWith("RCAsset/") ? _name.Remove(0, 8) : _name;
            if (!CacheResources.cacheRC.ContainsKey(name))
            {
                return CacheResources.cacheRC[name] = (GameObject)RCManager.Asset.Load(name);
            }
            return CacheResources.cacheRC[name];
        }
        //string key = asset.StartsWith("RCAsset/") ? asset.Remove(0, 8) : asset;
        //cacheRC.TryGetValue(key, out GameObject res);
        //if (res != null) return res;
        //res = (GameObject)RCManager.Load(key);
        //if (res = null)
        //{
        //    cacheRC.Add(key, res);
        //    return res;
        //}
        //Debug.Log($"Unable to find RC Resource with name \"{key}\"");
        //return null;
    }
}