// Decompiled with JetBrains decompiler
// Type: ExitGames.Client.Photon.PhotonPing
// Assembly: Photon3Unity3D, Version=4.0.0.4, Culture=neutral, PublicKeyToken=null
// MVID: 45094ECB-AE08-4341-BE26-E524C8B0C677
// Assembly location: D:\Development\Attack on Titan\Assemblies\Photon3Unity3D.dll

using System;

namespace ExitGames.Client.Photon
{
  public abstract class PhotonPing : IDisposable
  {
    public string DebugString = "";
    protected internal int PingLength = 13;
    protected internal byte[] PingBytes = new byte[13]
    {
      (byte) 125,
      (byte) 125,
      (byte) 125,
      (byte) 125,
      (byte) 125,
      (byte) 125,
      (byte) 125,
      (byte) 125,
      (byte) 125,
      (byte) 125,
      (byte) 125,
      (byte) 125,
      (byte) 0
    };
    public bool Successful;
    protected internal bool GotResult;
    protected internal byte PingId;

    public virtual bool StartPing(string ip)
    {
      throw new NotImplementedException();
    }

    public virtual bool Done()
    {
      throw new NotImplementedException();
    }

    public virtual void Dispose()
    {
      throw new NotImplementedException();
    }

    protected internal void Init()
    {
      this.GotResult = false;
      this.Successful = false;
      this.PingId = (byte) (Environment.TickCount % (int) byte.MaxValue);
    }
  }
}
