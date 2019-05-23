// Decompiled with JetBrains decompiler
// Type: ExitGames.Client.Photon.EventData
// Assembly: Photon3Unity3D, Version=4.0.0.4, Culture=neutral, PublicKeyToken=null
// MVID: 45094ECB-AE08-4341-BE26-E524C8B0C677
// Assembly location: D:\Development\Attack on Titan\Assemblies\Photon3Unity3D.dll

using System.Collections;
using System.Collections.Generic;

namespace ExitGames.Client.Photon
{
  public class EventData
  {
    public byte Code;
    public Dictionary<byte, object> Parameters;

    public object this[byte key]
    {
      get
      {
        object obj;
        this.Parameters.TryGetValue(key, out obj);
        return obj;
      }
      set
      {
        this.Parameters[key] = value;
      }
    }

    public override string ToString()
    {
      return string.Format("Event {0}.", (object) this.Code.ToString());
    }

    public string ToStringFull()
    {
      return string.Format("Event {0}: {1}", (object) this.Code, (object) SupportClass.DictionaryToString((IDictionary) this.Parameters));
    }
  }
}
