// Decompiled with JetBrains decompiler
// Type: ExitGames.Client.Photon.SocketUdpNativeStatic
// Assembly: Photon3Unity3D, Version=4.0.0.4, Culture=neutral, PublicKeyToken=null
// MVID: 45094ECB-AE08-4341-BE26-E524C8B0C677
// Assembly location: D:\Development\Attack on Titan\Assemblies\Photon3Unity3D.dll

using System;

namespace ExitGames.Client.Photon
{
  public class SocketUdpNativeStatic : IPhotonSocket
  {
    public SocketUdpNativeStatic(PeerBase peerBase)
      : base(peerBase)
    {
    }

    public override bool Disconnect()
    {
      throw new NotImplementedException("This class was compiled in an assembly WITH c# sockets. Another dll must be used for native sockets.");
    }

    public override PhotonSocketError Send(byte[] data, int length)
    {
      throw new NotImplementedException("This class was compiled in an assembly WITH c# sockets. Another dll must be used for native sockets.");
    }

    public override PhotonSocketError Receive(out byte[] data)
    {
      throw new NotImplementedException("This class was compiled in an assembly WITH c# sockets. Another dll must be used for native sockets.");
    }
  }
}
