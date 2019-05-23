// Decompiled with JetBrains decompiler
// Type: ExitGames.Client.Photon.TPeer
// Assembly: Photon3Unity3D, Version=4.0.0.4, Culture=neutral, PublicKeyToken=null
// MVID: 45094ECB-AE08-4341-BE26-E524C8B0C677
// Assembly location: D:\Development\Attack on Titan\Assemblies\Photon3Unity3D.dll

using System;
using System.Collections.Generic;
using System.IO;

namespace ExitGames.Client.Photon
{
  internal class TPeer : PeerBase
  {
    internal static readonly byte[] tcpHead = new byte[9]
    {
      (byte) 251,
      (byte) 0,
      (byte) 0,
      (byte) 0,
      (byte) 0,
      (byte) 0,
      (byte) 0,
      (byte) 243,
      (byte) 2
    };
    internal static readonly byte[] messageHeader = TPeer.tcpHead;
    private List<byte[]> incomingList = new List<byte[]>();
    private byte[] pingRequest = new byte[5]
    {
      (byte) 240,
      (byte) 0,
      (byte) 0,
      (byte) 0,
      (byte) 0
    };
    internal const int TCP_HEADER_BYTES = 7;
    internal const int MSG_HEADER_BYTES = 2;
    internal const int ALL_HEADER_BYTES = 9;
    internal MemoryStream outgoingStream;
    private int lastPingResult;

    internal override int QueuedIncomingCommandsCount
    {
      get
      {
        return this.incomingList.Count;
      }
    }

    internal override int QueuedOutgoingCommandsCount
    {
      get
      {
        return this.outgoingCommandsInStream;
      }
    }

    internal TPeer()
    {
      ++PeerBase.peerCount;
      this.InitOnce();
      this.TrafficPackageHeaderSize = 0;
    }

    internal TPeer(IPhotonPeerListener listener)
      : this()
    {
      this.Listener = listener;
    }

    internal override void InitPeerBase()
    {
      base.InitPeerBase();
      this.incomingList = new List<byte[]>();
    }

    internal override bool Connect(string serverAddress, string appID)
    {
      if (this.peerConnectionState != PeerBase.ConnectionStateValue.Disconnected)
      {
        this.Listener.DebugReturn(DebugLevel.WARNING, "Connect() can't be called if peer is not Disconnected. Not connecting.");
        return false;
      }
      if (this.debugOut >= DebugLevel.ALL)
        this.Listener.DebugReturn(DebugLevel.ALL, "Connect()");
      this.ServerAddress = serverAddress;
      this.InitPeerBase();
      this.outgoingStream = new MemoryStream(PeerBase.outgoingStreamBufferSize);
      if (appID == null)
        appID = "Lite";
      for (int index = 0; index < 32; ++index)
        this.INIT_BYTES[index + 9] = index < appID.Length ? (byte) appID[index] : (byte) 0;
      this.rt = (IPhotonSocket) new SocketTcp((PeerBase) this);
      if (this.rt == null)
      {
        this.Listener.DebugReturn(DebugLevel.ERROR, "Connect() failed, because SocketImplementation or socket was null. Set PhotonPeer.SocketImplementation before Connect().");
        return false;
      }
      if (this.rt.Connect())
      {
        this.peerConnectionState = PeerBase.ConnectionStateValue.Connecting;
        this.EnqueueInit();
        this.SendOutgoingCommands();
        return true;
      }
      this.peerConnectionState = PeerBase.ConnectionStateValue.Disconnected;
      return false;
    }

    internal override void Disconnect()
    {
      if (this.peerConnectionState == PeerBase.ConnectionStateValue.Disconnected || this.peerConnectionState == PeerBase.ConnectionStateValue.Disconnecting)
        return;
      if (this.debugOut >= DebugLevel.ALL)
        this.Listener.DebugReturn(DebugLevel.ALL, "TPeer.Disconnect()");
      this.StopConnection();
    }

    internal override void StopConnection()
    {
      this.peerConnectionState = PeerBase.ConnectionStateValue.Disconnecting;
      if (this.rt != null)
        this.rt.Disconnect();
      lock (this.incomingList)
        this.incomingList.Clear();
      this.peerConnectionState = PeerBase.ConnectionStateValue.Disconnected;
      this.Listener.OnStatusChanged(StatusCode.Disconnect);
    }

    internal override void FetchServerTimestamp()
    {
      if (this.peerConnectionState != PeerBase.ConnectionStateValue.Connected)
      {
        if (this.debugOut >= DebugLevel.INFO)
          this.Listener.DebugReturn(DebugLevel.INFO, "FetchServerTimestamp() was skipped, as the client is not connected. Current ConnectionState: " + (object) this.peerConnectionState);
        this.Listener.OnStatusChanged(StatusCode.SendError);
      }
      else
      {
        this.SendPing();
        this.serverTimeOffsetIsAvailable = false;
      }
    }

    private void EnqueueInit()
    {
      MemoryStream memoryStream = new MemoryStream(0);
      BinaryWriter binaryWriter = new BinaryWriter((Stream) memoryStream);
      byte[] numArray1 = new byte[7];
      numArray1[0] = (byte) 251;
      numArray1[6] = (byte) 1;
      byte[] numArray2 = numArray1;
      int targetOffset = 1;
      Protocol.Serialize(this.INIT_BYTES.Length + numArray2.Length, numArray2, ref targetOffset);
      binaryWriter.Write(numArray2);
      binaryWriter.Write(this.INIT_BYTES);
      byte[] array = memoryStream.ToArray();
      if (this.TrafficStatsEnabled)
      {
        ++this.TrafficStatsOutgoing.TotalPacketCount;
        ++this.TrafficStatsOutgoing.TotalCommandsInPackets;
        this.TrafficStatsOutgoing.CountControlCommand(array.Length);
      }
      this.EnqueueMessageAsPayload(true, array, (byte) 0);
    }

    internal override bool DispatchIncomingCommands()
    {
      while (true)
      {
        PeerBase.MyAction myAction;
        lock (this.ActionQueue)
        {
          if (this.ActionQueue.Count > 0)
            myAction = this.ActionQueue.Dequeue();
          else
            break;
        }
        myAction();
      }
      byte[] incoming;
      lock (this.incomingList)
      {
        if (this.incomingList.Count <= 0)
          return false;
        incoming = this.incomingList[0];
        this.incomingList.RemoveAt(0);
      }
      this.ByteCountCurrentDispatch = incoming.Length + 3;
      return this.DeserializeMessageAndCallback(incoming);
    }

    internal override bool SendOutgoingCommands()
    {
      if (this.peerConnectionState == PeerBase.ConnectionStateValue.Disconnected || !this.rt.Connected)
        return false;
      if (this.peerConnectionState == PeerBase.ConnectionStateValue.Connected && SupportClass.GetTickCount() - this.lastPingResult > this.timePingInterval)
        this.SendPing();
      lock (this.outgoingStream)
      {
        if (this.outgoingStream.Position > 0L)
        {
          this.SendData(this.outgoingStream.ToArray());
          this.outgoingStream.Position = 0L;
          this.outgoingStream.SetLength(0L);
          this.outgoingCommandsInStream = 0;
        }
      }
      return false;
    }

    internal override bool SendAcksOnly()
    {
      if (this.rt == null || !this.rt.Connected || (this.peerConnectionState != PeerBase.ConnectionStateValue.Connected || SupportClass.GetTickCount() - this.lastPingResult <= this.timePingInterval))
        return false;
      this.SendPing();
      return false;
    }

    internal override bool EnqueueOperation(Dictionary<byte, object> parameters, byte opCode, bool sendReliable, byte channelId, bool encrypt, PeerBase.EgMessageType messageType)
    {
      if (this.peerConnectionState != PeerBase.ConnectionStateValue.Connected)
      {
        if (this.debugOut >= DebugLevel.ERROR)
          this.Listener.DebugReturn(DebugLevel.ERROR, "Cannot send op: " + (object) opCode + "! Not connected. PeerState: " + (object) this.peerConnectionState);
        this.Listener.OnStatusChanged(StatusCode.SendError);
        return false;
      }
      if ((int) channelId >= (int) this.ChannelCount)
      {
        if (this.debugOut >= DebugLevel.ERROR)
          this.Listener.DebugReturn(DebugLevel.ERROR, "Cannot send op: Selected channel (" + (object) channelId + ")>= channelCount (" + (object) this.ChannelCount + ").");
        this.Listener.OnStatusChanged(StatusCode.SendError);
        return false;
      }
      byte[] message = this.SerializeOperationToMessage(opCode, parameters, messageType, encrypt);
      return this.EnqueueMessageAsPayload(sendReliable, message, channelId);
    }

    internal override byte[] SerializeOperationToMessage(byte opc, Dictionary<byte, object> parameters, PeerBase.EgMessageType messageType, bool encrypt)
    {
      byte[] array;
      lock (this.SerializeMemStream)
      {
        this.SerializeMemStream.Position = 0L;
        this.SerializeMemStream.SetLength(0L);
        if (!encrypt)
          this.SerializeMemStream.Write(TPeer.messageHeader, 0, TPeer.messageHeader.Length);
        Protocol.SerializeOperationRequest(this.SerializeMemStream, opc, parameters, false);
        if (encrypt)
        {
          byte[] buffer = this.CryptoProvider.Encrypt(this.SerializeMemStream.ToArray());
          this.SerializeMemStream.Position = 0L;
          this.SerializeMemStream.SetLength(0L);
          this.SerializeMemStream.Write(TPeer.messageHeader, 0, TPeer.messageHeader.Length);
          this.SerializeMemStream.Write(buffer, 0, buffer.Length);
        }
        array = this.SerializeMemStream.ToArray();
      }
      if (messageType != PeerBase.EgMessageType.Operation)
        array[TPeer.messageHeader.Length - 1] = (byte) messageType;
      if (encrypt)
        array[TPeer.messageHeader.Length - 1] = (byte) ((uint) array[TPeer.messageHeader.Length - 1] | 128U);
      int targetOffset = 1;
      Protocol.Serialize(array.Length, array, ref targetOffset);
      return array;
    }

    internal bool EnqueueMessageAsPayload(bool sendReliable, byte[] opMessage, byte channelId)
    {
      if (opMessage == null)
        return false;
      opMessage[5] = channelId;
      opMessage[6] = sendReliable ? (byte) 1 : (byte) 0;
      lock (this.outgoingStream)
      {
        this.outgoingStream.Write(opMessage, 0, opMessage.Length);
        ++this.outgoingCommandsInStream;
        if (this.outgoingCommandsInStream % this.warningSize == 0)
          this.Listener.OnStatusChanged(StatusCode.QueueOutgoingReliableWarning);
      }
      this.ByteCountLastOperation = opMessage.Length;
      if (this.TrafficStatsEnabled)
      {
        if (sendReliable)
          this.TrafficStatsOutgoing.CountReliableOpCommand(opMessage.Length);
        else
          this.TrafficStatsOutgoing.CountUnreliableOpCommand(opMessage.Length);
        this.TrafficStatsGameLevel.CountOperation(opMessage.Length);
      }
      return true;
    }

    internal void SendPing()
    {
      int targetOffset = 1;
      Protocol.Serialize(SupportClass.GetTickCount(), this.pingRequest, ref targetOffset);
      this.lastPingResult = SupportClass.GetTickCount();
      if (this.TrafficStatsEnabled)
        this.TrafficStatsOutgoing.CountControlCommand(this.pingRequest.Length);
      this.SendData(this.pingRequest);
    }

    internal void SendData(byte[] data)
    {
      try
      {
        this.bytesOut += (long) data.Length;
        if (this.TrafficStatsEnabled)
        {
          ++this.TrafficStatsOutgoing.TotalPacketCount;
          this.TrafficStatsOutgoing.TotalCommandsInPackets += this.outgoingCommandsInStream;
        }
        if (this.NetworkSimulationSettings.IsSimulationEnabled)
        {
          int num;
          this.SendNetworkSimulated((PeerBase.MyAction) (() => num = (int) this.rt.Send(data, data.Length)));
        }
        else
        {
          int num1 = (int) this.rt.Send(data, data.Length);
        }
      }
      catch (Exception ex)
      {
        if (this.debugOut >= DebugLevel.ERROR)
          this.Listener.DebugReturn(DebugLevel.ERROR, ex.ToString());
        SupportClass.WriteStackTrace(ex);
      }
    }

    internal override void ReceiveIncomingCommands(byte[] inbuff, int dataLength)
    {
      if (inbuff == null)
      {
        if (this.debugOut < DebugLevel.ERROR)
          return;
        this.EnqueueDebugReturn(DebugLevel.ERROR, "checkAndQueueIncomingCommands() inBuff: null");
      }
      else
      {
        this.timestampOfLastReceive = SupportClass.GetTickCount();
        this.bytesIn += (long) (inbuff.Length + 7);
        if (this.TrafficStatsEnabled)
        {
          ++this.TrafficStatsIncoming.TotalPacketCount;
          ++this.TrafficStatsIncoming.TotalCommandsInPackets;
        }
        if ((int) inbuff[0] == 243 || (int) inbuff[0] == 244)
        {
          lock (this.incomingList)
          {
            this.incomingList.Add(inbuff);
            if (this.incomingList.Count % this.warningSize != 0)
              return;
            this.EnqueueStatusCallback(StatusCode.QueueIncomingReliableWarning);
          }
        }
        else if ((int) inbuff[0] == 240)
        {
          this.TrafficStatsIncoming.CountControlCommand(inbuff.Length);
          this.ReadPingResult(inbuff);
        }
        else
        {
          if (this.debugOut < DebugLevel.ERROR)
            return;
          this.EnqueueDebugReturn(DebugLevel.ERROR, "receiveIncomingCommands() MagicNumber should be 0xF0, 0xF3 or 0xF4. Is: " + (object) inbuff[0]);
        }
      }
    }

    private void ReadPingResult(byte[] inbuff)
    {
      int num1 = 0;
      int num2 = 0;
      int offset = 1;
      Protocol.Deserialize(out num1, inbuff, ref offset);
      Protocol.Deserialize(out num2, inbuff, ref offset);
      this.lastRoundTripTime = SupportClass.GetTickCount() - num2;
      if (!this.serverTimeOffsetIsAvailable)
        this.roundTripTime = this.lastRoundTripTime;
      this.UpdateRoundTripTimeAndVariance(this.lastRoundTripTime);
      if (this.serverTimeOffsetIsAvailable)
        return;
      this.serverTimeOffset = num1 + (this.lastRoundTripTime >> 1) - SupportClass.GetTickCount();
      this.serverTimeOffsetIsAvailable = true;
    }
  }
}
