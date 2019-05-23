// Decompiled with JetBrains decompiler
// Type: ExitGames.Client.Photon.PeerStateValue
// Assembly: Photon3Unity3D, Version=4.0.0.4, Culture=neutral, PublicKeyToken=null
// MVID: 45094ECB-AE08-4341-BE26-E524C8B0C677
// Assembly location: D:\Development\Attack on Titan\Assemblies\Photon3Unity3D.dll

namespace ExitGames.Client.Photon
{
  public enum PeerStateValue : byte
  {
    Disconnected = 0,
    Connecting = 1,
    Connected = 3,
    Disconnecting = 4,
    InitializingApplication = 10,
  }
}
