// Decompiled with JetBrains decompiler
// Type: ExitGames.Client.Photon.SocketTcp
// Assembly: Photon3Unity3D, Version=4.0.0.4, Culture=neutral, PublicKeyToken=null
// MVID: 45094ECB-AE08-4341-BE26-E524C8B0C677
// Assembly location: D:\Development\Attack on Titan\Assemblies\Photon3Unity3D.dll

using System;
using System.IO;
using System.Net.Sockets;
using System.Security;
using System.Threading;

namespace ExitGames.Client.Photon
{
  internal class SocketTcp : IPhotonSocket
  {
    private readonly object syncer = new object();
    private Socket sock;

    public SocketTcp(PeerBase npeer)
      : base(npeer)
    {
      if (this.ReportDebugOfLevel(DebugLevel.ALL))
        this.Listener.DebugReturn(DebugLevel.ALL, "SocketTcp: TCP, DotNet, Unity.");
      this.Protocol = ConnectionProtocol.Tcp;
      this.PollReceive = false;
    }

    public override bool Connect()
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

    public override bool Disconnect()
    {
      if (this.ReportDebugOfLevel(DebugLevel.INFO))
        this.EnqueueDebugReturn(DebugLevel.INFO, "SocketTcp.Disconnect()");
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
      if (!this.sock.Connected)
        return PhotonSocketError.Skipped;
      try
      {
        this.sock.Send(data);
      }
      catch (Exception ex)
      {
        if (this.ReportDebugOfLevel(DebugLevel.ERROR))
          this.EnqueueDebugReturn(DebugLevel.ERROR, "Cannot send. " + ex.Message);
        this.HandleException(StatusCode.Exception);
        return PhotonSocketError.Exception;
      }
      return PhotonSocketError.Success;
    }

    public override PhotonSocketError Receive(out byte[] data)
    {
      data = (byte[]) null;
      return PhotonSocketError.NoData;
    }

    public void DnsAndConnect()
    {
      try
      {
        this.sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        this.sock.NoDelay = true;
        this.sock.Connect(IPhotonSocket.GetIpAddress(this.ServerAddress), this.ServerPort);
        this.State = PhotonSocketState.Connected;
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
      MemoryStream memoryStream = new MemoryStream(this.MTU);
      while (this.State == PhotonSocketState.Connected)
      {
        memoryStream.Position = 0L;
        memoryStream.SetLength(0L);
        try
        {
          int offset = 0;
          byte[] numArray = new byte[9];
          while (offset < 9)
          {
            int num = this.sock.Receive(numArray, offset, 9 - offset, SocketFlags.None);
            offset += num;
            if (num == 0)
              throw new SocketException(10054);
          }
          if ((int) numArray[0] == 240)
          {
            this.HandleReceivedDatagram(numArray, numArray.Length, false);
          }
          else
          {
            int size = (int) numArray[1] << 24 | (int) numArray[2] << 16 | (int) numArray[3] << 8 | (int) numArray[4];
            if (this.peerBase.TrafficStatsEnabled)
            {
              if ((int) numArray[5] == 0)
                this.peerBase.TrafficStatsIncoming.CountReliableOpCommand(size);
              else
                this.peerBase.TrafficStatsIncoming.CountUnreliableOpCommand(size);
            }
            if (this.ReportDebugOfLevel(DebugLevel.ALL))
              this.EnqueueDebugReturn(DebugLevel.ALL, "message length: " + (object) size);
            memoryStream.Write(numArray, 7, offset - 7);
            int num1 = 0;
            int length = size - 9;
            byte[] buffer = new byte[length];
            while (num1 < length)
            {
              int num2 = this.sock.Receive(buffer, num1, length - num1, SocketFlags.None);
              num1 += num2;
              if (num2 == 0)
                throw new SocketException(10054);
            }
            memoryStream.Write(buffer, 0, num1);
            if (memoryStream.Length > 0L)
              this.HandleReceivedDatagram(memoryStream.ToArray(), (int) memoryStream.Length, false);
            if (this.ReportDebugOfLevel(DebugLevel.ALL))
              this.EnqueueDebugReturn(DebugLevel.ALL, "TCP < " + (object) memoryStream.Length + (memoryStream.Length == (long) (length + 2) ? (object) " OK" : (object) " BAD"));
          }
        }
        catch (SocketException ex)
        {
          if (this.State != PhotonSocketState.Disconnecting && this.State != PhotonSocketState.Disconnected)
          {
            if (this.ReportDebugOfLevel(DebugLevel.ERROR))
              this.EnqueueDebugReturn(DebugLevel.ERROR, "Receiving failed. SocketException: " + (object) ex.SocketErrorCode);
            if (ex.SocketErrorCode == SocketError.ConnectionReset || ex.SocketErrorCode == SocketError.ConnectionAborted)
              this.HandleException(StatusCode.DisconnectByServer);
            else
              this.HandleException(StatusCode.ExceptionOnReceive);
          }
        }
        catch (Exception ex)
        {
          if (this.State != PhotonSocketState.Disconnecting && this.State != PhotonSocketState.Disconnected)
          {
            if (this.ReportDebugOfLevel(DebugLevel.ERROR))
              this.EnqueueDebugReturn(DebugLevel.ERROR, "Receiving failed. Exception: " + ex.ToString());
            this.HandleException(StatusCode.ExceptionOnReceive);
          }
        }
      }
      this.Disconnect();
    }
  }
}
