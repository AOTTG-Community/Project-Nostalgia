﻿using Optimization.Caching;
using System;
using System.Collections.Generic;
using UnityEngine;

public static class NGUITools
{
    private static float mGlobalVolume = 1f;

    private static Color mInvisible = new Color(0f, 0f, 0f, 0f);

    private static AudioListener mListener;

    private static bool mLoaded = false;

    public static string clipboard
    {
        get
        {
            return null;
        }
        set
        {
        }
    }

    public static bool fileAccess
    {
        get
        {
            return Application.platform != RuntimePlatform.WindowsWebPlayer && Application.platform != RuntimePlatform.OSXWebPlayer;
        }
    }

    public static float soundVolume
    {
        get
        {
            if (!NGUITools.mLoaded)
            {
                NGUITools.mLoaded = true;
                NGUITools.mGlobalVolume = PlayerPrefs.GetFloat("Sound", 1f);
            }
            return NGUITools.mGlobalVolume;
        }
        set
        {
            if (NGUITools.mGlobalVolume != value)
            {
                NGUITools.mLoaded = true;
                NGUITools.mGlobalVolume = value;
                PlayerPrefs.SetFloat("Sound", value);
            }
        }
    }

    private static void Activate(Transform t)
    {
        NGUITools.SetActiveSelf(t.gameObject, true);
        int i = 0;
        int childCount = t.childCount;
        while (i < childCount)
        {
            Transform child = t.GetChild(i);
            if (child.gameObject.activeSelf)
            {
                return;
            }
            i++;
        }
        int j = 0;
        int childCount2 = t.childCount;
        while (j < childCount2)
        {
            Transform child2 = t.GetChild(j);
            NGUITools.Activate(child2);
            j++;
        }
    }

    private static void Deactivate(Transform t)
    {
        NGUITools.SetActiveSelf(t.gameObject, false);
    }

    public static GameObject AddChild(GameObject parent)
    {
        GameObject gameObject = new GameObject();
        if (parent != null)
        {
            Transform transform = gameObject.transform;
            transform.parent = parent.transform;
            transform.localPosition = Vectors.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vectors.one;
            gameObject.layer = parent.layer;
        }
        return gameObject;
    }

    public static GameObject AddChild(GameObject parent, GameObject prefab)
    {
        GameObject gameObject = UnityEngine.Object.Instantiate(prefab) as GameObject;
        if (gameObject != null && parent != null)
        {
            Transform transform = gameObject.transform;
            transform.parent = parent.transform;
            transform.localPosition = Vectors.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vectors.one;
            gameObject.layer = parent.layer;
        }
        return gameObject;
    }

    public static T AddChild<T>(GameObject parent) where T : Component
    {
        GameObject gameObject = NGUITools.AddChild(parent);
        gameObject.name = NGUITools.GetName<T>();
        return gameObject.AddComponent<T>();
    }

    public static UISprite AddSprite(GameObject go, UIAtlas atlas, string spriteName)
    {
        UIAtlas.Sprite sprite = (!(atlas != null)) ? null : atlas.GetSprite(spriteName);
        UISprite uisprite = NGUITools.AddWidget<UISprite>(go);
        uisprite.type = ((sprite != null && !(sprite.inner == sprite.outer)) ? UISprite.Type.Sliced : UISprite.Type.Simple);
        uisprite.atlas = atlas;
        uisprite.spriteName = spriteName;
        return uisprite;
    }

    public static T AddWidget<T>(GameObject go) where T : UIWidget
    {
        int depth = NGUITools.CalculateNextDepth(go);
        T result = NGUITools.AddChild<T>(go);
        result.depth = depth;
        Transform transform = result.transform;
        transform.localPosition = Vectors.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = new Vector3(100f, 100f, 1f);
        result.gameObject.layer = go.layer;
        return result;
    }

    public static BoxCollider AddWidgetCollider(GameObject go)
    {
        if (go != null)
        {
            Collider component = go.GetComponent<Collider>();
            BoxCollider boxCollider = component as BoxCollider;
            if (boxCollider == null)
            {
                if (component != null)
                {
                    if (Application.isPlaying)
                    {
                        UnityEngine.Object.Destroy(component);
                    }
                    else
                    {
                        UnityEngine.Object.DestroyImmediate(component);
                    }
                }
                boxCollider = go.AddComponent<BoxCollider>();
            }
            int num = NGUITools.CalculateNextDepth(go);
            Bounds bounds = NGUIMath.CalculateRelativeWidgetBounds(go.transform);
            boxCollider.isTrigger = true;
            boxCollider.center = bounds.center + Vectors.back * ((float)num * 0.25f);
            boxCollider.size = new Vector3(bounds.size.x, bounds.size.y, 0f);
            return boxCollider;
        }
        return null;
    }

    public static Color ApplyPMA(Color c)
    {
        if (c.a != 1f)
        {
            c.r *= c.a;
            c.g *= c.a;
            c.b *= c.a;
        }
        return c;
    }

    public static void Broadcast(string funcName)
    {
        GameObject[] array = UnityEngine.Object.FindObjectsOfType(typeof(GameObject)) as GameObject[];
        int i = 0;
        int num = array.Length;
        while (i < num)
        {
            array[i].SendMessage(funcName, SendMessageOptions.DontRequireReceiver);
            i++;
        }
    }

    public static void Broadcast(string funcName, object param)
    {
        GameObject[] array = UnityEngine.Object.FindObjectsOfType(typeof(GameObject)) as GameObject[];
        int i = 0;
        int num = array.Length;
        while (i < num)
        {
            array[i].SendMessage(funcName, param, SendMessageOptions.DontRequireReceiver);
            i++;
        }
    }

    public static int CalculateNextDepth(GameObject go)
    {
        int num = -1;
        UIWidget[] componentsInChildren = go.GetComponentsInChildren<UIWidget>();
        int i = 0;
        int num2 = componentsInChildren.Length;
        while (i < num2)
        {
            num = Mathf.Max(num, componentsInChildren[i].depth);
            i++;
        }
        return num + 1;
    }

    public static void Destroy(UnityEngine.Object obj)
    {
        if (obj != null)
        {
            if (Application.isPlaying)
            {
                if (obj is GameObject)
                {
                    GameObject gameObject = obj as GameObject;
                    gameObject.transform.parent = null;
                }
                UnityEngine.Object.Destroy(obj);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(obj);
            }
        }
    }

    public static void DestroyImmediate(UnityEngine.Object obj)
    {
        if (obj != null)
        {
            if (Application.isEditor)
            {
                UnityEngine.Object.DestroyImmediate(obj);
            }
            else
            {
                UnityEngine.Object.Destroy(obj);
            }
        }
    }

    public static string EncodeColor(Color c)
    {
        int num = 16777215 & NGUIMath.ColorToInt(c) >> 8;
        return NGUIMath.DecimalToHex(num);
    }

    public static T[] FindActive<T>() where T : Component
    {
        return UnityEngine.Object.FindObjectsOfType(typeof(T)) as T[];
    }

    public static Camera FindCameraForLayer(int layer)
    {
        int num = 1 << layer;
        Camera[] array = NGUITools.FindActive<Camera>();
        int i = 0;
        int num2 = array.Length;
        while (i < num2)
        {
            Camera camera = array[i];
            if ((camera.cullingMask & num) != 0)
            {
                return camera;
            }
            i++;
        }
        return null;
    }

    public static T FindInParents<T>(GameObject go) where T : Component
    {
        if (go == null)
        {
            return (T)((object)null);
        }
        object obj = go.GetComponent<T>();
        if (obj == null)
        {
            Transform parent = go.transform.parent;
            while (parent != null && obj == null)
            {
                obj = parent.gameObject.GetComponent<T>();
                parent = parent.parent;
            }
        }
        return (T)((object)obj);
    }

    public static bool GetActive(GameObject go)
    {
        return go && go.activeInHierarchy;
    }

    public static string GetHierarchy(GameObject obj)
    {
        string text = obj.name;
        while (obj.transform.parent != null)
        {
            obj = obj.transform.parent.gameObject;
            text = obj.name + "/" + text;
        }
        return "\"" + text + "\"";
    }

    public static string GetName<T>() where T : Component
    {
        string text = typeof(T).ToString();
        if (text.StartsWith("UI"))
        {
            text = text.Substring(2);
        }
        else if (text.StartsWith("UnityEngine."))
        {
            text = text.Substring(12);
        }
        return text;
    }

    public static GameObject GetRoot(GameObject go)
    {
        Transform transform = go.transform;
        for (; ; )
        {
            Transform parent = transform.parent;
            if (parent == null)
            {
                break;
            }
            transform = parent;
        }
        return transform.gameObject;
    }

    public static bool IsChild(Transform parent, Transform child)
    {
        if (parent == null || child == null)
        {
            return false;
        }
        while (child != null)
        {
            if (child == parent)
            {
                return true;
            }
            child = child.parent;
        }
        return false;
    }

    public static byte[] Load(string fileName)
    {
        return null;
    }

    public static void MakePixelPerfect(Transform t)
    {
        UIWidget component = t.GetComponent<UIWidget>();
        if (component != null)
        {
            component.MakePixelPerfect();
        }
        else
        {
            t.localPosition = NGUITools.Round(t.localPosition);
            t.localScale = NGUITools.Round(t.localScale);
            int i = 0;
            int childCount = t.childCount;
            while (i < childCount)
            {
                NGUITools.MakePixelPerfect(t.GetChild(i));
                i++;
            }
        }
    }

    public static void MarkParentAsChanged(GameObject go)
    {
        UIWidget[] componentsInChildren = go.GetComponentsInChildren<UIWidget>();
        int i = 0;
        int num = componentsInChildren.Length;
        while (i < num)
        {
            componentsInChildren[i].ParentHasChanged();
            i++;
        }
    }

    public static WWW OpenURL(string url)
    {
        WWW result = null;
        try
        {
            result = new WWW(url);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
        }
        return result;
    }

    public static WWW OpenURL(string url, WWWForm form)
    {
        if (form == null)
        {
            return NGUITools.OpenURL(url);
        }
        WWW result = null;
        try
        {
            result = new WWW(url, form);
        }
        catch (Exception ex)
        {
            Debug.LogError((ex == null) ? "<null>" : ex.Message);
        }
        return result;
    }

    public static Color ParseColor(string text, int offset)
    {
        int num = NGUIMath.HexToDecimal(text[offset]) << 4 | NGUIMath.HexToDecimal(text[offset + 1]);
        int num2 = NGUIMath.HexToDecimal(text[offset + 2]) << 4 | NGUIMath.HexToDecimal(text[offset + 3]);
        int num3 = NGUIMath.HexToDecimal(text[offset + 4]) << 4 | NGUIMath.HexToDecimal(text[offset + 5]);
        float num4 = 0.003921569f;
        return new Color(num4 * (float)num, num4 * (float)num2, num4 * (float)num3);
    }

    public static int ParseSymbol(string text, int index, List<Color> colors, bool premultiply)
    {
        int length = text.Length;
        if (index + 2 < length)
        {
            if (text[index + 1] == '-')
            {
                if (text[index + 2] == ']')
                {
                    if (colors != null && colors.Count > 1)
                    {
                        colors.RemoveAt(colors.Count - 1);
                    }
                    return 3;
                }
            }
            else if (index + 7 < length && text[index + 7] == ']')
            {
                if (colors != null)
                {
                    Color color = NGUITools.ParseColor(text, index + 1);
                    if (NGUITools.EncodeColor(color) != text.Substring(index + 1, 6).ToUpper())
                    {
                        return 0;
                    }
                    color.a = colors[colors.Count - 1].a;
                    if (premultiply && color.a != 1f)
                    {
                        color = Color.Lerp(NGUITools.mInvisible, color, color.a);
                    }
                    colors.Add(color);
                }
                return 8;
            }
        }
        return 0;
    }

    public static AudioSource PlaySound(AudioClip clip)
    {
        return NGUITools.PlaySound(clip, 1f, 1f);
    }

    public static AudioSource PlaySound(AudioClip clip, float volume)
    {
        return NGUITools.PlaySound(clip, volume, 1f);
    }

    public static AudioSource PlaySound(AudioClip clip, float volume, float pitch)
    {
        volume *= NGUITools.soundVolume;
        if (clip != null && volume > 0.01f)
        {
            if (NGUITools.mListener == null)
            {
                NGUITools.mListener = (UnityEngine.Object.FindObjectOfType(typeof(AudioListener)) as AudioListener);
                if (NGUITools.mListener == null)
                {
                    Camera camera = IN_GAME_MAIN_CAMERA.BaseCamera;
                    if (camera == null)
                    {
                        camera = (UnityEngine.Object.FindObjectOfType(typeof(Camera)) as Camera);
                    }
                    if (camera != null)
                    {
                        NGUITools.mListener = camera.gameObject.AddComponent<AudioListener>();
                    }
                }
            }
            if (NGUITools.mListener != null && NGUITools.mListener.enabled && NGUITools.GetActive(NGUITools.mListener.gameObject))
            {
                AudioSource audioSource = NGUITools.mListener.audio;
                if (audioSource == null)
                {
                    audioSource = NGUITools.mListener.gameObject.AddComponent<AudioSource>();
                }
                audioSource.pitch = pitch;
                audioSource.PlayOneShot(clip, volume);
                return audioSource;
            }
        }
        return null;
    }

    public static int RandomRange(int min, int max)
    {
        if (min == max)
        {
            return min;
        }
        return UnityEngine.Random.Range(min, max + 1);
    }

    public static Vector3 Round(Vector3 v)
    {
        v.x = Mathf.Round(v.x);
        v.y = Mathf.Round(v.y);
        v.z = Mathf.Round(v.z);
        return v;
    }

    public static bool Save(string fileName, byte[] bytes)
    {
        return false;
    }

    public static void SetActive(GameObject go, bool state)
    {
        if (state)
        {
            NGUITools.Activate(go.transform);
        }
        else
        {
            NGUITools.Deactivate(go.transform);
        }
    }

    public static void SetActiveChildren(GameObject go, bool state)
    {
        Transform transform = go.transform;
        if (state)
        {
            int i = 0;
            int childCount = transform.childCount;
            while (i < childCount)
            {
                Transform child = transform.GetChild(i);
                NGUITools.Activate(child);
                i++;
            }
        }
        else
        {
            int j = 0;
            int childCount2 = transform.childCount;
            while (j < childCount2)
            {
                Transform child2 = transform.GetChild(j);
                NGUITools.Deactivate(child2);
                j++;
            }
        }
    }

    public static void SetActiveSelf(GameObject go, bool state)
    {
        go.SetActive(state);
    }

    public static void SetLayer(GameObject go, int layer)
    {
        go.layer = layer;
        Transform transform = go.transform;
        int i = 0;
        int childCount = transform.childCount;
        while (i < childCount)
        {
            Transform child = transform.GetChild(i);
            NGUITools.SetLayer(child.gameObject, layer);
            i++;
        }
    }

    public static string StripSymbols(string text)
    {
        if (text != null)
        {
            int i = 0;
            int length = text.Length;
            while (i < length)
            {
                char c = text[i];
                if (c == '[')
                {
                    int num = NGUITools.ParseSymbol(text, i, null, false);
                    if (num > 0)
                    {
                        text = text.Remove(i, num);
                        length = text.Length;
                        continue;
                    }
                }
                i++;
            }
        }
        return text;
    }
}