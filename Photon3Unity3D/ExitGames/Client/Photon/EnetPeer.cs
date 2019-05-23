// Decompiled with JetBrains decompiler
// Type: ExitGames.Client.Photon.EnetPeer
// Assembly: Photon3Unity3D, Version=4.0.0.4, Culture=neutral, PublicKeyToken=null
// MVID: 45094ECB-AE08-4341-BE26-E524C8B0C677
// Assembly location: D:\Development\Attack on Titan\Assemblies\Photon3Unity3D.dll

using System;
using System.Collections.Generic;
using System.Text;

namespace ExitGames.Client.Photon
{
  internal class EnetPeer : PeerBase
  {
    internal static readonly byte[] udpHeader0xF3 = new byte[2]
    {
      (byte) 243,
      (byte) 2
    };
    internal static readonly byte[] messageHeader = EnetPeer.udpHeader0xF3;
    private Dictionary<byte, EnetChannel> channels = new Dictionary<byte, EnetChannel>();
    private List<NCommand> sentReliableCommands = new List<NCommand>();
    private Queue<NCommand> outgoingAcknowledgementsList = new Queue<NCommand>();
    internal readonly int windowSize = 128;
    private const int CRC_LENGTH = 4;
    private byte udpCommandCount;
    private byte[] udpBuffer;
    private int udpBufferIndex;
    internal int challenge;
    internal int reliableCommandsRepeated;
    internal int reliableCommandsSent;
    internal int serverSentTime;

    internal override int QueuedIncomingCommandsCount
    {
      get
      {
        int num = 0;
        lock (this.channels)
        {
          foreach (EnetChannel enetChannel in this.channels.Values)
          {
            num += enetChannel.incomingReliableCommandsList.Count;
            num += enetChannel.incomingUnreliableCommandsList.Count;
          }
        }
        return num;
      }
    }

    internal override int QueuedOutgoingCommandsCount
    {
      get
      {
        int num = 0;
        lock (this.channels)
        {
          foreach (EnetChannel enetChannel in this.channels.Values)
          {
            num += enetChannel.outgoingReliableCommandsList.Count;
            num += enetChannel.outgoingUnreliableCommandsList.Count;
          }
        }
        return num;
      }
    }

    internal EnetPeer()
    {
      ++PeerBase.peerCount;
      this.InitOnce();
      this.TrafficPackageHeaderSize = 12;
    }

    internal EnetPeer(IPhotonPeerListener listener)
      : this()
    {
      this.Listener = listener;
    }

    internal override void InitPeerBase()
    {
      base.InitPeerBase();
      this.peerID = (short) -1;
      this.challenge = SupportClass.ThreadSafeRandom.Next();
      this.udpBuffer = new byte[this.mtu];
      this.reliableCommandsSent = 0;
      this.reliableCommandsRepeated = 0;
      lock (this.channels)
        this.channels = new Dictionary<byte, EnetChannel>();
      lock (this.channels)
      {
        this.channels[byte.MaxValue] = new EnetChannel(byte.MaxValue, this.commandBufferSize);
        for (byte channelNumber = 0; (int) channelNumber < (int) this.ChannelCount; ++channelNumber)
          this.channels[channelNumber] = new EnetChannel(channelNumber, this.commandBufferSize);
      }
      lock (this.sentReliableCommands)
        this.sentReliableCommands = new List<NCommand>(this.commandBufferSize);
      lock (this.outgoingAcknowledgementsList)
        this.outgoingAcknowledgementsList = new Queue<NCommand>(this.commandBufferSize);
    }

    internal override bool Connect(string ipport, string appID)
    {
      if (this.peerConnectionState != PeerBase.ConnectionStateValue.Disconnected)
      {
        this.Listener.DebugReturn(DebugLevel.WARNING, "Connect() can't be called if peer is not Disconnected. Not connecting. peerConnectionState: " + (object) this.peerConnectionState);
        return false;
      }
      if (this.debugOut >= DebugLevel.ALL)
        this.Listener.DebugReturn(DebugLevel.ALL, "Connect()");
      this.ServerAddress = ipport;
      this.InitPeerBase();
      if (appID == null)
        appID = "Lite";
      for (int index = 0; index < 32; ++index)
        this.INIT_BYTES[index + 9] = index < appID.Length ? (byte) appID[index] : (byte) 0;
      this.rt = (IPhotonSocket) new SocketUdp((PeerBase) this);
      if (this.rt == null)
      {
        this.Listener.DebugReturn(DebugLevel.ERROR, "Connect() failed, because SocketImplementation or socket was null. Set PhotonPeer.SocketImplementation before Connect().");
        return false;
      }
      if (!this.rt.Connect())
        return false;
      if (this.TrafficStatsEnabled)
      {
        this.TrafficStatsOutgoing.ControlCommandBytes += 44;
        ++this.TrafficStatsOutgoing.ControlCommandCount;
      }
      this.peerConnectionState = PeerBase.ConnectionStateValue.Connecting;
      this.QueueOutgoingReliableCommand(new NCommand(this, (byte) 2, (byte[]) null, byte.MaxValue));
      return true;
    }

    internal override void Disconnect()
    {
      if (this.peerConnectionState == PeerBase.ConnectionStateValue.Disconnected || this.peerConnectionState == PeerBase.ConnectionStateValue.Disconnecting)
        return;
      if (this.outgoingAcknowledgementsList != null)
      {
        lock (this.outgoingAcknowledgementsList)
          this.outgoingAcknowledgementsList.Clear();
      }
      if (this.sentReliableCommands != null)
      {
        lock (this.sentReliableCommands)
          this.sentReliableCommands.Clear();
      }
      lock (this.channels)
      {
        foreach (EnetChannel enetChannel in this.channels.Values)
          enetChannel.clearAll();
      }
      NCommand command = new NCommand(this, (byte) 4, (byte[]) null, byte.MaxValue);
      this.QueueOutgoingReliableCommand(command);
      this.SendOutgoingCommands();
      if (this.TrafficStatsEnabled)
        this.TrafficStatsOutgoing.CountControlCommand(command.Size);
      this.rt.Disconnect();
      this.peerConnectionState = PeerBase.ConnectionStateValue.Disconnected;
      this.Listener.OnStatusChanged(StatusCode.Disconnect);
    }

    internal override void StopConnection()
    {
      if (this.rt != null)
        this.rt.Disconnect();
      this.peerConnectionState = PeerBase.ConnectionStateValue.Disconnected;
      if (this.Listener == null)
        return;
      this.Listener.OnStatusChanged(StatusCode.Disconnect);
    }

    internal override void FetchServerTimestamp()
    {
      if (this.peerConnectionState != PeerBase.ConnectionStateValue.Connected)
      {
        if (this.debugOut < DebugLevel.INFO)
          return;
        this.EnqueueDebugReturn(DebugLevel.INFO, "FetchServerTimestamp() was skipped, as the client is not connected. Current ConnectionState: " + (object) this.peerConnectionState);
      }
      else
        this.CreateAndEnqueueCommand((byte) 12, new byte[0], byte.MaxValue);
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
      NCommand ncommand1 = (NCommand) null;
      Queue<int> intQueue = new Queue<int>();
      lock (this.channels)
      {
        foreach (EnetChannel enetChannel in this.channels.Values)
        {
          if (enetChannel.incomingUnreliableCommandsList.Count > 0)
          {
            int index = int.MaxValue;
            foreach (int key in enetChannel.incomingUnreliableCommandsList.Keys)
            {
              NCommand unreliableCommands = enetChannel.incomingUnreliableCommandsList[key];
              if (key < enetChannel.incomingUnreliableSequenceNumber || unreliableCommands.reliableSequenceNumber < enetChannel.incomingReliableSequenceNumber)
                intQueue.Enqueue(key);
              else if (this.limitOfUnreliableCommands > 0 && enetChannel.incomingUnreliableCommandsList.Count > this.limitOfUnreliableCommands)
                intQueue.Enqueue(key);
              else if (key < index && unreliableCommands.reliableSequenceNumber <= enetChannel.incomingReliableSequenceNumber)
                index = key;
            }
            while (intQueue.Count > 0)
              enetChannel.incomingUnreliableCommandsList.Remove(intQueue.Dequeue());
            if (index < int.MaxValue)
              ncommand1 = enetChannel.incomingUnreliableCommandsList[index];
            if (ncommand1 != null)
            {
              enetChannel.incomingUnreliableCommandsList.Remove(ncommand1.unreliableSequenceNumber);
              enetChannel.incomingUnreliableSequenceNumber = ncommand1.unreliableSequenceNumber;
              break;
            }
          }
          if (ncommand1 == null && enetChannel.incomingReliableCommandsList.Count > 0)
          {
            enetChannel.incomingReliableCommandsList.TryGetValue(enetChannel.incomingReliableSequenceNumber + 1, out ncommand1);
            if (ncommand1 != null)
            {
              if ((int) ncommand1.commandType != 8)
              {
                enetChannel.incomingReliableSequenceNumber = ncommand1.reliableSequenceNumber;
                enetChannel.incomingReliableCommandsList.Remove(ncommand1.reliableSequenceNumber);
                break;
              }
              if (ncommand1.fragmentsRemaining > 0)
              {
                ncommand1 = (NCommand) null;
                break;
              }
              byte[] numArray = new byte[ncommand1.totalLength];
              for (int startSequenceNumber = ncommand1.startSequenceNumber; startSequenceNumber < ncommand1.startSequenceNumber + ncommand1.fragmentCount; ++startSequenceNumber)
              {
                if (!enetChannel.ContainsReliableSequenceNumber(startSequenceNumber))
                  throw new Exception("command.fragmentsRemaining was 0, but not all fragments are found to be combined!");
                NCommand ncommand2 = enetChannel.FetchReliableSequenceNumber(startSequenceNumber);
                Buffer.BlockCopy((Array) ncommand2.Payload, 0, (Array) numArray, ncommand2.fragmentOffset, ncommand2.Payload.Length);
                enetChannel.incomingReliableCommandsList.Remove(ncommand2.reliableSequenceNumber);
              }
              if (this.debugOut >= DebugLevel.ALL)
                this.Listener.DebugReturn(DebugLevel.ALL, "assembled fragmented payload from " + (object) ncommand1.fragmentCount + " parts. Dispatching now.");
              ncommand1.Payload = numArray;
              ncommand1.Size = 12 * ncommand1.fragmentCount + ncommand1.totalLength;
              enetChannel.incomingReliableSequenceNumber = ncommand1.reliableSequenceNumber + ncommand1.fragmentCount - 1;
              break;
            }
          }
        }
      }
      if (ncommand1 != null && ncommand1.Payload != null)
      {
        this.ByteCountCurrentDispatch = ncommand1.Size;
        this.CommandInCurrentDispatch = ncommand1;
        if (this.DeserializeMessageAndCallback(ncommand1.Payload))
        {
          this.CommandInCurrentDispatch = (NCommand) null;
          return true;
        }
        this.CommandInCurrentDispatch = (NCommand) null;
      }
      return false;
    }

    internal override bool SendAcksOnly()
    {
      if (this.peerConnectionState == PeerBase.ConnectionStateValue.Disconnected || (this.rt == null || !this.rt.Connected))
        return false;
      lock (this.udpBuffer)
      {
        int num = 0;
        this.udpBufferIndex = 12;
        if (this.crcEnabled)
          this.udpBufferIndex += 4;
        this.udpCommandCount = (byte) 0;
        this.timeInt = SupportClass.GetTickCount() - this.timeBase;
        lock (this.outgoingAcknowledgementsList)
        {
          if (this.outgoingAcknowledgementsList.Count > 0)
            num = this.SerializeToBuffer(this.outgoingAcknowledgementsList);
        }
        if (this.timeInt > this.timeoutInt && this.sentReliableCommands.Count > 0)
        {
          lock (this.sentReliableCommands)
          {
            foreach (NCommand sentReliableCommand in this.sentReliableCommands)
            {
              if (sentReliableCommand != null && sentReliableCommand.roundTripTimeout != 0 && this.timeInt - sentReliableCommand.commandSentTime > sentReliableCommand.roundTripTimeout)
              {
                sentReliableCommand.commandSentCount = (byte) 1;
                sentReliableCommand.roundTripTimeout = 0;
                sentReliableCommand.timeoutTime = int.MaxValue;
                sentReliableCommand.commandSentTime = this.timeInt;
              }
            }
          }
        }
        if ((int) this.udpCommandCount <= 0)
          return false;
        if (this.TrafficStatsEnabled)
        {
          ++this.TrafficStatsOutgoing.TotalPacketCount;
          this.TrafficStatsOutgoing.TotalCommandsInPackets += (int) this.udpCommandCount;
        }
        this.SendData(this.udpBuffer, this.udpBufferIndex);
        return num > 0;
      }
    }

    internal override bool SendOutgoingCommands()
    {
      if (this.peerConnectionState == PeerBase.ConnectionStateValue.Disconnected || !this.rt.Connected)
        return false;
      lock (this.udpBuffer)
      {
        int num = 0;
        this.udpBufferIndex = 12;
        if (this.crcEnabled)
          this.udpBufferIndex += 4;
        this.udpCommandCount = (byte) 0;
        this.timeInt = SupportClass.GetTickCount() - this.timeBase;
        lock (this.outgoingAcknowledgementsList)
        {
          if (this.outgoingAcknowledgementsList.Count > 0)
            num = this.SerializeToBuffer(this.outgoingAcknowledgementsList);
        }
        if (!this.IsSendingOnlyAcks && this.timeInt > this.timeoutInt && this.sentReliableCommands.Count > 0)
        {
          lock (this.sentReliableCommands)
          {
            Queue<NCommand> ncommandQueue = new Queue<NCommand>();
            foreach (NCommand sentReliableCommand in this.sentReliableCommands)
            {
              if (sentReliableCommand != null && this.timeInt - sentReliableCommand.commandSentTime > sentReliableCommand.roundTripTimeout)
              {
                if ((int) sentReliableCommand.commandSentCount > this.sentCountAllowance || this.timeInt > sentReliableCommand.timeoutTime)
                {
                  if (this.debugOut >= DebugLevel.INFO)
                    this.Listener.DebugReturn(DebugLevel.INFO, "Timeout-disconnect! Command: " + (object) sentReliableCommand + " now: " + (object) this.timeInt + " challenge: " + Convert.ToString(this.challenge, 16));
                  this.peerConnectionState = PeerBase.ConnectionStateValue.Zombie;
                  this.Listener.OnStatusChanged(StatusCode.TimeoutDisconnect);
                  this.Disconnect();
                  return false;
                }
                ncommandQueue.Enqueue(sentReliableCommand);
              }
            }
            while (ncommandQueue.Count > 0)
            {
              NCommand command = ncommandQueue.Dequeue();
              this.QueueOutgoingReliableCommand(command);
              this.sentReliableCommands.Remove(command);
              ++this.reliableCommandsRepeated;
              if (this.debugOut >= DebugLevel.INFO)
                this.Listener.DebugReturn(DebugLevel.INFO, string.Format("Resending: {0}. times out after: {1} sent: {3} now: {2} rtt/var: {4}/{5} last recv: {6}", (object) command, (object) command.roundTripTimeout, (object) this.timeInt, (object) command.commandSentTime, (object) this.roundTripTime, (object) this.roundTripTimeVariance, (object) (SupportClass.GetTickCount() - this.timestampOfLastReceive)));
            }
          }
        }
        if (!this.IsSendingOnlyAcks && this.peerConnectionState == PeerBase.ConnectionStateValue.Connected && (this.timePingInterval > 0 && this.sentReliableCommands.Count == 0) && (this.timeInt - this.timeLastAckReceive > this.timePingInterval && !this.AreReliableCommandsInTransit()) && this.udpBufferIndex + 12 < this.udpBuffer.Length)
        {
          NCommand command = new NCommand(this, (byte) 5, (byte[]) null, byte.MaxValue);
          this.QueueOutgoingReliableCommand(command);
          if (this.TrafficStatsEnabled)
            this.TrafficStatsOutgoing.CountControlCommand(command.Size);
        }
        if (!this.IsSendingOnlyAcks)
        {
          lock (this.channels)
          {
            foreach (EnetChannel enetChannel in this.channels.Values)
            {
              num += this.SerializeToBuffer(enetChannel.outgoingReliableCommandsList);
              num += this.SerializeToBuffer(enetChannel.outgoingUnreliableCommandsList);
            }
          }
        }
        if ((int) this.udpCommandCount <= 0)
          return false;
        if (this.TrafficStatsEnabled)
        {
          ++this.TrafficStatsOutgoing.TotalPacketCount;
          this.TrafficStatsOutgoing.TotalCommandsInPackets += (int) this.udpCommandCount;
        }
        this.SendData(this.udpBuffer, this.udpBufferIndex);
        return num > 0;
      }
    }

    private bool AreReliableCommandsInTransit()
    {
      lock (this.channels)
      {
        foreach (EnetChannel enetChannel in this.channels.Values)
        {
          if (enetChannel.outgoingReliableCommandsList.Count > 0)
            return true;
        }
      }
      return false;
    }

    internal override bool EnqueueOperation(Dictionary<byte, object> parameters, byte opCode, bool sendReliable, byte channelId, bool encrypt, PeerBase.EgMessageType messageType)
    {
      if (this.peerConnectionState != PeerBase.ConnectionStateValue.Connected)
      {
        if (this.debugOut >= DebugLevel.ERROR)
          this.Listener.DebugReturn(DebugLevel.ERROR, "Cannot send op: " + (object) opCode + " Not connected. PeerState: " + (object) this.peerConnectionState);
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
      return this.CreateAndEnqueueCommand(sendReliable ? (byte) 6 : (byte) 7, message, channelId);
    }

    internal bool CreateAndEnqueueCommand(byte commandType, byte[] payload, byte channelNumber)
    {
      if (payload == null)
        return false;
      EnetChannel channel = this.channels[channelNumber];
      this.ByteCountLastOperation = 0;
      int count = this.mtu - 12 - 36;
      if (payload.Length > count)
      {
        int num1 = (payload.Length + count - 1) / count;
        int num2 = channel.outgoingReliableSequenceNumber + 1;
        int num3 = 0;
        int srcOffset = 0;
        while (srcOffset < payload.Length)
        {
          if (payload.Length - srcOffset < count)
            count = payload.Length - srcOffset;
          byte[] payload1 = new byte[count];
          Buffer.BlockCopy((Array) payload, srcOffset, (Array) payload1, 0, count);
          NCommand command = new NCommand(this, (byte) 8, payload1, channel.ChannelNumber);
          command.fragmentNumber = num3;
          command.startSequenceNumber = num2;
          command.fragmentCount = num1;
          command.totalLength = payload.Length;
          command.fragmentOffset = srcOffset;
          this.QueueOutgoingReliableCommand(command);
          this.ByteCountLastOperation += command.Size;
          if (this.TrafficStatsEnabled)
          {
            this.TrafficStatsOutgoing.CountFragmentOpCommand(command.Size);
            this.TrafficStatsGameLevel.CountOperation(command.Size);
          }
          ++num3;
          srcOffset += count;
        }
      }
      else
      {
        NCommand command = new NCommand(this, commandType, payload, channel.ChannelNumber);
        if ((int) command.commandFlags == 1)
        {
          this.QueueOutgoingReliableCommand(command);
          this.ByteCountLastOperation = command.Size;
          if (this.TrafficStatsEnabled)
          {
            this.TrafficStatsOutgoing.CountReliableOpCommand(command.Size);
            this.TrafficStatsGameLevel.CountOperation(command.Size);
          }
        }
        else
        {
          this.QueueOutgoingUnreliableCommand(command);
          this.ByteCountLastOperation = command.Size;
          if (this.TrafficStatsEnabled)
          {
            this.TrafficStatsOutgoing.CountUnreliableOpCommand(command.Size);
            this.TrafficStatsGameLevel.CountOperation(command.Size);
          }
        }
      }
      return true;
    }

    internal override byte[] SerializeOperationToMessage(byte opc, Dictionary<byte, object> parameters, PeerBase.EgMessageType messageType, bool encrypt)
    {
      byte[] array;
      lock (this.SerializeMemStream)
      {
        this.SerializeMemStream.Position = 0L;
        this.SerializeMemStream.SetLength(0L);
        if (!encrypt)
          this.SerializeMemStream.Write(EnetPeer.messageHeader, 0, EnetPeer.messageHeader.Length);
        Protocol.SerializeOperationRequest(this.SerializeMemStream, opc, parameters, false);
        if (encrypt)
        {
          byte[] buffer = this.CryptoProvider.Encrypt(this.SerializeMemStream.ToArray());
          this.SerializeMemStream.Position = 0L;
          this.SerializeMemStream.SetLength(0L);
          this.SerializeMemStream.Write(EnetPeer.messageHeader, 0, EnetPeer.messageHeader.Length);
          this.SerializeMemStream.Write(buffer, 0, buffer.Length);
        }
        array = this.SerializeMemStream.ToArray();
      }
      if (messageType != PeerBase.EgMessageType.Operation)
        array[EnetPeer.messageHeader.Length - 1] = (byte) messageType;
      if (encrypt)
        array[EnetPeer.messageHeader.Length - 1] = (byte) ((uint) array[EnetPeer.messageHeader.Length - 1] | 128U);
      return array;
    }

    internal int SerializeToBuffer(Queue<NCommand> commandList)
    {
      while (commandList.Count > 0)
      {
        NCommand command = commandList.Peek();
        if (command == null)
        {
          commandList.Dequeue();
        }
        else
        {
          if (this.udpBufferIndex + command.Size > this.udpBuffer.Length)
          {
            if (this.debugOut >= DebugLevel.INFO)
            {
              this.Listener.DebugReturn(DebugLevel.INFO, "UDP package is full. Commands in Package: " + (object) this.udpCommandCount + ". Commands left in queue: " + (object) commandList.Count);
              break;
            }
            break;
          }
          Buffer.BlockCopy((Array) command.Serialize(), 0, (Array) this.udpBuffer, this.udpBufferIndex, command.Size);
          this.udpBufferIndex += command.Size;
          ++this.udpCommandCount;
          if (((int) command.commandFlags & 1) > 0)
            this.QueueSentCommand(command);
          commandList.Dequeue();
        }
      }
      return commandList.Count;
    }

    internal void SendData(byte[] data, int length)
    {
      try
      {
        int targetOffset1 = 0;
        Protocol.Serialize(this.peerID, data, ref targetOffset1);
        data[2] = this.crcEnabled ? (byte) 204 : (byte) 0;
        data[3] = this.udpCommandCount;
        int targetOffset2 = 4;
        Protocol.Serialize(this.timeInt, data, ref targetOffset2);
        Protocol.Serialize(this.challenge, data, ref targetOffset2);
        if (this.crcEnabled)
        {
          Protocol.Serialize(0, data, ref targetOffset2);
          uint crc = SupportClass.CalculateCrc(data, length);
          targetOffset2 -= 4;
          Protocol.Serialize((int) crc, data, ref targetOffset2);
        }
        this.bytesOut += (long) length;
        if (this.NetworkSimulationSettings.IsSimulationEnabled)
        {
          byte[] dataCopy = new byte[length];
          Buffer.BlockCopy((Array) data, 0, (Array) dataCopy, 0, length);
          int num;
          this.SendNetworkSimulated((PeerBase.MyAction) (() => num = (int) this.rt.Send(dataCopy, length)));
        }
        else
        {
          int num1 = (int) this.rt.Send(data, length);
        }
      }
      catch (Exception ex)
      {
        if (this.debugOut >= DebugLevel.ERROR)
          this.Listener.DebugReturn(DebugLevel.ERROR, ex.ToString());
        SupportClass.WriteStackTrace(ex);
      }
    }

    internal void QueueSentCommand(NCommand command)
    {
      command.commandSentTime = this.timeInt;
      ++command.commandSentCount;
      if (command.roundTripTimeout == 0)
      {
        command.roundTripTimeout = this.roundTripTime + 4 * this.roundTripTimeVariance;
        command.timeoutTime = this.timeInt + this.DisconnectTimeout;
      }
      else
        command.roundTripTimeout *= 2;
      lock (this.sentReliableCommands)
      {
        if (this.sentReliableCommands.Count == 0)
          this.timeoutInt = command.commandSentTime + command.roundTripTimeout;
        ++this.reliableCommandsSent;
        this.sentReliableCommands.Add(command);
      }
      if (this.sentReliableCommands.Count < this.warningSize || this.sentReliableCommands.Count % this.warningSize != 0)
        return;
      this.Listener.OnStatusChanged(StatusCode.QueueSentWarning);
    }

    internal void QueueOutgoingReliableCommand(NCommand command)
    {
      EnetChannel channel = this.channels[command.commandChannelID];
      lock (channel)
      {
        Queue<NCommand> reliableCommandsList = channel.outgoingReliableCommandsList;
        if (reliableCommandsList.Count >= this.warningSize && reliableCommandsList.Count % this.warningSize == 0)
          this.Listener.OnStatusChanged(StatusCode.QueueOutgoingReliableWarning);
        if (command.reliableSequenceNumber == 0)
          command.reliableSequenceNumber = ++channel.outgoingReliableSequenceNumber;
        reliableCommandsList.Enqueue(command);
      }
    }

    internal void QueueOutgoingUnreliableCommand(NCommand command)
    {
      Queue<NCommand> unreliableCommandsList = this.channels[command.commandChannelID].outgoingUnreliableCommandsList;
      if (unreliableCommandsList.Count >= this.warningSize && unreliableCommandsList.Count % this.warningSize == 0)
        this.Listener.OnStatusChanged(StatusCode.QueueOutgoingUnreliableWarning);
      EnetChannel channel = this.channels[command.commandChannelID];
      command.reliableSequenceNumber = channel.outgoingReliableSequenceNumber;
      command.unreliableSequenceNumber = ++channel.outgoingUnreliableSequenceNumber;
      unreliableCommandsList.Enqueue(command);
    }

    internal void QueueOutgoingAcknowledgement(NCommand command)
    {
      lock (this.outgoingAcknowledgementsList)
      {
        if (this.outgoingAcknowledgementsList.Count >= this.warningSize && this.outgoingAcknowledgementsList.Count % this.warningSize == 0)
          this.Listener.OnStatusChanged(StatusCode.QueueOutgoingAcksWarning);
        this.outgoingAcknowledgementsList.Enqueue(command);
      }
    }

    internal override void ReceiveIncomingCommands(byte[] inBuff, int dataLength)
    {
      this.timestampOfLastReceive = SupportClass.GetTickCount();
      try
      {
        int offset = 0;
        short num1;
        Protocol.Deserialize(out num1, inBuff, ref offset);
        byte[] numArray1 = inBuff;
        int index1 = offset;
        int num2 = 1;
        int num3 = index1 + num2;
        byte num4 = numArray1[index1];
        byte[] numArray2 = inBuff;
        int index2 = num3;
        int num5 = 1;
        int num6 = index2 + num5;
        byte num7 = numArray2[index2];
        Protocol.Deserialize(out this.serverSentTime, inBuff, ref num6);
        int num8;
        Protocol.Deserialize(out num8, inBuff, ref num6);
        if ((int) num4 == 204)
        {
          int num9;
          Protocol.Deserialize(out num9, inBuff, ref num6);
          this.bytesIn += 4L;
          num6 -= 4;
          Protocol.Serialize(0, inBuff, ref num6);
          uint crc = SupportClass.CalculateCrc(inBuff, dataLength);
          if (num9 != (int) crc)
          {
            ++this.packetLossByCrc;
            if (this.peerConnectionState == PeerBase.ConnectionStateValue.Disconnected || this.debugOut < DebugLevel.INFO)
              return;
            this.EnqueueDebugReturn(DebugLevel.INFO, string.Format("Ignored package due to wrong CRC. Incoming:  {0:X} Local: {1:X}", (object) (uint) num9, (object) crc));
            return;
          }
        }
        this.bytesIn += 12L;
        if (this.TrafficStatsEnabled)
        {
          ++this.TrafficStatsIncoming.TotalPacketCount;
          this.TrafficStatsIncoming.TotalCommandsInPackets += (int) num7;
        }
        if ((int) num7 > this.commandBufferSize || (int) num7 <= 0)
          this.EnqueueDebugReturn(DebugLevel.ERROR, "too many/few incoming commands in package: " + (object) num7 + " > " + (object) this.commandBufferSize);
        if (num8 != this.challenge)
        {
          if (this.peerConnectionState == PeerBase.ConnectionStateValue.Disconnected || this.debugOut < DebugLevel.ALL)
            return;
          this.EnqueueDebugReturn(DebugLevel.ALL, "Info: Ignoring received package due to wrong challenge. Challenge in-package!=local:" + (object) num8 + "!=" + (object) this.challenge + " Commands in it: " + (object) num7);
        }
        else
        {
          this.timeInt = SupportClass.GetTickCount() - this.timeBase;
          for (int index3 = 0; index3 < (int) num7; ++index3)
          {
            NCommand readCommand = new NCommand(this, inBuff, ref num6);
            if ((int) readCommand.commandType != 1)
            {
              this.EnqueueActionForDispatch((PeerBase.MyAction) (() => this.ExecuteCommand(readCommand)));
            }
            else
            {
              this.TrafficStatsIncoming.TimestampOfLastAck = SupportClass.GetTickCount();
              this.ExecuteCommand(readCommand);
            }
            if (((int) readCommand.commandFlags & 1) > 0)
            {
              NCommand ack = NCommand.CreateAck(this, readCommand, this.serverSentTime);
              this.QueueOutgoingAcknowledgement(ack);
              if (this.TrafficStatsEnabled)
              {
                this.TrafficStatsIncoming.TimestampOfLastReliableCommand = SupportClass.GetTickCount();
                this.TrafficStatsOutgoing.CountControlCommand(ack.Size);
              }
            }
          }
        }
      }
      catch (Exception ex)
      {
        if (this.debugOut >= DebugLevel.ERROR)
          this.EnqueueDebugReturn(DebugLevel.ERROR, string.Format("Exception while reading commands from incoming data: {0}", (object) ex));
        SupportClass.WriteStackTrace(ex);
      }
    }

    internal bool ExecuteCommand(NCommand command)
    {
      bool flag = true;
      switch (command.commandType)
      {
        case 1:
          if (this.TrafficStatsEnabled)
            this.TrafficStatsIncoming.CountControlCommand(command.Size);
          this.timeLastAckReceive = this.timeInt;
          this.lastRoundTripTime = this.timeInt - command.ackReceivedSentTime;
          NCommand ncommand = this.RemoveSentReliableCommand(command.ackReceivedReliableSequenceNumber, (int) command.commandChannelID);
          if (ncommand != null)
          {
            if ((int) ncommand.commandType == 12)
            {
              if (this.lastRoundTripTime <= this.roundTripTime)
              {
                this.serverTimeOffset = this.serverSentTime + (this.lastRoundTripTime >> 1) - SupportClass.GetTickCount();
                this.serverTimeOffsetIsAvailable = true;
              }
              else
                this.FetchServerTimestamp();
            }
            else
            {
              this.UpdateRoundTripTimeAndVariance(this.lastRoundTripTime);
              if ((int) ncommand.commandType == 4 && this.peerConnectionState == PeerBase.ConnectionStateValue.Disconnecting)
              {
                if (this.debugOut >= DebugLevel.INFO)
                  this.EnqueueDebugReturn(DebugLevel.INFO, "Received disconnect ACK by server");
                this.EnqueueActionForDispatch((PeerBase.MyAction) (() => this.rt.Disconnect()));
              }
              else if ((int) ncommand.commandType == 2)
                this.roundTripTime = this.lastRoundTripTime;
            }
            break;
          }
          break;
        case 2:
        case 5:
          if (this.TrafficStatsEnabled)
          {
            this.TrafficStatsIncoming.CountControlCommand(command.Size);
            break;
          }
          break;
        case 3:
          if (this.TrafficStatsEnabled)
            this.TrafficStatsIncoming.CountControlCommand(command.Size);
          if (this.peerConnectionState == PeerBase.ConnectionStateValue.Connecting)
          {
            command = new NCommand(this, (byte) 6, this.INIT_BYTES, (byte) 0);
            this.QueueOutgoingReliableCommand(command);
            if (this.TrafficStatsEnabled)
              this.TrafficStatsOutgoing.CountControlCommand(command.Size);
            this.peerConnectionState = PeerBase.ConnectionStateValue.Connected;
            break;
          }
          break;
        case 4:
          if (this.TrafficStatsEnabled)
            this.TrafficStatsIncoming.CountControlCommand(command.Size);
          StatusCode statusCode = StatusCode.DisconnectByServer;
          if ((int) command.reservedByte == 1)
            statusCode = StatusCode.DisconnectByServerLogic;
          else if ((int) command.reservedByte == 3)
            statusCode = StatusCode.DisconnectByServerUserLimit;
          if (this.debugOut >= DebugLevel.INFO)
            this.Listener.DebugReturn(DebugLevel.INFO, "Server " + this.ServerAddress + " sent disconnect. PeerId: " + (object) (ushort) this.peerID + " RTT/Variance:" + (object) this.roundTripTime + "/" + (object) this.roundTripTimeVariance + " reason byte: " + (object) command.reservedByte);
          this.peerConnectionState = PeerBase.ConnectionStateValue.Disconnecting;
          this.Listener.OnStatusChanged(statusCode);
          this.rt.Disconnect();
          this.peerConnectionState = PeerBase.ConnectionStateValue.Disconnected;
          this.Listener.OnStatusChanged(StatusCode.Disconnect);
          break;
        case 6:
          if (this.TrafficStatsEnabled)
            this.TrafficStatsIncoming.CountReliableOpCommand(command.Size);
          if (this.peerConnectionState == PeerBase.ConnectionStateValue.Connected)
          {
            flag = this.QueueIncomingCommand(command);
            break;
          }
          break;
        case 7:
          if (this.TrafficStatsEnabled)
            this.TrafficStatsIncoming.CountUnreliableOpCommand(command.Size);
          if (this.peerConnectionState == PeerBase.ConnectionStateValue.Connected)
          {
            flag = this.QueueIncomingCommand(command);
            break;
          }
          break;
        case 8:
          if (this.TrafficStatsEnabled)
            this.TrafficStatsIncoming.CountFragmentOpCommand(command.Size);
          if (this.peerConnectionState == PeerBase.ConnectionStateValue.Connected)
          {
            if (command.fragmentNumber > command.fragmentCount || command.fragmentOffset >= command.totalLength || command.fragmentOffset + command.Payload.Length > command.totalLength)
            {
              if (this.debugOut >= DebugLevel.ERROR)
              {
                this.Listener.DebugReturn(DebugLevel.ERROR, "Received fragment has bad size: " + (object) command);
                break;
              }
              break;
            }
            flag = this.QueueIncomingCommand(command);
            if (flag)
            {
              EnetChannel channel = this.channels[command.commandChannelID];
              if (command.reliableSequenceNumber == command.startSequenceNumber)
              {
                --command.fragmentsRemaining;
                int num = command.startSequenceNumber + 1;
                while (command.fragmentsRemaining > 0 && num < command.startSequenceNumber + command.fragmentCount)
                {
                  if (channel.ContainsReliableSequenceNumber(num++))
                    --command.fragmentsRemaining;
                }
              }
              else if (channel.ContainsReliableSequenceNumber(command.startSequenceNumber))
                --channel.FetchReliableSequenceNumber(command.startSequenceNumber).fragmentsRemaining;
            }
            break;
          }
          break;
      }
      return flag;
    }

    internal bool QueueIncomingCommand(NCommand command)
    {
      EnetChannel enetChannel = (EnetChannel) null;
      this.channels.TryGetValue(command.commandChannelID, out enetChannel);
      if (enetChannel == null)
      {
        if (this.debugOut >= DebugLevel.ERROR)
          this.Listener.DebugReturn(DebugLevel.ERROR, "Received command for non-existing channel: " + (object) command.commandChannelID);
        return false;
      }
      if (this.debugOut >= DebugLevel.ALL)
        this.Listener.DebugReturn(DebugLevel.ALL, "queueIncomingCommand() " + (object) command + " channel seq# r/u: " + (object) enetChannel.incomingReliableSequenceNumber + "/" + (object) enetChannel.incomingUnreliableSequenceNumber);
      if ((int) command.commandFlags == 1)
      {
        if (command.reliableSequenceNumber <= enetChannel.incomingReliableSequenceNumber)
        {
          if (this.debugOut >= DebugLevel.INFO)
            this.Listener.DebugReturn(DebugLevel.INFO, "incoming command " + command.ToString() + " is old (not saving it). Dispatched incomingReliableSequenceNumber: " + (object) enetChannel.incomingReliableSequenceNumber);
          return false;
        }
        if (enetChannel.ContainsReliableSequenceNumber(command.reliableSequenceNumber))
        {
          if (this.debugOut >= DebugLevel.INFO)
            this.Listener.DebugReturn(DebugLevel.INFO, "Info: command was received before! Old/New: " + (object) enetChannel.FetchReliableSequenceNumber(command.reliableSequenceNumber) + "/" + (object) command + " inReliableSeq#: " + (object) enetChannel.incomingReliableSequenceNumber);
          return false;
        }
        if (enetChannel.incomingReliableCommandsList.Count >= this.warningSize && enetChannel.incomingReliableCommandsList.Count % this.warningSize == 0)
          this.Listener.OnStatusChanged(StatusCode.QueueIncomingReliableWarning);
        enetChannel.incomingReliableCommandsList.Add(command.reliableSequenceNumber, command);
        return true;
      }
      if ((int) command.commandFlags != 0)
        return false;
      if (command.reliableSequenceNumber < enetChannel.incomingReliableSequenceNumber)
      {
        if (this.debugOut >= DebugLevel.INFO)
          this.Listener.DebugReturn(DebugLevel.INFO, "incoming reliable-seq# < Dispatched-rel-seq#. not saved.");
        return true;
      }
      if (command.unreliableSequenceNumber <= enetChannel.incomingUnreliableSequenceNumber)
      {
        if (this.debugOut >= DebugLevel.INFO)
          this.Listener.DebugReturn(DebugLevel.INFO, "incoming unreliable-seq# < Dispatched-unrel-seq#. not saved.");
        return true;
      }
      if (enetChannel.ContainsUnreliableSequenceNumber(command.unreliableSequenceNumber))
      {
        if (this.debugOut >= DebugLevel.INFO)
          this.Listener.DebugReturn(DebugLevel.INFO, "command was received before! Old/New: " + (object) enetChannel.incomingUnreliableCommandsList[command.unreliableSequenceNumber] + "/" + (object) command);
        return false;
      }
      if (enetChannel.incomingUnreliableCommandsList.Count >= this.warningSize && enetChannel.incomingUnreliableCommandsList.Count % this.warningSize == 0)
        this.Listener.OnStatusChanged(StatusCode.QueueIncomingUnreliableWarning);
      enetChannel.incomingUnreliableCommandsList.Add(command.unreliableSequenceNumber, command);
      return true;
    }

    internal NCommand RemoveSentReliableCommand(int ackReceivedReliableSequenceNumber, int ackReceivedChannel)
    {
      NCommand ncommand = (NCommand) null;
      lock (this.sentReliableCommands)
      {
        foreach (NCommand sentReliableCommand in this.sentReliableCommands)
        {
          if (sentReliableCommand != null && sentReliableCommand.reliableSequenceNumber == ackReceivedReliableSequenceNumber && (int) sentReliableCommand.commandChannelID == ackReceivedChannel)
          {
            ncommand = sentReliableCommand;
            break;
          }
        }
        if (ncommand != null)
        {
          this.sentReliableCommands.Remove(ncommand);
          if (this.sentReliableCommands.Count > 0)
            this.timeoutInt = this.sentReliableCommands[0].commandSentTime + this.sentReliableCommands[0].roundTripTimeout;
        }
        else if (this.debugOut >= DebugLevel.ALL && this.peerConnectionState != PeerBase.ConnectionStateValue.Connected && this.peerConnectionState != PeerBase.ConnectionStateValue.Disconnecting)
          this.EnqueueDebugReturn(DebugLevel.ALL, string.Format("No sent command for ACK (Ch: {0} Sq#: {1}). PeerState: {2}.", (object) ackReceivedReliableSequenceNumber, (object) ackReceivedChannel, (object) this.peerConnectionState));
      }
      return ncommand;
    }

    internal string CommandListToString(NCommand[] list)
    {
      if (this.debugOut < DebugLevel.ALL)
        return string.Empty;
      StringBuilder stringBuilder = new StringBuilder();
      for (int index = 0; index < list.Length; ++index)
      {
        stringBuilder.Append(index.ToString() + "=");
        stringBuilder.Append((object) list[index]);
        stringBuilder.Append(" # ");
      }
      return stringBuilder.ToString();
    }
  }
}
