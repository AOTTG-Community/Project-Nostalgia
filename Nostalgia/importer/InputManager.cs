// Decompiled with JetBrains decompiler
// Type: BRS.InputManager
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: EA706D30-A741-450F-BB06-DFDA2E2B50C1
// Assembly location: G:\Games x2\AoTTG\BRS Mod v4\BRS Mod v4.0.7_Data\Managed\Assembly-CSharp.dll

using ns11;
using ns22;
using ns8;
using ns9;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace BRS
{
  public class InputManager : MonoBehaviour
  {
    private static string string_0 = "";
    public static bool menuOn = false;
    private static Dictionary<string, InputManager.InputInfo> dictionary_0;
    private static Dictionary<string, KeyCode> dictionary_1;
    private static Dictionary<string, string> dictionary_2;

    [Attribute5]
    private void Awake()
    {
      InputManager.dictionary_0 = new Dictionary<string, InputManager.InputInfo>();
      InputManager.dictionary_1 = new Dictionary<string, KeyCode>();
      InputManager.dictionary_2 = new Dictionary<string, string>();
      this.method_0();
    }

    public static void CreateKeysUI()
    {
      Vector3 vector3 = new Vector3(-290f, 100f, 0.0f);
      int num = 0;
      bool flag = false;
      foreach (string key in InputManager.dictionary_0.Keys)
      {
        Class65.smethod_4(UIMainReferences.panelBRSControls, vector3 + (flag ? Vector3.right * 400f : Vector3.zero), Quaternion.identity, "BRSKeyLbl" + (object) num, key, 10, 130);
        Class65.smethod_2(UIMainReferences.panelBRSControls, vector3 + Vector3.right * 150f + Vector3.down * 4f + (flag ? Vector3.right * 400f : Vector3.zero), Quaternion.identity, "BRSKey" + (object) num, InputManager.dictionary_0[key].key.ToString(), typeof (Class22)).GetComponent<Class22>().string_0 = key;
        ++num;
        if (flag)
          vector3 += Vector3.down * 30f;
        flag = !flag;
      }
    }

    public static void SetListen(string key)
    {
      InputManager.string_0 = key;
    }

    private void method_0()
    {
      if (Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/BRS/Config"))
      {
        if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/BRS/Config/keys.dat"))
        {
          BinaryReader binaryReader = new BinaryReader((Stream) new FileStream(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/BRS/Config/keys.dat", FileMode.Open, FileAccess.Read));
          if (binaryReader.ReadString() != "BRSKeyConfig")
          {
            binaryReader.Close();
          }
          else
          {
            double num = (double) binaryReader.ReadSingle();
            InputManager.dictionary_1 = Class70.smethod_4<Dictionary<string, KeyCode>>(binaryReader.ReadString());
            InputManager.dictionary_2 = Class70.smethod_4<Dictionary<string, string>>(binaryReader.ReadString());
            InputManager.dictionary_0 = Class70.smethod_4<Dictionary<string, InputManager.InputInfo>>(binaryReader.ReadString());
            binaryReader.Close();
          }
        }
        else
          InputManager.SetDefaults();
      }
      else
        InputManager.SetDefaults();
    }

    public static void SetDefaults()
    {
      if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/BRS/Config"))
        InputManager.smethod_0();
      if (!File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/BRS/Config/keys.dat"))
        InputManager.smethod_0();
      foreach (KeyValuePair<string, KeyCode> keyValuePair in InputManager.dictionary_1)
      {
        InputManager.dictionary_0[keyValuePair.Key].key = keyValuePair.Value;
        InputManager.dictionary_0[keyValuePair.Key].isAxis = false;
      }
      foreach (KeyValuePair<string, string> keyValuePair in InputManager.dictionary_2)
      {
        InputManager.dictionary_0[keyValuePair.Key].axisName = keyValuePair.Value;
        InputManager.dictionary_0[keyValuePair.Key].isAxis = true;
      }
    }

    private static void smethod_0()
    {
      if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/BRS"))
        Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/BRS");
      if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/BRS/Config"))
        Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/BRS/Config");
      if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/BRS/Config/keys.dat"))
        return;
      File.Create(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/BRS/Config/keys.dat");
    }

    private void method_1()
    {
      int num = 0;
      foreach (string key in InputManager.dictionary_0.Keys)
      {
        if (key != InputManager.string_0)
          GameObject.Find("BRSKey" + (object) num).transform.Find("Label").gameObject.GetComponent<UILabel>().text = !InputManager.dictionary_0[key].isAxis ? InputManager.dictionary_0[key].key.ToString() : InputManager.dictionary_0[key].axisName;
        ++num;
      }
    }

    private void method_2()
    {
      if (Event.current.type == UnityEngine.EventType.KeyDown)
      {
        InputManager.dictionary_0[InputManager.string_0].key = Event.current.keyCode;
        InputManager.dictionary_0[InputManager.string_0].isAxis = false;
        InputManager.string_0 = "";
        this.SaveKeys();
      }
      if (Event.current.type == UnityEngine.EventType.MouseDown)
      {
        InputManager.dictionary_0[InputManager.string_0].key = (KeyCode) (323 + Event.current.button);
        InputManager.dictionary_0[InputManager.string_0].isAxis = false;
        InputManager.string_0 = "";
        this.SaveKeys();
      }
      if (Input.GetKeyDown(KeyCode.LeftShift))
      {
        InputManager.dictionary_0[InputManager.string_0].key = KeyCode.LeftShift;
        InputManager.dictionary_0[InputManager.string_0].isAxis = false;
        InputManager.string_0 = "";
        this.SaveKeys();
      }
      else if (Input.GetKeyDown(KeyCode.RightShift))
      {
        InputManager.dictionary_0[InputManager.string_0].key = KeyCode.RightShift;
        InputManager.dictionary_0[InputManager.string_0].isAxis = false;
        InputManager.string_0 = "";
        this.SaveKeys();
      }
      if ((double) Input.GetAxis("MouseScrollUp") > 0.0)
      {
        InputManager.dictionary_0[InputManager.string_0].axisName = "MouseScrollUp";
        InputManager.dictionary_0[InputManager.string_0].isAxis = true;
        InputManager.string_0 = "";
        this.SaveKeys();
      }
      if ((double) Input.GetAxis("MouseScrollDown") <= 0.0)
        return;
      InputManager.dictionary_0[InputManager.string_0].axisName = "MouseScrollDown";
      InputManager.dictionary_0[InputManager.string_0].isAxis = true;
      InputManager.string_0 = "";
      this.SaveKeys();
    }

    [Attribute5]
    private void OnGUI()
    {
      if (!InputManager.menuOn)
        return;
      this.method_1();
      if (!(InputManager.string_0 != ""))
        return;
      this.method_2();
    }

    [Attribute5]
    private void Update()
    {
      foreach (string key in InputManager.dictionary_0.Keys)
      {
        InputManager.dictionary_0[key].isInput = Input.GetKey(InputManager.dictionary_0[key].key);
        InputManager.dictionary_0[key].isInputDown = Input.GetKeyDown(InputManager.dictionary_0[key].key);
        InputManager.dictionary_0[key].isInputUp = Input.GetKeyUp(InputManager.dictionary_0[key].key);
      }
    }

    [Attribute5]
    private void OnDestroy()
    {
      this.SaveKeys();
    }

    public bool RegisterKey(string keyname, KeyCode keycode)
    {
      if (InputManager.dictionary_0.ContainsKey(keyname))
        return false;
      InputManager.InputInfo inputInfo = new InputManager.InputInfo()
      {
        key = keycode,
        isAxis = false
      };
      InputManager.dictionary_0.Add(keyname, inputInfo);
      InputManager.dictionary_1.Add(keyname, keycode);
      return true;
    }

    public bool RegisterAxis(string keyname, string axisname)
    {
      if (InputManager.dictionary_0.ContainsKey(keyname))
        return false;
      InputManager.InputInfo inputInfo = new InputManager.InputInfo()
      {
        axisName = axisname,
        isAxis = true
      };
      InputManager.dictionary_0.Add(keyname, inputInfo);
      InputManager.dictionary_2.Add(keyname, axisname);
      return true;
    }

    public string GetKey(string keyname)
    {
      if (!InputManager.dictionary_0.ContainsKey(keyname))
        throw new ArgumentException("Key doesn't exist", nameof (keyname));
      if (InputManager.dictionary_0[keyname].isAxis)
        return InputManager.dictionary_0[keyname].axisName;
      return InputManager.dictionary_0[keyname].key.ToString();
    }

    public InputManager.InputInfo GetKeyInfo(string keyname)
    {
      if (!InputManager.dictionary_0.ContainsKey(keyname))
        throw new ArgumentException("Key doesn't exist", nameof (keyname));
      return InputManager.dictionary_0[keyname];
    }

    public void SaveKeys()
    {
      if (Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/BRS/Config"))
      {
        if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/BRS/Config/keys.dat"))
        {
          BinaryWriter binaryWriter = new BinaryWriter((Stream) new FileStream(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/BRS/Config/keys.dat", FileMode.Create, FileAccess.Write));
          binaryWriter.Write("BRSKeyConfig");
          binaryWriter.Write(1f);
          binaryWriter.Write(Class70.smethod_3<Dictionary<string, KeyCode>>(InputManager.dictionary_1));
          binaryWriter.Write(Class70.smethod_3<Dictionary<string, string>>(InputManager.dictionary_2));
          binaryWriter.Write(Class70.smethod_3<Dictionary<string, InputManager.InputInfo>>(InputManager.dictionary_0));
          binaryWriter.Close();
        }
        else
          InputManager.smethod_0();
      }
      else
        InputManager.smethod_0();
    }

    public bool IsInputDown(string keyname)
    {
      if (InputManager.dictionary_0.ContainsKey(keyname) && !InputManager.dictionary_0[keyname].isAxis)
        return InputManager.dictionary_0[keyname].isInputDown;
      return false;
    }

    public bool IsInputUp(string keyname)
    {
      if (InputManager.dictionary_0.ContainsKey(keyname) && !InputManager.dictionary_0[keyname].isAxis)
        return InputManager.dictionary_0[keyname].isInputUp;
      return false;
    }

    public bool IsInput(string keyname)
    {
      if (InputManager.dictionary_0.ContainsKey(keyname) && !InputManager.dictionary_0[keyname].isAxis)
        return InputManager.dictionary_0[keyname].isInput;
      return false;
    }

    public bool IsAxis(string keyname)
    {
      if (InputManager.dictionary_0.ContainsKey(keyname))
        return InputManager.dictionary_0[keyname].isAxis;
      return false;
    }

    public float GetAxis(string keyname)
    {
      if (!InputManager.dictionary_0.ContainsKey(keyname) || !InputManager.dictionary_0[keyname].isAxis)
        return 0.0f;
      return Input.GetAxis(InputManager.dictionary_0[keyname].axisName);
    }

    [Serializable]
    public class InputInfo
    {
      public KeyCode key;
      public bool isInputUp;
      public bool isInputDown;
      public bool isInput;
      public bool isAxis;
      public string axisName;
    }
  }
}
