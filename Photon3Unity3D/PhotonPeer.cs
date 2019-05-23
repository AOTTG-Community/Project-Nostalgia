// Decompiled with JetBrains decompiler
// Type: ExitGames.Client.Photon.PhotonPeer
// Assembly: Photon3Unity3D, Version=4.0.0.4, Culture=neutral, PublicKeyToken=null
// MVID: 45094ECB-AE08-4341-BE26-E524C8B0C677
// Assembly location: D:\Development\Attack on Titan\Assemblies\Photon3Unity3D.dll

using System;
using System.Collections.Generic;

namespace ExitGames.Client.Photon
{
  public class PhotonPeer
  {
    private readonly object SendOutgoingLockObject = new object();
    private readonly object DispatchLockObject = new object();
    private readonly object EnqueueLock = new object();
    public const bool NoSocket = false;
    internal PeerBase peerBase;

    public Type SocketImplementation
    {
      set
      {
        this.peerBase.SocketImplementation = value;
      }
      get
      {
        return this.peerBase.SocketImplementation;
      }
    }

    public DebugLevel DebugOut
    {
      set
      {
        this.peerBase.debugOut = value;
      }
      get
      {
        return this.peerBase.debugOut;
      }
    }

    public IPhotonPeerListener Listener
    {
      get
      {
        return this.peerBase.Listener;
      }
      protected set
      {
        this.peerBase.Listener = value;
      }
    }

    public long BytesIn
    {
      get
      {
        return this.peerBase.BytesIn;
      }
    }

    public long BytesOut
    {
      get
      {
        return this.peerBase.BytesOut;
      }
    }

    public int ByteCountCurrentDispatch
    {
      get
      {
        return this.peerBase.ByteCountCurrentDispatch;
      }
    }

    public string CommandInfoCurrentDispatch
    {
      get
      {
        return this.peerBase.CommandInCurrentDispatch != null ? this.peerBase.CommandInCurrentDispatch.ToString() : string.Empty;
      }
    }

    public int ByteCountLastOperation
    {
      get
      {
        return this.peerBase.ByteCountLastOperation;
      }
    }

    public bool TrafficStatsEnabled
    {
      get
      {
        return this.peerBase.TrafficStatsEnabled;
      }
      set
      {
        this.peerBase.TrafficStatsEnabled = value;
      }
    }

    public long TrafficStatsElapsedMs
    {
      get
      {
        return this.peerBase.TrafficStatsEnabledTime;
      }
    }

    public void TrafficStatsReset()
    {
      this.peerBase.InitializeTrafficStats();
      this.peerBase.TrafficStatsEnabled = true;
    }

    public TrafficStats TrafficStatsIncoming
    {
      get
      {
        return this.peerBase.TrafficStatsIncoming;
      }
    }

    public TrafficStats TrafficStatsOutgoing
    {
      get
      {
        return this.peerBase.TrafficStatsOutgoing;
      }
    }

    public TrafficStatsGameLevel TrafficStatsGameLevel
    {
      get
      {
        return this.peerBase.TrafficStatsGameLevel;
      }
    }

    public PeerStateValue PeerState
    {
      get
      {
        if (this.peerBase.peerConnectionState == PeerBase.ConnectionStateValue.Connected && !this.peerBase.ApplicationIsInitialized)
          return PeerStateValue.InitializingApplication;
        return (PeerStateValue) this.peerBase.peerConnectionState;
      }
    }

    public string PeerID
    {
      get
      {
        return this.peerBase.PeerID;
      }
    }

    public int CommandBufferSize
    {
      get
      {
        return this.peerBase.commandBufferSize;
      }
    }

    public int RhttpMinConnections
    {
      get
      {
        return this.peerBase.rhttpMinConnections;
      }
      set
      {
        this.peerBase.rhttpMinConnections = value;
      }
    }

    public int RhttpMaxConnections
    {
      get
      {
        return this.peerBase.rhttpMaxConnections;
      }
      set
      {
        this.peerBase.rhttpMaxConnections = value;
      }
    }

    public int LimitOfUnreliableCommands
    {
      get
      {
        return this.peerBase.limitOfUnreliableCommands;
      }
      set
      {
        this.peerBase.limitOfUnreliableCommands = value;
      }
    }

    public int QueuedIncomingCommands
    {
      get
      {
        return this.peerBase.QueuedIncomingCommandsCount;
      }
    }

    public int QueuedOutgoingCommands
    {
      get
      {
        return this.peerBase.QueuedOutgoingCommandsCount;
      }
    }

    public byte ChannelCount
    {
      get
      {
        return this.peerBase.ChannelCount;
      }
      set
      {
        if ((int) value == 0 || this.PeerState != PeerStateValue.Disconnected)
          throw new Exception("ChannelCount can only be set while disconnected and must be > 0.");
        this.peerBase.ChannelCount = value;
      }
    }

    public bool CrcEnabled
    {
      get
      {
        return this.peerBase.crcEnabled;
      }
      set
      {
        if (this.peerBase.peerConnectionState != PeerBase.ConnectionStateValue.Disconnected)
          throw new Exception("CrcEnabled can only be set while disconnected.");
        this.peerBase.crcEnabled = value;
      }
    }

    public int PacketLossByCrc
    {
      get
      {
        return this.peerBase.packetLossByCrc;
      }
    }

    public int ResentReliableCommands
    {
      get
      {
        return this.UsedProtocol != ConnectionProtocol.Udp ? 0 : ((EnetPeer) this.peerBase).reliableCommandsRepeated;
      }
    }

    public int WarningSize
    {
      get
      {
        return this.peerBase.warningSize;
      }
      set
      {
        this.peerBase.warningSize = value;
      }
    }

    public int SentCountAllowance
    {
      get
      {
        return this.peerBase.sentCountAllowance;
      }
      set
      {
        this.peerBase.sentCountAllowance = value;
      }
    }

    public int TimePingInterval
    {
      get
      {
        return this.peerBase.timePingInterval;
      }
      set
      {
        this.peerBase.timePingInterval = value;
      }
    }

    public int DisconnectTimeout
    {
      get
      {
        return this.peerBase.DisconnectTimeout;
      }
      set
      {
        this.peerBase.DisconnectTimeout = value;
      }
    }

    public int ServerTimeInMilliSeconds
    {
      get
      {
        return this.peerBase.serverTimeOffsetIsAvailable ? this.peerBase.serverTimeOffset + SupportClass.GetTickCount() : 0;
      }
    }

    [Obsolete("Should be replaced by: SupportClass.GetTickCount(). Internally this is used, too.")]
    public int LocalTimeInMilliSeconds
    {
      get
      {
        return SupportClass.GetTickCount();
      }
    }

    public SupportClass.IntegerMillisecondsDelegate LocalMsTimestampDelegate
    {
      set
      {
        if (this.PeerState != PeerStateValue.Disconnected)
          throw new Exception("LocalMsTimestampDelegate only settable while disconnected. State: " + (object) this.PeerState);
        SupportClass.IntegerMilliseconds = value;
      }
    }

    public int RoundTripTime
    {
      get
      {
        return this.peerBase.roundTripTime;
      }
    }

    public int RoundTripTimeVariance
    {
      get
      {
        return this.peerBase.roundTripTimeVariance;
      }
    }

    public int TimestampOfLastSocketReceive
    {
      get
      {
        return this.peerBase.timestampOfLastReceive;
      }
    }

    public string ServerAddress
    {
      get
      {
        return this.peerBase.ServerAddress;
      }
      set
      {
        if (this.UsedProtocol == ConnectionProtocol.RHttp)
        {
          this.peerBase.ServerAddress = value;
        }
        else
        {
          if (this.DebugOut < DebugLevel.ERROR)
            return;
          this.Listener.DebugReturn(DebugLevel.ERROR, "Failed to set ServerAddress. Can be set only when using HTTP.");
        }
      }
    }

    public string HttpUrlParameters
    {
      get
      {
        if (this.UsedProtocol == ConnectionProtocol.RHttp)
          return this.peerBase.HttpUrlParameters;
        return string.Empty;
      }
      set
      {
        if (this.UsedProtocol == ConnectionProtocol.RHttp)
        {
          this.peerBase.HttpUrlParameters = value;
        }
        else
        {
          if (this.DebugOut < DebugLevel.ERROR)
            return;
          this.Listener.DebugReturn(DebugLevel.ERROR, "Failed to set HttpUrlParameters. Can be set only when using HTTP.");
        }
      }
    }

    public ConnectionProtocol UsedProtocol
    {
      get
      {
        return this.peerBase.usedProtocol;
      }
    }

    public virtual bool IsSimulationEnabled
    {
      get
      {
        return this.NetworkSimulationSettings.IsSimulationEnabled;
      }
      set
      {
        if (value == this.NetworkSimulationSettings.IsSimulationEnabled)
          return;
        lock (this.SendOutgoingLockObject)
          this.NetworkSimulationSettings.IsSimulationEnabled = value;
      }
    }

    public NetworkSimulationSet NetworkSimulationSettings
    {
      get
      {
        return this.peerBase.NetworkSimulationSettings;
      }
    }

    public static int OutgoingStreamBufferSize
    {
      get
      {
        return PeerBase.outgoingStreamBufferSize;
      }
      set
      {
        PeerBase.outgoingStreamBufferSize = value;
      }
    }

    public int MaximumTransferUnit
    {
      get
      {
        return this.peerBase.mtu;
      }
      set
      {
        if (this.PeerState != PeerStateValue.Disconnected)
          throw new Exception("MaximumTransferUnit is only settable while disconnected. State: " + (object) this.PeerState);
        if (value < 520)
          value = 520;
        this.peerBase.mtu = value;
      }
    }

    public bool IsEncryptionAvailable
    {
      get
      {
        return this.peerBase.isEncryptionAvailable;
      }
    }

    public bool IsSendingOnlyAcks
    {
      get
      {
        return this.peerBase.IsSendingOnlyAcks;
      }
      set
      {
        lock (this.SendOutgoingLockObject)
          this.peerBase.IsSendingOnlyAcks = value;
      }
    }

    protected internal PhotonPeer(ConnectionProtocol protocolType)
    {
      if (protocolType == ConnectionProtocol.Tcp)
      {
        this.peerBase = (PeerBase) new TPeer();
        this.peerBase.usedProtocol = protocolType;
      }
      else if (protocolType == ConnectionProtocol.Udp)
      {
        this.peerBase = (PeerBase) new EnetPeer();
        this.peerBase.usedProtocol = protocolType;
      }
      else
      {
        if (protocolType != ConnectionProtocol.RHttp)
          return;
        this.peerBase = (PeerBase) new HttpBase3();
        this.peerBase.usedProtocol = protocolType;
      }
    }

    public PhotonPeer(IPhotonPeerListener listener, ConnectionProtocol protocolType)
      : this(protocolType)
    {
      if (listener == null)
        throw new Exception("listener cannot be null");
      this.Listener = listener;
    }

    [Obsolete("Use the constructor with ConnectionProtocol instead.")]
    public PhotonPeer(IPhotonPeerListener listener)
      : this(listener, ConnectionProtocol.Udp)
    {
    }

    [Obsolete("Use the constructor with ConnectionProtocol instead.")]
    public PhotonPeer(IPhotonPeerListener listener, bool useTcp)
      : this(listener, useTcp ? ConnectionProtocol.Tcp : ConnectionProtocol.Udp)
    {
    }

    public virtual bool Connect(string serverAddress, string applicationName)
    {
      lock (this.DispatchLockObject)
      {
        lock (this.SendOutgoingLockObject)
          return this.peerBase.Connect(serverAddress, applicationName);
      }
    }

    public virtual void Disconnect()
    {
      lock (this.DispatchLockObject)
      {
        lock (this.SendOutgoingLockObject)
          this.peerBase.Disconnect();
      }
    }

    public virtual void StopThread()
    {
      lock (this.DispatchLockObject)
      {
        lock (this.SendOutgoingLockObject)
          this.peerBase.StopConnection();
      }
    }

    public virtual void FetchServerTimestamp()
    {
      this.peerBase.FetchServerTimestamp();
    }

    public bool EstablishEncryption()
    {
      return this.peerBase.ExchangeKeysForEncryption();
    }

    public virtual void Service()
    {
      do
        ;
      while (this.DispatchIncomingCommands());
      do
        ;
      while (this.SendOutgoingCommands());
    }

    public virtual bool SendOutgoingCommands()
    {
      if (this.TrafficStatsEnabled)
        this.TrafficStatsGameLevel.SendOutgoingCommandsCalled();
      lock (this.SendOutgoingLockObject)
        return this.peerBase.SendOutgoingCommands();
    }

    public virtual bool SendAcksOnly()
    {
      if (this.TrafficStatsEnabled)
        this.TrafficStatsGameLevel.SendOutgoingCommandsCalled();
      lock (this.SendOutgoingLockObject)
        return this.peerBase.SendAcksOnly();
    }

    public virtual bool DispatchIncomingCommands()
    {
      this.peerBase.ByteCountCurrentDispatch = 0;
      if (this.TrafficStatsEnabled)
        this.TrafficStatsGameLevel.DispatchIncomingCommandsCalled();
      lock (this.DispatchLockObject)
        return this.peerBase.DispatchIncomingCommands();
    }

    public string VitalStatsToString(bool all)
    {
      if (this.TrafficStatsGameLevel == null)
        return "Stats not available. Use PhotonPeer.TrafficStatsEnabled.";
      if (!all)
        return string.Format("Rtt(variance): {0}({1}). Ms since last receive: {2}. Stats elapsed: {4}sec.\n{3}", (object) this.RoundTripTime, (object) this.RoundTripTimeVariance, (object) (SupportClass.GetTickCount() - this.TimestampOfLastSocketReceive), (object) this.TrafficStatsGameLevel.ToStringVitalStats(), (object) (this.TrafficStatsElapsedMs / 1000L));
      return string.Format("Rtt(variance): {0}({1}). Ms since last receive: {2}. Stats elapsed: {6}sec.\n{3}\n{4}\n{5}", (object) this.RoundTripTime, (object) this.RoundTripTimeVariance, (object) (SupportClass.GetTickCount() - this.TimestampOfLastSocketReceive), (object) this.TrafficStatsGameLevel.ToStringVitalStats(), (object) this.TrafficStatsIncoming.ToString(), (object) this.TrafficStatsOutgoing.ToString(), (object) (this.TrafficStatsElapsedMs / 1000L));
    }

    public virtual bool OpCustom(byte customOpCode, Dictionary<byte, object> customOpParameters, bool sendReliable)
    {
      return this.OpCustom(customOpCode, customOpParameters, sendReliable, (byte) 0);
    }

    public virtual bool OpCustom(byte customOpCode, Dictionary<byte, object> customOpParameters, bool sendReliable, byte channelId)
    {
      lock (this.EnqueueLock)
        return this.peerBase.EnqueueOperation(customOpParameters, customOpCode, sendReliable, channelId, false);
    }

    public virtual bool OpCustom(byte customOpCode, Dictionary<byte, object> customOpParameters, bool sendReliable, byte channelId, bool encrypt)
    {
      if (encrypt && !this.IsEncryptionAvailable)
        throw new ArgumentException("Can't use encryption yet. Exchange keys first.");
      lock (this.EnqueueLock)
        return this.peerBase.EnqueueOperation(customOpParameters, customOpCode, sendReliable, channelId, encrypt);
    }

    public virtual bool OpCustom(OperationRequest operationRequest, bool sendReliable, byte channelId, bool encrypt)
    {
      if (encrypt && !this.IsEncryptionAvailable)
        throw new ArgumentException("Can't use encryption yet. Exchange keys first.");
      lock (this.EnqueueLock)
        return this.peerBase.EnqueueOperation(operationRequest.Parameters, operationRequest.OperationCode, sendReliable, channelId, encrypt);
    }

    public static bool RegisterType(Type customType, byte code, SerializeMethod serializeMethod, DeserializeMethod constructor)
    {
      return Protocol.TryRegisterType(customType, code, serializeMethod, constructor);
    }
  }
}
