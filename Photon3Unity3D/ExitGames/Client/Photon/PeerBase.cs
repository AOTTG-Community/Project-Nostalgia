using Photon.SocketServer.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace ExitGames.Client.Photon
{
    public abstract class PeerBase
    {
        private readonly Random lagRandomizer = new Random();
        private readonly NetworkSimulationSet networkSimulationSettings = new NetworkSimulationSet();
        private int commandLogSize;
        protected internal static Queue<StreamBuffer> MessageBufferPool = new Queue<StreamBuffer>(32);
        internal static short peerCount;
        internal readonly Queue<PeerBase.MyAction> ActionQueue = new Queue<PeerBase.MyAction>();
        internal readonly LinkedList<SimulationItem> NetSimListIncoming = new LinkedList<SimulationItem>();
        internal readonly LinkedList<SimulationItem> NetSimListOutgoing = new LinkedList<SimulationItem>();
        internal string AppId;
        internal bool ApplicationIsInitialized;
        internal int ByteCountCurrentDispatch;
        internal int ByteCountLastOperation;
        internal long bytesIn;
        internal long bytesOut;
        internal NCommand CommandInCurrentDispatch;
        internal Queue<CmdLogItem> CommandLog;
        internal ICryptoProvider CryptoProvider;
        internal object CustomInitData;
        internal int highestRoundTripTimeVariance;
        internal Queue<CmdLogItem> InReliableLog;
        internal bool isEncryptionAvailable;
        internal int lastRoundTripTime;
        internal int lastRoundTripTimeVariance;
        internal int lowestRoundTripTime;
        internal int outgoingCommandsInStream = 0;
        internal int packetLossByChallenge;
        internal int packetLossByCrc;
        internal ConnectionStateValue peerConnectionState;
        internal short peerID = -1;
        internal PhotonPeer photonPeer;

        internal IPhotonSocket PhotonSocket;
        internal int roundTripTime;
        internal int roundTripTimeVariance;
        internal IProtocol serializationProtocol;

        internal int serverTimeOffset;
        internal bool serverTimeOffsetIsAvailable;
        internal int timeBase;
        internal int timeInt;
        internal int timeLastAckReceive;
        internal int timeLastSendAck;
        internal int timeLastSendOutgoing;
        internal int timeoutInt;
        internal int timestampOfLastReceive;
        internal int TrafficPackageHeaderSize;
        internal ConnectionProtocol usedTransportProtocol;

        protected PeerBase()
        {
            this.networkSimulationSettings.peerBase = this;
            PeerBase.peerCount += 1;
        }

        internal delegate void MyAction();

        protected internal bool IsIpv6
        {
            get
            {
                return this.PhotonSocket != null && this.PhotonSocket.AddressResolvedAsIpv6;
            }
        }

        internal static int outgoingStreamBufferSize
        {
            get
            {
                return PhotonPeer.OutgoingStreamBufferSize;
            }
        }

        internal long BytesIn
        {
            get
            {
                return this.bytesIn;
            }
        }

        internal long BytesOut
        {
            get
            {
                return this.bytesOut;
            }
        }

        internal byte ChannelCount
        {
            get
            {
                return this.photonPeer.ChannelCount;
            }
        }

        internal int CommandLogSize
        {
            get
            {
                return this.commandLogSize;
            }
            set
            {
                this.commandLogSize = value;
                this.CommandLogResize();
            }
        }

        internal DebugLevel debugOut
        {
            get
            {
                return this.photonPeer.DebugOut;
            }
        }

        internal int DisconnectTimeout
        {
            get
            {
                return this.photonPeer.DisconnectTimeout;
            }
        }

        internal bool IsSendingOnlyAcks
        {
            get
            {
                return this.photonPeer.IsSendingOnlyAcks;
            }
        }

        internal IPhotonPeerListener Listener
        {
            get
            {
                return this.photonPeer.Listener;
            }
        }

        internal int mtu
        {
            get
            {
                return this.photonPeer.MaximumTransferUnit;
            }
        }

        internal abstract int QueuedIncomingCommandsCount { get; }

        internal abstract int QueuedOutgoingCommandsCount { get; }

        internal Type SocketImplementation
        {
            get
            {
                return this.photonPeer.SocketImplementation;
            }
        }

        internal int timePingInterval
        {
            get
            {
                return this.photonPeer.TimePingInterval;
            }
        }

        internal bool TrafficStatsEnabled
        {
            get
            {
                return this.photonPeer.TrafficStatsEnabled;
            }
        }

        internal TrafficStatsGameLevel TrafficStatsGameLevel
        {
            get
            {
                return this.photonPeer.TrafficStatsGameLevel;
            }
        }

        internal TrafficStats TrafficStatsIncoming
        {
            get
            {
                return this.photonPeer.TrafficStatsIncoming;
            }
        }

        internal TrafficStats TrafficStatsOutgoing
        {
            get
            {
                return this.photonPeer.TrafficStatsOutgoing;
            }
        }

        public NetworkSimulationSet NetworkSimulationSettings
        {
            get
            {
                return this.networkSimulationSettings;
            }
        }

        public virtual string PeerID
        {
            get
            {
                return ((ushort)this.peerID).ToString();
            }
        }

        public string ServerAddress { get; internal set; }

        private string GetHttpKeyValueString(Dictionary<string, string> dic)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (KeyValuePair<string, string> keyValuePair in dic)
            {
                stringBuilder.Append(keyValuePair.Key).Append("=").Append(keyValuePair.Value).Append("&");
            }
            return stringBuilder.ToString();
        }

        protected internal void NetworkSimRun()
        {
            for (; ; )
            {
                bool flag = false;
                ManualResetEvent netSimManualResetEvent = this.networkSimulationSettings.NetSimManualResetEvent;
                lock (netSimManualResetEvent)
                {
                    flag = this.networkSimulationSettings.IsSimulationEnabled;
                }
                bool flag2 = !flag;
                if (flag2)
                {
                    this.networkSimulationSettings.NetSimManualResetEvent.WaitOne();
                }
                else
                {
                    LinkedList<SimulationItem> netSimListIncoming = this.NetSimListIncoming;
                    lock (netSimListIncoming)
                    {
                        while (this.NetSimListIncoming.First != null)
                        {
                            SimulationItem value = this.NetSimListIncoming.First.Value;
                            bool flag3 = value.stopw.ElapsedMilliseconds < (long)value.Delay;
                            if (flag3)
                            {
                                break;
                            }
                            this.ReceiveIncomingCommands(value.DelayedData, value.DelayedData.Length);
                            this.NetSimListIncoming.RemoveFirst();
                        }
                    }
                    LinkedList<SimulationItem> netSimListOutgoing = this.NetSimListOutgoing;
                    lock (netSimListOutgoing)
                    {
                        while (this.NetSimListOutgoing.First != null)
                        {
                            SimulationItem value2 = this.NetSimListOutgoing.First.Value;
                            bool flag4 = value2.stopw.ElapsedMilliseconds < (long)value2.Delay;
                            if (flag4)
                            {
                                break;
                            }
                            bool flag5 = this.PhotonSocket != null && this.PhotonSocket.Connected;
                            if (flag5)
                            {
                                this.PhotonSocket.Send(value2.DelayedData, value2.DelayedData.Length);
                            }
                            this.NetSimListOutgoing.RemoveFirst();
                        }
                    }
                    Thread.Sleep(0);
                }
            }
        }

        internal void CommandLogInit()
        {
            bool flag = this.CommandLogSize <= 0;
            if (flag)
            {
                this.CommandLog = null;
                this.InReliableLog = null;
            }
            else
            {
                bool flag2 = this.CommandLog == null || this.InReliableLog == null;
                if (flag2)
                {
                    this.CommandLog = new Queue<CmdLogItem>(this.CommandLogSize);
                    this.InReliableLog = new Queue<CmdLogItem>(this.CommandLogSize);
                }
                else
                {
                    this.CommandLog.Clear();
                    this.InReliableLog.Clear();
                }
            }
        }

        internal void CommandLogResize()
        {
            bool flag = this.CommandLogSize <= 0;
            if (flag)
            {
                this.CommandLog = null;
                this.InReliableLog = null;
            }
            else
            {
                bool flag2 = this.CommandLog == null || this.InReliableLog == null;
                if (flag2)
                {
                    this.CommandLogInit();
                }
                while (this.CommandLog.Count > 0 && this.CommandLog.Count > this.CommandLogSize)
                {
                    this.CommandLog.Dequeue();
                }
                while (this.InReliableLog.Count > 0 && this.InReliableLog.Count > this.CommandLogSize)
                {
                    this.InReliableLog.Dequeue();
                }
            }
        }

        internal abstract bool Connect(string serverAddress, string appID, object customData = null);

        internal void DeriveSharedKey(OperationResponse operationResponse)
        {
            bool flag = operationResponse.ReturnCode != 0;
            if (flag)
            {
                this.EnqueueDebugReturn(DebugLevel.Error, "Establishing encryption keys failed. " + operationResponse.ToStringFull());
                this.EnqueueStatusCallback(StatusCode.EncryptionFailedToEstablish);
            }
            else
            {
                byte[] array = (byte[])operationResponse[PhotonCodes.ServerKey];
                bool flag2 = array == null || array.Length == 0;
                if (flag2)
                {
                    this.EnqueueDebugReturn(DebugLevel.Error, "Establishing encryption keys failed. Server's public key is null or empty. " + operationResponse.ToStringFull());
                    this.EnqueueStatusCallback(StatusCode.EncryptionFailedToEstablish);
                }
                else
                {
                    this.CryptoProvider.DeriveSharedKey(array);
                    this.isEncryptionAvailable = true;
                    this.EnqueueStatusCallback(StatusCode.EncryptionEstablished);
                }
            }
        }

        internal virtual bool DeserializeMessageAndCallback(StreamBuffer stream)
        {
            bool flag = stream.Length < 2;
            bool result;
            if (flag)
            {
                bool flag2 = this.debugOut >= DebugLevel.Error;
                if (flag2)
                {
                    this.Listener.DebugReturn(DebugLevel.Error, "Incoming UDP data too short! " + stream.Length);
                }
                result = false;
            }
            else
            {
                byte b = (byte)stream.ReadByte();
                bool flag3 = b != 243 && b != 253;
                if (flag3)
                {
                    bool flag4 = this.debugOut >= DebugLevel.Error;
                    if (flag4)
                    {
                        this.Listener.DebugReturn(DebugLevel.All, "No regular operation UDP message: " + b);
                    }
                    result = false;
                }
                else
                {
                    byte b2 = (byte)stream.ReadByte();
                    byte b3 = (byte)(b2 & 127);
                    bool flag5 = (b2 & 128) > 0;
                    bool flag6 = b3 != 1;
                    if (flag6)
                    {
                        try
                        {
                            bool flag7 = flag5;
                            if (flag7)
                            {
                                byte[] buf = this.CryptoProvider.Decrypt(stream.GetBuffer(), 2, stream.Length - 2);
                                stream = new StreamBuffer(buf);
                            }
                            else
                            {
                                stream.Seek(2L, SeekOrigin.Begin);
                            }
                        }
                        catch (Exception ex)
                        {
                            bool flag8 = this.debugOut >= DebugLevel.Error;
                            if (flag8)
                            {
                                this.Listener.DebugReturn(DebugLevel.Error, string.Concat(new object[]
                                {
                                    "msgType: ",
                                    b3,
                                    " exception: ",
                                    ex.ToString()
                                }));
                            }
                            SupportClass.WriteStackTrace(ex);
                            return false;
                        }
                    }
                    int num = 0;
                    switch (b3)
                    {
                        case 1:
                            this.InitCallback();
                            goto IL_43E;
                        case 3:
                            {
                                OperationResponse operationResponse = this.serializationProtocol.DeserializeOperationResponse(stream);
                                bool trafficStatsEnabled = this.TrafficStatsEnabled;
                                if (trafficStatsEnabled)
                                {
                                    this.TrafficStatsGameLevel.CountResult(this.ByteCountCurrentDispatch);
                                    num = SupportClass.GetTickCount();
                                }
                                this.Listener.OnOperationResponse(operationResponse);
                                bool trafficStatsEnabled2 = this.TrafficStatsEnabled;
                                if (trafficStatsEnabled2)
                                {
                                    this.TrafficStatsGameLevel.TimeForResponseCallback(operationResponse.OperationCode, SupportClass.GetTickCount() - num);
                                }
                                goto IL_43E;
                            }
                        case 4:
                            {
                                EventData eventData = this.serializationProtocol.DeserializeEventData(stream);
                                bool trafficStatsEnabled3 = this.TrafficStatsEnabled;
                                if (trafficStatsEnabled3)
                                {
                                    this.TrafficStatsGameLevel.CountEvent(this.ByteCountCurrentDispatch);
                                    num = SupportClass.GetTickCount();
                                }
                                this.Listener.OnEvent(eventData);
                                bool trafficStatsEnabled4 = this.TrafficStatsEnabled;
                                if (trafficStatsEnabled4)
                                {
                                    this.TrafficStatsGameLevel.TimeForEventCallback(eventData.Code, SupportClass.GetTickCount() - num);
                                }
                                goto IL_43E;
                            }
                        case 7:
                            {
                                OperationResponse operationResponse = this.serializationProtocol.DeserializeOperationResponse(stream);
                                bool trafficStatsEnabled5 = this.TrafficStatsEnabled;
                                if (trafficStatsEnabled5)
                                {
                                    this.TrafficStatsGameLevel.CountResult(this.ByteCountCurrentDispatch);
                                    num = SupportClass.GetTickCount();
                                }
                                bool flag9 = operationResponse.OperationCode == PhotonCodes.InitEncryption;
                                if (flag9)
                                {
                                    this.DeriveSharedKey(operationResponse);
                                }
                                else
                                {
                                    bool flag10 = operationResponse.OperationCode == PhotonCodes.Ping;
                                    if (flag10)
                                    {
                                        TPeer tpeer = this as TPeer;
                                        bool flag11 = tpeer != null;
                                        if (flag11)
                                        {
                                            tpeer.ReadPingResult(operationResponse);
                                        }
                                    }
                                    else
                                    {
                                        this.EnqueueDebugReturn(DebugLevel.Error, "Received unknown internal operation. " + operationResponse.ToStringFull());
                                    }
                                }
                                bool trafficStatsEnabled6 = this.TrafficStatsEnabled;
                                if (trafficStatsEnabled6)
                                {
                                    this.TrafficStatsGameLevel.TimeForResponseCallback(operationResponse.OperationCode, SupportClass.GetTickCount() - num);
                                }
                                goto IL_43E;
                            }
                        case 8:
                            {
                                object obj = this.serializationProtocol.DeserializeMessage(stream);
                                bool trafficStatsEnabled7 = this.TrafficStatsEnabled;
                                if (trafficStatsEnabled7)
                                {
                                    this.TrafficStatsGameLevel.CountEvent(this.ByteCountCurrentDispatch);
                                    num = SupportClass.GetTickCount();
                                }
                                bool trafficStatsEnabled8 = this.TrafficStatsEnabled;
                                if (trafficStatsEnabled8)
                                {
                                    this.TrafficStatsGameLevel.TimeForMessageCallback(SupportClass.GetTickCount() - num);
                                }
                                goto IL_43E;
                            }
                        case 9:
                            {
                                bool trafficStatsEnabled9 = this.TrafficStatsEnabled;
                                if (trafficStatsEnabled9)
                                {
                                    this.TrafficStatsGameLevel.CountEvent(this.ByteCountCurrentDispatch);
                                    num = SupportClass.GetTickCount();
                                }
                                byte[] array = stream.ToArrayFromPos();
                                bool trafficStatsEnabled10 = this.TrafficStatsEnabled;
                                if (trafficStatsEnabled10)
                                {
                                    this.TrafficStatsGameLevel.TimeForRawMessageCallback(SupportClass.GetTickCount() - num);
                                }
                                goto IL_43E;
                            }
                    }
                    this.EnqueueDebugReturn(DebugLevel.Error, "unexpected msgType " + b3);
                    IL_43E:
                    result = true;
                }
            }
            return result;
        }

        internal abstract void Disconnect();

        internal abstract bool DispatchIncomingCommands();

        internal void EnqueueActionForDispatch(PeerBase.MyAction action)
        {
            Queue<PeerBase.MyAction> actionQueue = this.ActionQueue;
            lock (actionQueue)
            {
                this.ActionQueue.Enqueue(action);
            }
        }

        internal void EnqueueDebugReturn(DebugLevel level, string debugReturn)
        {
            Queue<PeerBase.MyAction> actionQueue = this.ActionQueue;
            lock (actionQueue)
            {
                this.ActionQueue.Enqueue(delegate
                {
                    this.Listener.DebugReturn(level, debugReturn);
                });
            }
        }

        internal abstract bool EnqueueMessage(object message, SendOptions sendOptions);

        internal abstract bool EnqueueOperation(Dictionary<byte, object> parameters, byte opCode, SendOptions sendParams, EgMessageType messageType = EgMessageType.Operation);

        internal void EnqueueStatusCallback(StatusCode statusValue)
        {
            Queue<PeerBase.MyAction> actionQueue = this.ActionQueue;
            lock (actionQueue)
            {
                this.ActionQueue.Enqueue(delegate
                {
                    this.Listener.OnStatusChanged(statusValue);
                });
            }
        }

        internal bool ExchangeKeysForEncryption(object lockObject)
        {
            this.isEncryptionAvailable = false;
            bool flag = this.CryptoProvider != null;
            if (flag)
            {
                this.CryptoProvider.Dispose();
                this.CryptoProvider = null;
            }
            bool flag2 = this.CryptoProvider == null;
            if (flag2)
            {
                this.CryptoProvider = new DiffieHellmanCryptoProvider();
            }
            Dictionary<byte, object> dictionary = new Dictionary<byte, object>(1);
            dictionary[PhotonCodes.ClientKey] = this.CryptoProvider.PublicKey;
            bool flag3 = lockObject != null;
            if (flag3)
            {
                lock (lockObject)
                {
                    SendOptions sendParams = new SendOptions
                    {
                        Channel = 0,
                        Encrypt = false,
                        DeliveryMode = DeliveryMode.Reliable
                    };
                    return this.EnqueueOperation(dictionary, PhotonCodes.InitEncryption, sendParams, EgMessageType.InternalOperationRequest);
                }
            }
            SendOptions sendParams2 = new SendOptions
            {
                Channel = 0,
                Encrypt = false,
                DeliveryMode = DeliveryMode.Reliable
            };
            return this.EnqueueOperation(dictionary, PhotonCodes.InitEncryption, sendParams2, EgMessageType.InternalOperationRequest);
        }

        internal abstract void FetchServerTimestamp();

        internal void InitCallback()
        {
            bool flag = this.peerConnectionState == ConnectionStateValue.Connecting;
            if (flag)
            {
                this.peerConnectionState = ConnectionStateValue.Connected;
            }
            this.ApplicationIsInitialized = true;
            this.FetchServerTimestamp();
            this.Listener.OnStatusChanged(StatusCode.Connect);
        }

        internal virtual void InitEncryption(byte[] secret)
        {
            bool flag = this.CryptoProvider == null;
            if (flag)
            {
                this.CryptoProvider = new DiffieHellmanCryptoProvider(secret);
                this.isEncryptionAvailable = true;
            }
        }

        internal virtual void InitPeerBase()
        {
            this.serializationProtocol = SerializationProtocolFactory.Create(SerializationProtocol.GpBinaryV16);
            this.photonPeer.InitializeTrafficStats();
            this.ByteCountLastOperation = 0;
            this.ByteCountCurrentDispatch = 0;
            this.bytesIn = 0L;
            this.bytesOut = 0L;
            this.packetLossByCrc = 0;
            this.packetLossByChallenge = 0;
            this.networkSimulationSettings.LostPackagesIn = 0;
            this.networkSimulationSettings.LostPackagesOut = 0;
            LinkedList<SimulationItem> netSimListOutgoing = this.NetSimListOutgoing;
            lock (netSimListOutgoing)
            {
                this.NetSimListOutgoing.Clear();
            }
            LinkedList<SimulationItem> netSimListIncoming = this.NetSimListIncoming;
            lock (netSimListIncoming)
            {
                this.NetSimListIncoming.Clear();
            }
            this.peerConnectionState = ConnectionStateValue.Disconnected;
            this.timeBase = SupportClass.GetTickCount();
            this.isEncryptionAvailable = false;
            this.ApplicationIsInitialized = false;
            this.roundTripTime = 200;
            this.roundTripTimeVariance = 5;
            this.serverTimeOffsetIsAvailable = false;
            this.serverTimeOffset = 0;
        }

        internal string PepareWebSocketUrl(string serverAddress, string appId, object customData)
        {
            StringBuilder stringBuilder = new StringBuilder(1024);
            string empty = string.Empty;
            bool flag = customData != null;
            if (flag)
            {
                byte[] array = this.serializationProtocol.Serialize(customData);
                bool flag2 = array == null;
                if (flag2)
                {
                    this.EnqueueDebugReturn(DebugLevel.Error, "Can not deserialize custom data");
                    return null;
                }
            }
            stringBuilder.AppendFormat("app={0}&clientver={1}&sid={2}&{3}&initobj={4}", new object[]
            {
                appId,
                this.photonPeer.ClientVersion,
                this.photonPeer.ClientSdkIdShifted,
                this.IsIpv6 ? "IPv6" : string.Empty,
                empty
            });
            return stringBuilder.ToString();
        }

        internal byte[] PrepareConnectData(string serverAddress, string appID, object custom)
        {
            bool flag = this.PhotonSocket == null || !this.PhotonSocket.Connected;
            if (flag)
            {
                this.EnqueueDebugReturn(DebugLevel.Warning, "The peer attempts to prepare an Init-Request but the socket is not connected!?");
            }
            bool flag2 = custom == null;
            byte[] result;
            if (flag2)
            {
                byte[] array = new byte[41];
                byte[] clientVersion = Version.clientVersion;
                array[0] = 243;
                array[1] = 0;
                array[2] = this.serializationProtocol.VersionBytes[0];
                array[3] = this.serializationProtocol.VersionBytes[1];
                array[4] = this.photonPeer.ClientSdkIdShifted;
                array[5] = (byte)((byte)(clientVersion[0] << 4) | (byte)clientVersion[1]);
                array[6] = clientVersion[2];
                array[7] = clientVersion[3];
                array[8] = 0;
                bool flag3 = string.IsNullOrEmpty(appID);
                if (flag3)
                {
                    appID = "LoadBalancing";
                }
                for (int i = 0; i < 32; i++)
                {
                    array[i + 9] = (byte)((i < appID.Length) ? ((byte)appID[i]) : 0);
                }
                bool isIpv = this.IsIpv6;
                if (isIpv)
                {
                    byte[] array2 = array;
                    int num = 5;
                    array2[num] |= 128;
                }
                else
                {
                    byte[] array3 = array;
                    int num2 = 5;
                    array3[num2] &= 127;
                }
                result = array;
            }
            else
            {
                bool flag4 = custom != null;
                if (flag4)
                {
                    Dictionary<string, string> dictionary = new Dictionary<string, string>();
                    dictionary["init"] = null;
                    dictionary["app"] = appID;
                    dictionary["clientversion"] = this.photonPeer.ClientVersion;
                    dictionary["protocol"] = this.serializationProtocol.protocolType;
                    dictionary["sid"] = this.photonPeer.ClientSdkIdShifted.ToString();
                    byte[] array4 = null;
                    int num3 = 0;
                    bool flag5 = custom != null;
                    if (flag5)
                    {
                        array4 = this.serializationProtocol.Serialize(custom);
                        num3 += array4.Length;
                    }
                    string text = this.GetHttpKeyValueString(dictionary);
                    bool isIpv2 = this.IsIpv6;
                    if (isIpv2)
                    {
                        text += "&IPv6";
                    }
                    string text2 = string.Format("POST /?{0} HTTP/1.1\r\nHost: {1}\r\nContent-Length: {2}\r\n\r\n", text, serverAddress, num3);
                    byte[] array5 = new byte[text2.Length + num3];
                    bool flag6 = array4 != null;
                    if (flag6)
                    {
                        Buffer.BlockCopy(array4, 0, array5, text2.Length, array4.Length);
                    }
                    Buffer.BlockCopy(Encoding.UTF8.GetBytes(text2), 0, array5, 0, text2.Length);
                    result = array5;
                }
                else
                {
                    result = null;
                }
            }
            return result;
        }

        internal abstract void ReceiveIncomingCommands(byte[] inBuff, int dataLength);

        internal void ReceiveNetworkSimulated(byte[] dataReceived)
        {
            bool flag = !this.networkSimulationSettings.IsSimulationEnabled;
            if (flag)
            {
                throw new NotImplementedException("ReceiveNetworkSimulated was called, despite NetworkSimulationSettings.IsSimulationEnabled == false.");
            }
            bool flag2 = this.usedTransportProtocol == ConnectionProtocol.Udp && this.networkSimulationSettings.IncomingLossPercentage > 0 && this.lagRandomizer.Next(101) < this.networkSimulationSettings.IncomingLossPercentage;
            if (flag2)
            {
                NetworkSimulationSet networkSimulationSet = this.networkSimulationSettings;
                int lostPackagesIn = networkSimulationSet.LostPackagesIn;
                networkSimulationSet.LostPackagesIn = lostPackagesIn + 1;
            }
            else
            {
                int num = (this.networkSimulationSettings.IncomingJitter <= 0) ? 0 : (this.lagRandomizer.Next(this.networkSimulationSettings.IncomingJitter * 2) - this.networkSimulationSettings.IncomingJitter);
                int num2 = this.networkSimulationSettings.IncomingLag + num;
                int num3 = SupportClass.GetTickCount() + num2;
                SimulationItem value = new SimulationItem
                {
                    DelayedData = dataReceived,
                    TimeToExecute = num3,
                    Delay = num2
                };
                LinkedList<SimulationItem> netSimListIncoming = this.NetSimListIncoming;
                lock (netSimListIncoming)
                {
                    bool flag3 = this.NetSimListIncoming.Count == 0 || this.usedTransportProtocol == ConnectionProtocol.Tcp;
                    if (flag3)
                    {
                        this.NetSimListIncoming.AddLast(value);
                    }
                    else
                    {
                        LinkedListNode<SimulationItem> linkedListNode = this.NetSimListIncoming.First;
                        while (linkedListNode != null && linkedListNode.Value.TimeToExecute < num3)
                        {
                            linkedListNode = linkedListNode.Next;
                        }
                        bool flag4 = linkedListNode == null;
                        if (flag4)
                        {
                            this.NetSimListIncoming.AddLast(value);
                        }
                        else
                        {
                            this.NetSimListIncoming.AddBefore(linkedListNode, value);
                        }
                    }
                }
            }
        }

        internal virtual bool SendAcksOnly()
        {
            return false;
        }

        internal void SendNetworkSimulated(byte[] dataToSend)
        {
            bool flag = !this.NetworkSimulationSettings.IsSimulationEnabled;
            if (flag)
            {
                throw new NotImplementedException("SendNetworkSimulated was called, despite NetworkSimulationSettings.IsSimulationEnabled == false.");
            }
            bool flag2 = this.usedTransportProtocol == ConnectionProtocol.Udp && this.NetworkSimulationSettings.OutgoingLossPercentage > 0 && this.lagRandomizer.Next(101) < this.NetworkSimulationSettings.OutgoingLossPercentage;
            if (flag2)
            {
                NetworkSimulationSet networkSimulationSet = this.networkSimulationSettings;
                int lostPackagesOut = networkSimulationSet.LostPackagesOut;
                networkSimulationSet.LostPackagesOut = lostPackagesOut + 1;
            }
            else
            {
                int num = (this.networkSimulationSettings.OutgoingJitter <= 0) ? 0 : (this.lagRandomizer.Next(this.networkSimulationSettings.OutgoingJitter * 2) - this.networkSimulationSettings.OutgoingJitter);
                int num2 = this.networkSimulationSettings.OutgoingLag + num;
                int num3 = SupportClass.GetTickCount() + num2;
                SimulationItem value = new SimulationItem
                {
                    DelayedData = dataToSend,
                    TimeToExecute = num3,
                    Delay = num2
                };
                LinkedList<SimulationItem> netSimListOutgoing = this.NetSimListOutgoing;
                lock (netSimListOutgoing)
                {
                    bool flag3 = this.NetSimListOutgoing.Count == 0 || this.usedTransportProtocol == ConnectionProtocol.Tcp;
                    if (flag3)
                    {
                        this.NetSimListOutgoing.AddLast(value);
                    }
                    else
                    {
                        LinkedListNode<SimulationItem> linkedListNode = this.NetSimListOutgoing.First;
                        while (linkedListNode != null && linkedListNode.Value.TimeToExecute < num3)
                        {
                            linkedListNode = linkedListNode.Next;
                        }
                        bool flag4 = linkedListNode == null;
                        if (flag4)
                        {
                            this.NetSimListOutgoing.AddLast(value);
                        }
                        else
                        {
                            this.NetSimListOutgoing.AddBefore(linkedListNode, value);
                        }
                    }
                }
            }
        }

        internal abstract bool SendOutgoingCommands();

        internal StreamBuffer SerializeMessageToMessage(object message, bool encrypt, byte[] messageHeader, bool writeLength = true)
        {
            StreamBuffer streamBuffer = PeerBase.MessageBufferPoolGet();
            streamBuffer.SetLength(0L);
            bool flag = !encrypt;
            if (flag)
            {
                streamBuffer.Write(messageHeader, 0, messageHeader.Length);
            }
            bool flag2 = message is byte[];
            bool flag3 = flag2;
            if (flag3)
            {
                byte[] array = message as byte[];
                streamBuffer.Write(array, 0, array.Length);
            }
            else
            {
                this.serializationProtocol.SerializeMessage(streamBuffer, message);
            }
            if (encrypt)
            {
                byte[] array2 = this.CryptoProvider.Encrypt(streamBuffer.GetBuffer(), 0, streamBuffer.Length);
                streamBuffer.SetLength(0L);
                streamBuffer.Write(messageHeader, 0, messageHeader.Length);
                streamBuffer.Write(array2, 0, array2.Length);
            }
            byte[] buffer = streamBuffer.GetBuffer();
            buffer[messageHeader.Length - 1] = (byte)((message is byte[]) ? 9 : 8);
            if (encrypt)
            {
                buffer[messageHeader.Length - 1] = (byte)(buffer[messageHeader.Length - 1] | 128);
            }
            if (writeLength)
            {
                int num = 1;
                Protocol.Serialize(streamBuffer.Length, buffer, ref num);
            }
            return streamBuffer;
        }

        internal abstract StreamBuffer SerializeOperationToMessage(byte opCode, Dictionary<byte, object> parameters, EgMessageType messageType, bool encrypt);

        internal abstract void StopConnection();

        internal void UpdateRoundTripTimeAndVariance(int lastRoundtripTime)
        {
            bool flag = lastRoundtripTime < 0;
            if (!flag)
            {
                this.roundTripTimeVariance -= this.roundTripTimeVariance / 4;
                bool flag2 = lastRoundtripTime >= this.roundTripTime;
                if (flag2)
                {
                    this.roundTripTime += (lastRoundtripTime - this.roundTripTime) / 8;
                    this.roundTripTimeVariance += (lastRoundtripTime - this.roundTripTime) / 4;
                }
                else
                {
                    this.roundTripTime += (lastRoundtripTime - this.roundTripTime) / 8;
                    this.roundTripTimeVariance -= (lastRoundtripTime - this.roundTripTime) / 4;
                }
                bool flag3 = this.roundTripTime < this.lowestRoundTripTime;
                if (flag3)
                {
                    this.lowestRoundTripTime = this.roundTripTime;
                }
                bool flag4 = this.roundTripTimeVariance > this.highestRoundTripTimeVariance;
                if (flag4)
                {
                    this.highestRoundTripTimeVariance = this.roundTripTimeVariance;
                }
            }
        }

        public static StreamBuffer MessageBufferPoolGet()
        {
            Queue<StreamBuffer> messageBufferPool = PeerBase.MessageBufferPool;
            StreamBuffer result;
            lock (messageBufferPool)
            {
                bool flag = PeerBase.MessageBufferPool.Count > 0;
                if (flag)
                {
                    result = PeerBase.MessageBufferPool.Dequeue();
                }
                else
                {
                    result = new StreamBuffer(75);
                }
            }
            return result;
        }

        public static void MessageBufferPoolPut(StreamBuffer buff)
        {
            Queue<StreamBuffer> messageBufferPool = PeerBase.MessageBufferPool;
            lock (messageBufferPool)
            {
                buff.Position = 0;
                buff.SetLength(0L);
                PeerBase.MessageBufferPool.Enqueue(buff);
            }
        }

        public string CommandLogToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            int num = (this.usedTransportProtocol != ConnectionProtocol.Udp) ? 0 : ((EnetPeer)this).reliableCommandsRepeated;
            stringBuilder.AppendFormat("PeerId: {0} Now: {1} Server: {2} State: {3} Total Resends: {4} Received {5}ms ago.\n", new object[]
            {
                this.PeerID,
                this.timeInt,
                this.ServerAddress,
                this.peerConnectionState,
                num,
                SupportClass.GetTickCount() - this.timestampOfLastReceive
            });
            bool flag = this.CommandLog == null;
            string result;
            if (flag)
            {
                result = stringBuilder.ToString();
            }
            else
            {
                foreach (CmdLogItem cmdLogItem in this.CommandLog)
                {
                    stringBuilder.AppendLine(cmdLogItem.ToString());
                }
                stringBuilder.AppendLine("Received Reliable Log: ");
                foreach (CmdLogItem cmdLogItem2 in this.InReliableLog)
                {
                    stringBuilder.AppendLine(cmdLogItem2.ToString());
                }
                result = stringBuilder.ToString();
            }
            return result;
        }

        public abstract void OnConnect();
    }
}