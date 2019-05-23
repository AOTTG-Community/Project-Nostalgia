﻿using ExitGames.Client.Photon.EncryptorManaged;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ExitGames.Client.Photon
{
    public class PhotonPeer
    {
        private readonly object DispatchLockObject = new object();
        private readonly object EnqueueLock = new object();
        private readonly object SendOutgoingLockObject = new object();
        private string clientVersion;
        private bool crcEnabled;
        private int mtu = 1200;
        private byte quickResendAttempts;
        private bool trafficStatsEnabled = false;
        private Stopwatch trafficStatsStopwatch;
        protected internal byte ClientSdkId = 15;
        protected internal byte[] PayloadEncryptionSecret;
        internal Decryptor decryptor;
        internal Encryptor encryptor;
        internal PeerBase peerBase;
        internal bool randomizeSequenceNumbers;
        internal byte[] sequenceNumberSource;
        public const bool DebugBuild = true;
        public const bool NativeDatagramEncrypt = false;
        public const bool NoSocket = false;
        public static bool AsyncKeyExchange = false;
        public static int OutgoingStreamBufferSize = 1200;
        public byte ChannelCount = 2;
        public DebugLevel DebugOut = DebugLevel.Error;
        public int DisconnectTimeout = 10000;

        [Obsolete("Used only for debugging. If true, the outgoing reliable sequence number is set to the channel's value, else it's added to the current value (most likely 1).")]
        public bool randomizeSequenceNumbersWillSetOutgoingValue;

        public int RhttpMaxConnections = 6;
        public int RhttpMinConnections = 2;
        public int SentCountAllowance = 7;
        public Dictionary<ConnectionProtocol, Type> SocketImplementationConfig;
        public int TimePingInterval = 1000;

        [Obsolete("Check QueuedOutgoingCommands and QueuedIncomingCommands on demand instead.")]
        public int WarningSize;

        public PhotonPeer(ConnectionProtocol protocolType)
        {
            this.TransportProtocol = protocolType;
            this.CreatePeerBase();
        }

        public PhotonPeer(IPhotonPeerListener listener, ConnectionProtocol protocolType) : this(protocolType)
        {
            this.Listener = listener;
        }

        protected internal byte ClientSdkIdShifted
        {
            get
            {
                byte b = 0;
                return (byte)((int)this.ClientSdkId << 1 | (int)b);
            }
        }

        public int ByteCountCurrentDispatch
        {
            get
            {
                return this.peerBase.ByteCountCurrentDispatch;
            }
        }

        public int ByteCountLastOperation
        {
            get
            {
                return this.peerBase.ByteCountLastOperation;
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

        public string ClientVersion
        {
            get
            {
                bool flag = string.IsNullOrEmpty(this.clientVersion);
                if (flag)
                {
                    this.clientVersion = string.Format("{0}.{1}.{2}.{3}", new object[]
                    {
                        Version.clientVersion[0],
                        Version.clientVersion[1],
                        Version.clientVersion[2],
                        Version.clientVersion[3]
                    });
                }
                return this.clientVersion;
            }
        }

        public int CommandBufferSize
        {
            get
            {
                EnetPeer enetPeer = this.peerBase as EnetPeer;
                return (enetPeer != null) ? enetPeer.commandBufferSize : -1;
            }
        }

        public string CommandInfoCurrentDispatch
        {
            get
            {
                return (this.peerBase.CommandInCurrentDispatch != null) ? this.peerBase.CommandInCurrentDispatch.ToString() : string.Empty;
            }
        }

        public int CommandLogSize
        {
            get
            {
                return this.peerBase.CommandLogSize;
            }
            set
            {
                this.peerBase.CommandLogSize = value;
            }
        }

        public int ConnectionTime
        {
            get
            {
                return this.peerBase.timeInt;
            }
        }

        public bool CrcEnabled
        {
            get
            {
                return this.crcEnabled;
            }
            set
            {
                bool flag = this.crcEnabled == value;
                if (!flag)
                {
                    bool flag2 = this.peerBase.peerConnectionState > ConnectionStateValue.Disconnected;
                    if (flag2)
                    {
                        throw new Exception("CrcEnabled can only be set while disconnected.");
                    }
                    this.crcEnabled = value;
                }
            }
        }

        public bool EnableServerTracing { get; set; }

        public bool IsEncryptionAvailable
        {
            get
            {
                return this.peerBase.isEncryptionAvailable;
            }
        }

        public bool IsSendingOnlyAcks { get; set; }

        public virtual bool IsSimulationEnabled
        {
            get
            {
                return this.NetworkSimulationSettings.IsSimulationEnabled;
            }
            set
            {
                bool flag = value == this.NetworkSimulationSettings.IsSimulationEnabled;
                if (!flag)
                {
                    object sendOutgoingLockObject = this.SendOutgoingLockObject;
                    lock (sendOutgoingLockObject)
                    {
                        this.NetworkSimulationSettings.IsSimulationEnabled = value;
                    }
                }
            }
        }

        public int LastSendAckTime
        {
            get
            {
                return this.peerBase.timeLastSendAck;
            }
        }

        public int LastSendOutgoingTime
        {
            get
            {
                return this.peerBase.timeLastSendOutgoing;
            }
        }

        public int LimitOfUnreliableCommands { get; set; }
        public IPhotonPeerListener Listener { get; protected set; }

        public SupportClass.IntegerMillisecondsDelegate LocalMsTimestampDelegate
        {
            set
            {
                bool flag = this.PeerState > PeerStateValue.Disconnected;
                if (flag)
                {
                    throw new Exception("LocalMsTimestampDelegate only settable while disconnected. State: " + this.PeerState);
                }
                SupportClass.IntegerMilliseconds = value;
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

        public int MaximumTransferUnit
        {
            get
            {
                return this.mtu;
            }
            set
            {
                bool flag = this.PeerState > PeerStateValue.Disconnected;
                if (flag)
                {
                    throw new Exception("MaximumTransferUnit is only settable while disconnected. State: " + this.PeerState);
                }
                bool flag2 = value < 576;
                if (flag2)
                {
                    value = 576;
                }
                this.mtu = value;
            }
        }

        public NetworkSimulationSet NetworkSimulationSettings
        {
            get
            {
                return this.peerBase.NetworkSimulationSettings;
            }
        }

        public int PacketLossByChallenge
        {
            get
            {
                return this.peerBase.packetLossByChallenge;
            }
        }

        public int PacketLossByCrc
        {
            get
            {
                return this.peerBase.packetLossByCrc;
            }
        }

        public string PeerID
        {
            get
            {
                return this.peerBase.PeerID;
            }
        }

        public PeerStateValue PeerState
        {
            get
            {
                bool flag = this.peerBase.peerConnectionState == ConnectionStateValue.Connected && !this.peerBase.ApplicationIsInitialized;
                PeerStateValue result;
                if (flag)
                {
                    result = PeerStateValue.InitializingApplication;
                }
                else
                {
                    result = (PeerStateValue)this.peerBase.peerConnectionState;
                }
                return result;
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

        public byte QuickResendAttempts
        {
            get
            {
                return this.quickResendAttempts;
            }
            set
            {
                this.quickResendAttempts = value;
                bool flag = this.quickResendAttempts > 4;
                if (flag)
                {
                    this.quickResendAttempts = 4;
                }
            }
        }

        public int ResentReliableCommands
        {
            get
            {
                return (this.UsedProtocol != ConnectionProtocol.Udp) ? 0 : ((EnetPeer)this.peerBase).reliableCommandsRepeated;
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

        public SerializationProtocol SerializationProtocolType { get; set; }

        public string ServerAddress
        {
            get
            {
                return this.peerBase.ServerAddress;
            }
            set
            {
                bool flag = this.DebugOut >= DebugLevel.Error;
                if (flag)
                {
                    this.Listener.DebugReturn(DebugLevel.Error, "Failed to set ServerAddress. Can be set only when using HTTP.");
                }
            }
        }

        public int ServerTimeInMilliSeconds
        {
            get
            {
                return this.peerBase.serverTimeOffsetIsAvailable ? (this.peerBase.serverTimeOffset + SupportClass.GetTickCount()) : 0;
            }
        }

        public Type SocketImplementation { get; internal set; }

        public int TimestampOfLastSocketReceive
        {
            get
            {
                return this.peerBase.timestampOfLastReceive;
            }
        }

        public long TrafficStatsElapsedMs
        {
            get
            {
                return (this.trafficStatsStopwatch != null) ? this.trafficStatsStopwatch.ElapsedMilliseconds : 0L;
            }
        }

        public bool TrafficStatsEnabled
        {
            get
            {
                return this.trafficStatsEnabled;
            }
            set
            {
                bool flag = this.trafficStatsEnabled == value;
                if (!flag)
                {
                    this.trafficStatsEnabled = value;
                    if (value)
                    {
                        bool flag2 = this.trafficStatsStopwatch == null;
                        if (flag2)
                        {
                            this.InitializeTrafficStats();
                        }
                        this.trafficStatsStopwatch.Start();
                    }
                    else
                    {
                        bool flag3 = this.trafficStatsStopwatch != null;
                        if (flag3)
                        {
                            this.trafficStatsStopwatch.Stop();
                        }
                    }
                }
            }
        }

        public TrafficStatsGameLevel TrafficStatsGameLevel { get; internal set; }

        public TrafficStats TrafficStatsIncoming { get; internal set; }

        public TrafficStats TrafficStatsOutgoing { get; internal set; }

        public ConnectionProtocol TransportProtocol { get; set; }

        public ConnectionProtocol UsedProtocol
        {
            get
            {
                return this.peerBase.usedTransportProtocol;
            }
        }

        private void CreatePeerBase()
        {
            bool flag = this.SocketImplementationConfig == null;
            if (flag)
            {
                this.SocketImplementationConfig = new Dictionary<ConnectionProtocol, Type>(5);
                this.SocketImplementationConfig.Add(ConnectionProtocol.Udp, typeof(SocketUdp));
                this.SocketImplementationConfig.Add(ConnectionProtocol.Tcp, typeof(SocketTcp));
            }
            switch (this.TransportProtocol)
            {
                case ConnectionProtocol.Tcp:
                    this.peerBase = new TPeer();
                    goto IL_9D;
                case ConnectionProtocol.WebSocket:
                case ConnectionProtocol.WebSocketSecure:
                    this.peerBase = new TPeer
                    {
                        DoFraming = false
                    };
                    goto IL_9D;
            }
            this.peerBase = new EnetPeer();
            IL_9D:
            bool flag2 = this.peerBase == null;
            if (flag2)
            {
                throw new Exception("No PeerBase");
            }
            this.peerBase.photonPeer = this;
            this.peerBase.usedTransportProtocol = this.TransportProtocol;
            Type socketImplementation = null;
            this.SocketImplementationConfig.TryGetValue(this.TransportProtocol, out socketImplementation);
            this.SocketImplementation = socketImplementation;
        }

        internal void InitializeTrafficStats()
        {
            this.TrafficStatsIncoming = new TrafficStats(this.peerBase.TrafficPackageHeaderSize);
            this.TrafficStatsOutgoing = new TrafficStats(this.peerBase.TrafficPackageHeaderSize);
            this.TrafficStatsGameLevel = new TrafficStatsGameLevel();
            this.trafficStatsStopwatch = new Stopwatch();
        }

        public static void MessageBufferPoolTrim(int countOfBuffers)
        {
            bool flag = countOfBuffers <= 0;
            if (flag)
            {
                PeerBase.MessageBufferPool.Clear();
            }
            else
            {
                bool flag2 = countOfBuffers >= PeerBase.MessageBufferPool.Count;
                if (!flag2)
                {
                    while (PeerBase.MessageBufferPool.Count > countOfBuffers)
                    {
                        PeerBase.MessageBufferPool.Dequeue();
                    }
                    PeerBase.MessageBufferPool.TrimExcess();
                }
            }
        }

        public static bool RegisterType(Type customType, byte code, SerializeMethod serializeMethod, DeserializeMethod constructor)
        {
            return Protocol.TryRegisterType(customType, code, serializeMethod, constructor);
        }

        public string CommandLogToString()
        {
            return this.peerBase.CommandLogToString();
        }

        public virtual bool Connect(string serverAddress, string applicationName)
        {
            return this.Connect(serverAddress, applicationName, null);
        }

        public virtual bool Connect(string serverAddress, string applicationName, object custom)
        {
            object dispatchLockObject = this.DispatchLockObject;
            bool result;
            lock (dispatchLockObject)
            {
                object sendOutgoingLockObject = this.SendOutgoingLockObject;
                lock (sendOutgoingLockObject)
                {
                    this.CreatePeerBase();
                    bool flag = this.peerBase == null;
                    if (flag)
                    {
                        result = false;
                    }
                    else
                    {
                        bool flag2 = this.peerBase.SocketImplementation == null;
                        if (flag2)
                        {
                            this.peerBase.EnqueueDebugReturn(DebugLevel.Error, string.Concat(new object[]
                            {
                                "Connect failed. SocketImplementationConfig is null for protocol ",
                                this.TransportProtocol,
                                ": ",
                                SupportClass.DictionaryToString(this.SocketImplementationConfig)
                            }));
                            result = false;
                        }
                        else
                        {
                            this.peerBase.CustomInitData = custom;
                            this.peerBase.AppId = applicationName;
                            result = this.peerBase.Connect(serverAddress, applicationName, custom);
                        }
                    }
                }
            }
            return result;
        }

        public virtual void Disconnect()
        {
            object dispatchLockObject = this.DispatchLockObject;
            lock (dispatchLockObject)
            {
                object sendOutgoingLockObject = this.SendOutgoingLockObject;
                lock (sendOutgoingLockObject)
                {
                    this.peerBase.Disconnect();
                }
            }
        }

        public virtual bool DispatchIncomingCommands()
        {
            bool flag = this.TrafficStatsEnabled;
            if (flag)
            {
                this.TrafficStatsGameLevel.DispatchIncomingCommandsCalled();
            }
            object dispatchLockObject = this.DispatchLockObject;
            bool result;
            lock (dispatchLockObject)
            {
                this.peerBase.ByteCountCurrentDispatch = 0;
                result = this.peerBase.DispatchIncomingCommands();
            }
            return result;
        }

        public bool EstablishEncryption()
        {
            bool asyncKeyExchange = PhotonPeer.AsyncKeyExchange;
            bool result;
            if (asyncKeyExchange)
            {
                SupportClass.CallInBackground(delegate
                {
                    this.peerBase.ExchangeKeysForEncryption(this.SendOutgoingLockObject);
                    return false;
                }, 100, "");
                result = true;
            }
            else
            {
                result = this.peerBase.ExchangeKeysForEncryption(this.SendOutgoingLockObject);
            }
            return result;
        }

        public virtual void FetchServerTimestamp()
        {
            this.peerBase.FetchServerTimestamp();
        }

        public bool InitDatagramEncryption(byte[] encryptionSecret, byte[] hmacSecret, bool randomizedSequenceNumbers = false)
        {
            try
            {
                this.encryptor = new Encryptor();
                this.encryptor.Init(encryptionSecret, hmacSecret);
                this.decryptor = new Decryptor();
                this.decryptor.Init(encryptionSecret, hmacSecret);
            }
            catch (Exception)
            {
                return false;
            }
            if (randomizedSequenceNumbers)
            {
                this.sequenceNumberSource = encryptionSecret;
                this.randomizeSequenceNumbers = true;
            }
            return true;
        }

        public void InitPayloadEncryption(byte[] secret)
        {
            this.PayloadEncryptionSecret = secret;
        }

        [Obsolete("Use SendOperation() or SendMessage().")]
        public virtual bool OpCustom(byte customOpCode, Dictionary<byte, object> customOpParameters, bool sendReliable, byte channelId = 0, bool encrypt = false)
        {
            bool flag = this.peerBase.usedTransportProtocol == ConnectionProtocol.WebSocketSecure;
            if (flag)
            {
                encrypt = false;
            }
            bool flag2 = encrypt && !this.IsEncryptionAvailable;
            if (flag2)
            {
                throw new ArgumentException("Can't use encryption yet. Exchange keys first.");
            }
            object enqueueLock = this.EnqueueLock;
            bool result;
            lock (enqueueLock)
            {
                SendOptions sendParams = new SendOptions
                {
                    Channel = channelId,
                    Encrypt = encrypt,
                    DeliveryMode = (sendReliable ? DeliveryMode.Reliable : DeliveryMode.Unreliable)
                };
                result = this.peerBase.EnqueueOperation(customOpParameters, customOpCode, sendParams, EgMessageType.Operation);
            }
            return result;
        }

        [Obsolete("Use SendOperation() or SendMessage().")]
        public virtual bool OpCustom(OperationRequest operationRequest, bool sendReliable, byte channelId, bool encrypt)
        {
            bool flag = this.peerBase.usedTransportProtocol == ConnectionProtocol.WebSocketSecure;
            if (flag)
            {
                encrypt = false;
            }
            bool flag2 = encrypt && !this.IsEncryptionAvailable;
            if (flag2)
            {
                throw new ArgumentException("Can't use encryption yet. Exchange keys first.");
            }
            object enqueueLock = this.EnqueueLock;
            bool result;
            lock (enqueueLock)
            {
                SendOptions sendParams = new SendOptions
                {
                    Channel = channelId,
                    Encrypt = encrypt,
                    DeliveryMode = (sendReliable ? DeliveryMode.Reliable : DeliveryMode.Unreliable)
                };
                result = this.peerBase.EnqueueOperation(operationRequest.Parameters, operationRequest.OperationCode, sendParams, EgMessageType.Operation);
            }
            return result;
        }

        public virtual bool SendAcksOnly()
        {
            bool flag = this.TrafficStatsEnabled;
            if (flag)
            {
                this.TrafficStatsGameLevel.SendOutgoingCommandsCalled();
            }
            object sendOutgoingLockObject = this.SendOutgoingLockObject;
            bool result;
            lock (sendOutgoingLockObject)
            {
                result = this.peerBase.SendAcksOnly();
            }
            return result;
        }

        public virtual bool SendOperation(byte operationCode, Dictionary<byte, object> operationParameters, SendOptions sendOptions)
        {
            bool flag = this.peerBase.usedTransportProtocol == ConnectionProtocol.WebSocketSecure;
            if (flag)
            {
                sendOptions.Encrypt = false;
            }
            object enqueueLock = this.EnqueueLock;
            bool result;
            lock (enqueueLock)
            {
                result = this.peerBase.EnqueueOperation(operationParameters, operationCode, sendOptions, EgMessageType.Operation);
            }
            return result;
        }

        public virtual bool SendOutgoingCommands()
        {
            bool flag = this.TrafficStatsEnabled;
            if (flag)
            {
                this.TrafficStatsGameLevel.SendOutgoingCommandsCalled();
            }
            object sendOutgoingLockObject = this.SendOutgoingLockObject;
            bool result;
            lock (sendOutgoingLockObject)
            {
                result = this.peerBase.SendOutgoingCommands();
            }
            return result;
        }

        public virtual void Service()
        {
            while (this.DispatchIncomingCommands())
            {
            }
            while (this.SendOutgoingCommands())
            {
            }
        }

        public virtual void StopThread()
        {
            object dispatchLockObject = this.DispatchLockObject;
            lock (dispatchLockObject)
            {
                object sendOutgoingLockObject = this.SendOutgoingLockObject;
                lock (sendOutgoingLockObject)
                {
                    this.peerBase.StopConnection();
                }
            }
        }

        public void TrafficStatsReset()
        {
            this.TrafficStatsEnabled = false;
            this.InitializeTrafficStats();
            this.TrafficStatsEnabled = true;
        }

        public string VitalStatsToString(bool all)
        {
            bool flag = this.TrafficStatsGameLevel == null;
            string result;
            if (flag)
            {
                result = "Stats not available. Use PhotonPeer.TrafficStatsEnabled.";
            }
            else
            {
                bool flag2 = !all;
                if (flag2)
                {
                    result = string.Format("Rtt(variance): {0}({1}). Ms since last receive: {2}. Stats elapsed: {4}sec.\n{3}", new object[]
                    {
                        this.RoundTripTime,
                        this.RoundTripTimeVariance,
                        SupportClass.GetTickCount() - this.TimestampOfLastSocketReceive,
                        this.TrafficStatsGameLevel.ToStringVitalStats(),
                        this.TrafficStatsElapsedMs / 1000L
                    });
                }
                else
                {
                    result = string.Format("Rtt(variance): {0}({1}). Ms since last receive: {2}. Stats elapsed: {6}sec.\n{3}\n{4}\n{5}", new object[]
                    {
                        this.RoundTripTime,
                        this.RoundTripTimeVariance,
                        SupportClass.GetTickCount() - this.TimestampOfLastSocketReceive,
                        this.TrafficStatsGameLevel.ToStringVitalStats(),
                        this.TrafficStatsIncoming.ToString(),
                        this.TrafficStatsOutgoing.ToString(),
                        this.TrafficStatsElapsedMs / 1000L
                    });
                }
            }
            return result;
        }
    }
}