// Decompiled with JetBrains decompiler
// Type: ExitGames.Client.Photon.DictionaryEntryEnumerator
// Assembly: Photon3Unity3D, Version=4.0.0.4, Culture=neutral, PublicKeyToken=null
// MVID: 45094ECB-AE08-4341-BE26-E524C8B0C677
// Assembly location: D:\Development\Attack on Titan\Assemblies\Photon3Unity3D.dll

using System;
using System.Collections;
using System.Collections.Generic;

namespace ExitGames.Client.Photon
{
  public class DictionaryEntryEnumerator : IEnumerator<DictionaryEntry>, IDisposable, IEnumerator
  {
    private IDictionaryEnumerator enumerator;

    public DictionaryEntryEnumerator(IDictionaryEnumerator original)
    {
      this.enumerator = original;
    }

    public bool MoveNext()
    {
      return this.enumerator.MoveNext();
    }

    public void Reset()
    {
      this.enumerator.Reset();
    }

    object IEnumerator.Current
    {
      get
      {
        return (object) (DictionaryEntry) this.enumerator.Current;
      }
    }

    public DictionaryEntry Current
    {
      get
      {
        return (DictionaryEntry) this.enumerator.Current;
      }
    }

    public object Key
    {
      get
      {
        return this.enumerator.Key;
      }
    }

    public object Value
    {
      get
      {
        return this.enumerator.Value;
      }
    }

    public DictionaryEntry Entry
    {
      get
      {
        return this.enumerator.Entry;
      }
    }

    public void Dispose()
    {
      this.enumerator = (IDictionaryEnumerator) null;
    }
  }
}
