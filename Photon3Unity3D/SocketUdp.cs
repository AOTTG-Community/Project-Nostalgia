// Decompiled with JetBrains decompiler
// Type: ExitGames.Client.Photon.SocketUdp
// Assembly: Photon3Unity3D, Version=4.0.0.4, Culture=neutral, PublicKeyToken=null
// MVID: 45094ECB-AE08-4341-BE26-E524C8B0C677
// Assembly location: D:\Development\Attack on Titan\Assemblies\Photon3Unity3D.dll

using System;
using System.Net.Sockets;
using System.Security;
using System.Threading;

namespace ExitGames.Client.Photon
{
  internal class SocketUdp : IPhotonSocket
  {
    private readonly object syncer = new object();
    private Socket sock;

    public SocketUdp(PeerBase npeer)
      : base(npeer)
    {
      if (this.ReportDebugOfLevel(DebugLevel.ALL))
        this.Listener.DebugReturn(DebugLevel.ALL, "CSharpSocket: UDP, Unity3d.");
      this.Protocol = ConnectionProtocol.Udp;
      this.PollReceive = false;
    }

    public override bool Connect()
    {
      lock (this.syncer)
      {
        if (!base.Connect())
          return false;
        this.State = PhotonSocketState.Connecting;
        new Thread(new ThreadStart(this.DnsAndConnect))
        {
          Name = "photon dns thread",
          IsBackground = true
        }.Start();
        return true;
      }
    }

    public override bool Disconnect()
    {
      if (this.ReportDebugOfLevel(DebugLevel.INFO))
        this.EnqueueDebugReturn(DebugLevel.INFO, "CSharpSocket.Disconnect()");
      this.State = PhotonSocketState.Disconnecting;
      lock (this.syncer)
      {
        if (this.sock != null)
        {
          try
          {
            this.sock.Close();
          }
          catch (Exception ex)
          {
            this.EnqueueDebugReturn(DebugLevel.INFO, "Exception in Disconnect(): " + (object) ex);
          }
          this.sock = (Socket) null;
        }
      }
      this.State = PhotonSocketState.Disconnected;
      return true;
    }

    public override PhotonSocketError Send(byte[] data, int length)
    {
      lock (this.syncer)
      {
        if (!this.sock.Connected)
          return PhotonSocketError.Skipped;
        try
        {
          this.sock.Send(data, 0, length, SocketFlags.None);
        }
        catch
        {
          return PhotonSocketError.Exception;
        }
      }
      return PhotonSocketError.Success;
    }

    public override PhotonSocketError Receive(out byte[] data)
    {
      data = (byte[]) null;
      return PhotonSocketError.NoData;
    }

    internal void DnsAndConnect()
    {
      try
      {
        lock (this.syncer)
        {
          this.sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
          this.sock.Connect(IPhotonSocket.GetIpAddress(this.ServerAddress), this.ServerPort);
          this.State = PhotonSocketState.Connected;
        }
      }
      catch (SecurityException ex)
      {
        if (this.ReportDebugOfLevel(DebugLevel.ERROR))
          this.Listener.DebugReturn(DebugLevel.ERROR, "Connect() failed: " + ex.ToString());
        this.HandleException(StatusCode.SecurityExceptionOnConnect);
        return;
      }
      catch (Exception ex)
      {
        if (this.ReportDebugOfLevel(DebugLevel.ERROR))
          this.Listener.DebugReturn(DebugLevel.ERROR, "Connect() failed: " + ex.ToString());
        this.HandleException(StatusCode.ExceptionOnConnect);
        return;
      }
      new Thread(new ThreadStart(this.ReceiveLoop))
      {
        Name = "photon receive thread",
        IsBackground = true
      }.Start();
    }

    public void ReceiveLoop()
    {
      byte[] numArray = new byte[this.MTU];
      while (this.State == PhotonSocketState.Connected)
      {
        try
        {
          int length = this.sock.Receive(numArray);
          this.HandleReceivedDatagram(numArray, length, true);
        }
        catch (Exception ex)
        {
          if (this.State != PhotonSocketState.Disconnecting && this.State != PhotonSocketState.Disconnected)
          {
            if (this.ReportDebugOfLevel(DebugLevel.ERROR))
              this.EnqueueDebugReturn(DebugLevel.ERROR, "Receive issue. State: " + (object) this.State + " Exception: " + (object) ex);
            this.HandleException(StatusCode.ExceptionOnReceive);
          }
        }
      }
      this.Disconnect();
    }
  }
}
