using System;
using System.Collections.Generic;

namespace ExitGames.Client.Photon
{
    internal class TPeer : PeerBase
    {
        private Queue<byte[]> incomingList = new Queue<byte[]>(32);
        private int lastPingResult;
        private byte[] pingRequest;
        protected internal bool DoFraming;
        internal const int MSG_HEADER_BYTES = 2;
        internal const int TCP_HEADER_BYTES = 7;

        internal static readonly byte[] tcpFramedMessageHead = new byte[]
        {
            251,
            0,
            0,
            0,
            0,
            0,
            0,
            243,
            2
        };

        internal static readonly byte[] tcpMsgHead = new byte[]
        {
            243,
            2
        };

        internal byte[] messageHeader;
        internal List<StreamBuffer> outgoingStream;
        public const int ALL_HEADER_BYTES = 9;

        internal TPeer()
        {
            byte[] array = new byte[5];
            array[0] = 240;
            this.pingRequest = array;
            this.DoFraming = true;
            this.TrafficPackageHeaderSize = 0;
        }

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

        private void EnqueueInit(byte[] data)
        {
            bool flag = !this.DoFraming;
            if (!flag)
            {
                StreamBuffer streamBuffer = new StreamBuffer(data.Length + 32);
                byte[] array = new byte[]
                {
                    251,
                    0,
                    0,
                    0,
                    0,
                    0,
                    1
                };
                int num = 1;
                Protocol.Serialize(data.Length + array.Length, array, ref num);
                streamBuffer.Write(array, 0, array.Length);
                streamBuffer.Write(data, 0, data.Length);
                bool trafficStatsEnabled = base.TrafficStatsEnabled;
                if (trafficStatsEnabled)
                {
                    TrafficStats trafficStatsOutgoing = base.TrafficStatsOutgoing;
                    int num2 = trafficStatsOutgoing.TotalPacketCount;
                    trafficStatsOutgoing.TotalPacketCount = num2 + 1;
                    TrafficStats trafficStatsOutgoing2 = base.TrafficStatsOutgoing;
                    num2 = trafficStatsOutgoing2.TotalCommandsInPackets;
                    trafficStatsOutgoing2.TotalCommandsInPackets = num2 + 1;
                    base.TrafficStatsOutgoing.CountControlCommand(streamBuffer.Length);
                }
                this.EnqueueMessageAsPayload(DeliveryMode.Reliable, streamBuffer, 0);
            }
        }

        private void ReadPingResult(byte[] inbuff)
        {
            int num = 0;
            int num2 = 0;
            int num3 = 1;
            Protocol.Deserialize(out num, inbuff, ref num3);
            Protocol.Deserialize(out num2, inbuff, ref num3);
            this.lastRoundTripTime = SupportClass.GetTickCount() - num2;
            bool flag = !this.serverTimeOffsetIsAvailable;
            if (flag)
            {
                this.roundTripTime = this.lastRoundTripTime;
            }
            base.UpdateRoundTripTimeAndVariance(this.lastRoundTripTime);
            bool flag2 = !this.serverTimeOffsetIsAvailable;
            if (flag2)
            {
                this.serverTimeOffset = num + (this.lastRoundTripTime >> 1) - SupportClass.GetTickCount();
                this.serverTimeOffsetIsAvailable = true;
            }
        }

        protected internal void ReadPingResult(OperationResponse operationResponse)
        {
            int num = (int)operationResponse.Parameters[2];
            int num2 = (int)operationResponse.Parameters[1];
            this.lastRoundTripTime = SupportClass.GetTickCount() - num2;
            bool flag = !this.serverTimeOffsetIsAvailable;
            if (flag)
            {
                this.roundTripTime = this.lastRoundTripTime;
            }
            base.UpdateRoundTripTimeAndVariance(this.lastRoundTripTime);
            bool flag2 = !this.serverTimeOffsetIsAvailable;
            if (flag2)
            {
                this.serverTimeOffset = num + (this.lastRoundTripTime >> 1) - SupportClass.GetTickCount();
                this.serverTimeOffsetIsAvailable = true;
            }
        }

        internal override bool Connect(string serverAddress, string appID, object customData = null)
        {
            bool flag = this.peerConnectionState > ConnectionStateValue.Disconnected;
            bool result;
            if (flag)
            {
                base.Listener.DebugReturn(DebugLevel.Warning, "Connect() can't be called if peer is not Disconnected. Not connecting.");
                result = false;
            }
            else
            {
                bool flag2 = base.debugOut >= DebugLevel.All;
                if (flag2)
                {
                    base.Listener.DebugReturn(DebugLevel.All, "Connect()");
                }
                base.ServerAddress = serverAddress;
                this.InitPeerBase();
                this.outgoingStream = new List<StreamBuffer>();
                bool flag3 = this.usedTransportProtocol == ConnectionProtocol.WebSocket || this.usedTransportProtocol == ConnectionProtocol.WebSocketSecure;
                if (flag3)
                {
                    serverAddress = base.PepareWebSocketUrl(serverAddress, appID, customData);
                }
                bool flag4 = base.SocketImplementation != null;
                if (flag4)
                {
                    this.PhotonSocket = (IPhotonSocket)Activator.CreateInstance(base.SocketImplementation, new object[]
                    {
                        this
                    });
                }
                else
                {
                    this.PhotonSocket = new SocketTcp(this);
                }
                bool flag5 = this.PhotonSocket == null;
                if (flag5)
                {
                    base.Listener.DebugReturn(DebugLevel.Error, "Connect() failed, because SocketImplementation or socket was null. Set PhotonPeer.SocketImplementation before Connect(). SocketImplementation: " + base.SocketImplementation);
                    result = false;
                }
                else
                {
                    this.messageHeader = (this.DoFraming ? TPeer.tcpFramedMessageHead : TPeer.tcpMsgHead);
                    bool flag6 = this.PhotonSocket.Connect();
                    if (flag6)
                    {
                        this.peerConnectionState = ConnectionStateValue.Connecting;
                        result = true;
                    }
                    else
                    {
                        this.peerConnectionState = ConnectionStateValue.Disconnected;
                        result = false;
                    }
                }
            }
            return result;
        }

        internal override void Disconnect()
        {
            bool flag = this.peerConnectionState == ConnectionStateValue.Disconnected || this.peerConnectionState == ConnectionStateValue.Disconnecting;
            if (!flag)
            {
                bool flag2 = base.debugOut >= DebugLevel.All;
                if (flag2)
                {
                    base.Listener.DebugReturn(DebugLevel.All, "TPeer.Disconnect()");
                }
                this.StopConnection();
            }
        }

        internal override bool DispatchIncomingCommands()
        {
            bool flag = this.peerConnectionState == ConnectionStateValue.Connected && SupportClass.GetTickCount() - this.timestampOfLastReceive > base.DisconnectTimeout;
            if (flag)
            {
                base.EnqueueStatusCallback(StatusCode.TimeoutDisconnect);
                base.EnqueueActionForDispatch(new PeerBase.MyAction(this.Disconnect));
            }
            for (; ; )
            {
                Queue<PeerBase.MyAction> actionQueue = this.ActionQueue;
                PeerBase.MyAction myAction;
                lock (actionQueue)
                {
                    bool flag2 = this.ActionQueue.Count <= 0;
                    if (flag2)
                    {
                        break;
                    }
                    myAction = this.ActionQueue.Dequeue();
                }
                myAction();
            }
            Queue<byte[]> obj = this.incomingList;
            byte[] array;
            lock (obj)
            {
                bool flag3 = this.incomingList.Count <= 0;
                if (flag3)
                {
                    return false;
                }
                array = this.incomingList.Dequeue();
            }
            this.ByteCountCurrentDispatch = array.Length + 3;
            return this.DeserializeMessageAndCallback(new StreamBuffer(array));
        }

        internal override bool EnqueueMessage(object msg, SendOptions sendOptions)
        {
            bool flag = this.peerConnectionState != ConnectionStateValue.Connected;
            bool result;
            if (flag)
            {
                bool flag2 = base.debugOut >= DebugLevel.Error;
                if (flag2)
                {
                    base.Listener.DebugReturn(DebugLevel.Error, "Cannot send message! Not connected. PeerState: " + this.peerConnectionState);
                }
                base.Listener.OnStatusChanged(StatusCode.SendError);
                result = false;
            }
            else
            {
                byte channel = sendOptions.Channel;
                bool flag3 = channel >= base.ChannelCount;
                if (flag3)
                {
                    bool flag4 = base.debugOut >= DebugLevel.Error;
                    if (flag4)
                    {
                        base.Listener.DebugReturn(DebugLevel.Error, string.Concat(new object[]
                        {
                            "Cannot send op: Selected channel (",
                            channel,
                            ")>= channelCount (",
                            base.ChannelCount,
                            ")."
                        }));
                    }
                    base.Listener.OnStatusChanged(StatusCode.SendError);
                    result = false;
                }
                else
                {
                    StreamBuffer opMessage = base.SerializeMessageToMessage(msg, sendOptions.Encrypt, this.messageHeader, true);
                    result = this.EnqueueMessageAsPayload(sendOptions.DeliveryMode, opMessage, channel);
                }
            }
            return result;
        }

        internal bool EnqueueMessageAsPayload(DeliveryMode deliveryMode, StreamBuffer opMessage, byte channelId)
        {
            bool flag = opMessage == null;
            bool result;
            if (flag)
            {
                result = false;
            }
            else
            {
                bool doFraming = this.DoFraming;
                if (doFraming)
                {
                    byte[] buffer = opMessage.GetBuffer();
                    buffer[5] = channelId;
                    switch (deliveryMode)
                    {
                        case DeliveryMode.Unreliable:
                            buffer[6] = 0;
                            break;

                        case DeliveryMode.Reliable:
                            buffer[6] = 1;
                            break;

                        case DeliveryMode.UnreliableUnsequenced:
                            buffer[6] = 2;
                            break;

                        case DeliveryMode.ReliableUnsequenced:
                            buffer[6] = 3;
                            break;

                        default:
                            throw new ArgumentOutOfRangeException("deliveryMode", deliveryMode, null);
                    }
                }
                List<StreamBuffer> obj = this.outgoingStream;
                lock (obj)
                {
                    this.outgoingStream.Add(opMessage);
                    this.outgoingCommandsInStream++;
                }
                int length = opMessage.Length;
                this.ByteCountLastOperation = length;
                bool trafficStatsEnabled = base.TrafficStatsEnabled;
                if (trafficStatsEnabled)
                {
                    if (deliveryMode != DeliveryMode.Unreliable)
                    {
                        if (deliveryMode != DeliveryMode.Reliable)
                        {
                            throw new ArgumentOutOfRangeException("deliveryMode", deliveryMode, null);
                        }
                        base.TrafficStatsOutgoing.CountReliableOpCommand(length);
                    }
                    else
                    {
                        base.TrafficStatsOutgoing.CountUnreliableOpCommand(length);
                    }
                    base.TrafficStatsGameLevel.CountOperation(length);
                }
                result = true;
            }
            return result;
        }

        internal override bool EnqueueOperation(Dictionary<byte, object> parameters, byte opCode, SendOptions sendParams, EgMessageType messageType)
        {
            bool flag = this.peerConnectionState != ConnectionStateValue.Connected;
            bool result;
            if (flag)
            {
                bool flag2 = base.debugOut >= DebugLevel.Error;
                if (flag2)
                {
                    base.Listener.DebugReturn(DebugLevel.Error, string.Concat(new object[]
                    {
                        "Cannot send op: ",
                        opCode,
                        "! Not connected. PeerState: ",
                        this.peerConnectionState
                    }));
                }
                base.Listener.OnStatusChanged(StatusCode.SendError);
                result = false;
            }
            else
            {
                bool flag3 = sendParams.Channel >= base.ChannelCount;
                if (flag3)
                {
                    bool flag4 = base.debugOut >= DebugLevel.Error;
                    if (flag4)
                    {
                        base.Listener.DebugReturn(DebugLevel.Error, string.Concat(new object[]
                        {
                            "Cannot send op: Selected channel (",
                            sendParams.Channel,
                            ")>= channelCount (",
                            base.ChannelCount,
                            ")."
                        }));
                    }
                    base.Listener.OnStatusChanged(StatusCode.SendError);
                    result = false;
                }
                else
                {
                    StreamBuffer opMessage = this.SerializeOperationToMessage(opCode, parameters, messageType, sendParams.Encrypt);
                    result = this.EnqueueMessageAsPayload(sendParams.DeliveryMode, opMessage, sendParams.Channel);
                }
            }
            return result;
        }

        internal override void FetchServerTimestamp()
        {
            bool flag = this.peerConnectionState != ConnectionStateValue.Connected;
            if (flag)
            {
                bool flag2 = base.debugOut >= DebugLevel.Info;
                if (flag2)
                {
                    base.Listener.DebugReturn(DebugLevel.Info, "FetchServerTimestamp() was skipped, as the client is not connected. Current ConnectionState: " + this.peerConnectionState);
                }
                base.Listener.OnStatusChanged(StatusCode.SendError);
            }
            else
            {
                this.SendPing();
                this.serverTimeOffsetIsAvailable = false;
            }
        }

        internal override void InitPeerBase()
        {
            base.InitPeerBase();
            this.incomingList = new Queue<byte[]>(32);
            this.timestampOfLastReceive = SupportClass.GetTickCount();
        }

        internal override void ReceiveIncomingCommands(byte[] inbuff, int dataLength)
        {
            bool flag = inbuff == null;
            if (flag)
            {
                bool flag2 = base.debugOut >= DebugLevel.Error;
                if (flag2)
                {
                    base.EnqueueDebugReturn(DebugLevel.Error, "checkAndQueueIncomingCommands() inBuff: null");
                }
            }
            else
            {
                this.timestampOfLastReceive = SupportClass.GetTickCount();
                this.timeInt = SupportClass.GetTickCount() - this.timeBase;
                this.bytesIn += (long)(dataLength + 7);
                bool trafficStatsEnabled = base.TrafficStatsEnabled;
                if (trafficStatsEnabled)
                {
                    TrafficStats trafficStatsIncoming = base.TrafficStatsIncoming;
                    int num = trafficStatsIncoming.TotalPacketCount;
                    trafficStatsIncoming.TotalPacketCount = num + 1;
                    TrafficStats trafficStatsIncoming2 = base.TrafficStatsIncoming;
                    num = trafficStatsIncoming2.TotalCommandsInPackets;
                    trafficStatsIncoming2.TotalCommandsInPackets = num + 1;
                }
                bool flag3 = inbuff[0] == 243 || inbuff[0] == 244;
                if (flag3)
                {
                    byte[] array = new byte[dataLength];
                    Buffer.BlockCopy(inbuff, 0, array, 0, dataLength);
                    Queue<byte[]> obj = this.incomingList;
                    lock (obj)
                    {
                        this.incomingList.Enqueue(array);
                    }
                }
                else
                {
                    bool flag4 = inbuff[0] == 240;
                    if (flag4)
                    {
                        base.TrafficStatsIncoming.CountControlCommand(inbuff.Length);
                        this.ReadPingResult(inbuff);
                    }
                    else
                    {
                        bool flag5 = base.debugOut >= DebugLevel.Error;
                        if (flag5)
                        {
                            base.EnqueueDebugReturn(DebugLevel.Error, "receiveIncomingCommands() MagicNumber should be 0xF0, 0xF3 or 0xF4. Is: " + inbuff[0]);
                        }
                    }
                }
            }
        }

        internal override bool SendAcksOnly()
        {
            bool flag = this.PhotonSocket == null || !this.PhotonSocket.Connected;
            bool result;
            if (flag)
            {
                result = false;
            }
            else
            {
                this.timeInt = SupportClass.GetTickCount() - this.timeBase;
                bool flag2 = this.peerConnectionState == ConnectionStateValue.Connected && SupportClass.GetTickCount() - this.lastPingResult > base.timePingInterval;
                if (flag2)
                {
                    this.SendPing();
                }
                result = false;
            }
            return result;
        }

        internal void SendData(byte[] data, int length)
        {
            try
            {
                this.bytesOut += (long)length;
                bool trafficStatsEnabled = base.TrafficStatsEnabled;
                if (trafficStatsEnabled)
                {
                    TrafficStats trafficStatsOutgoing = base.TrafficStatsOutgoing;
                    int totalPacketCount = trafficStatsOutgoing.TotalPacketCount;
                    trafficStatsOutgoing.TotalPacketCount = totalPacketCount + 1;
                    base.TrafficStatsOutgoing.TotalCommandsInPackets += this.outgoingCommandsInStream;
                }
                bool isSimulationEnabled = base.NetworkSimulationSettings.IsSimulationEnabled;
                if (isSimulationEnabled)
                {
                    byte[] array = new byte[length];
                    Buffer.BlockCopy(data, 0, array, 0, length);
                    base.SendNetworkSimulated(array);
                }
                else
                {
                    this.PhotonSocket.Send(data, length);
                }
            }
            catch (Exception ex)
            {
                bool flag = base.debugOut >= DebugLevel.Error;
                if (flag)
                {
                    base.Listener.DebugReturn(DebugLevel.Error, ex.ToString());
                }
                SupportClass.WriteStackTrace(ex);
            }
        }

        internal override bool SendOutgoingCommands()
        {
            bool flag = this.peerConnectionState == ConnectionStateValue.Disconnected;
            bool result;
            if (flag)
            {
                result = false;
            }
            else
            {
                bool flag2 = !this.PhotonSocket.Connected;
                if (flag2)
                {
                    result = false;
                }
                else
                {
                    this.timeInt = SupportClass.GetTickCount() - this.timeBase;
                    this.timeLastSendOutgoing = this.timeInt;
                    bool flag3 = this.peerConnectionState == ConnectionStateValue.Connected && Math.Abs(SupportClass.GetTickCount() - this.lastPingResult) > base.timePingInterval;
                    if (flag3)
                    {
                        this.SendPing();
                    }
                    List<StreamBuffer> obj = this.outgoingStream;
                    lock (obj)
                    {
                        for (int i = 0; i < this.outgoingStream.Count; i++)
                        {
                            StreamBuffer streamBuffer = this.outgoingStream[i];
                            this.SendData(streamBuffer.GetBuffer(), streamBuffer.Length);
                            PeerBase.MessageBufferPoolPut(streamBuffer);
                        }
                        this.outgoingStream.Clear();
                        this.outgoingCommandsInStream = 0;
                    }
                    result = false;
                }
            }
            return result;
        }

        internal void SendPing()
        {
            this.lastPingResult = SupportClass.GetTickCount();
            bool flag = !this.DoFraming;
            if (flag)
            {
                int tickCount = SupportClass.GetTickCount();
                SendOptions sendParams = new SendOptions
                {
                    DeliveryMode = DeliveryMode.Reliable
                };
                this.EnqueueOperation(new Dictionary<byte, object>
                {
                    {
                        1,
                        tickCount
                    }
                }, PhotonCodes.Ping, sendParams, EgMessageType.InternalOperationRequest);
            }
            else
            {
                int num = 1;
                Protocol.Serialize(SupportClass.GetTickCount(), this.pingRequest, ref num);
                bool trafficStatsEnabled = base.TrafficStatsEnabled;
                if (trafficStatsEnabled)
                {
                    base.TrafficStatsOutgoing.CountControlCommand(this.pingRequest.Length);
                }
                this.SendData(this.pingRequest, this.pingRequest.Length);
            }
        }

        internal override StreamBuffer SerializeOperationToMessage(byte opCode, Dictionary<byte, object> parameters, EgMessageType messageType, bool encrypt)
        {
            StreamBuffer streamBuffer = PeerBase.MessageBufferPoolGet();
            streamBuffer.SetLength(0L);
            bool flag = !encrypt;
            if (flag)
            {
                streamBuffer.Write(this.messageHeader, 0, this.messageHeader.Length);
            }
            this.serializationProtocol.SerializeOperationRequest(streamBuffer, opCode, parameters, false);
            if (encrypt)
            {
                byte[] array = this.CryptoProvider.Encrypt(streamBuffer.GetBuffer(), 0, streamBuffer.Length);
                streamBuffer.SetLength(0L);
                streamBuffer.Write(this.messageHeader, 0, this.messageHeader.Length);
                streamBuffer.Write(array, 0, array.Length);
            }
            byte[] buffer = streamBuffer.GetBuffer();
            bool flag2 = messageType != EgMessageType.Operation;
            if (flag2)
            {
                buffer[this.messageHeader.Length - 1] = (byte)messageType;
            }
            if (encrypt)
            {
                buffer[this.messageHeader.Length - 1] = (byte)(buffer[this.messageHeader.Length - 1] | (byte)128);
            }
            bool doFraming = this.DoFraming;
            if (doFraming)
            {
                int num = 1;
                Protocol.Serialize(streamBuffer.Length, buffer, ref num);
            }
            return streamBuffer;
        }

        internal override void StopConnection()
        {
            this.peerConnectionState = ConnectionStateValue.Disconnecting;
            bool flag = this.PhotonSocket != null;
            if (flag)
            {
                this.PhotonSocket.Disconnect();
            }
            Queue<byte[]> obj = this.incomingList;
            lock (obj)
            {
                this.incomingList.Clear();
            }
            this.peerConnectionState = ConnectionStateValue.Disconnected;
            base.EnqueueStatusCallback(StatusCode.Disconnect);
        }

        public override void OnConnect()
        {
            this.lastPingResult = SupportClass.GetTickCount();
            byte[] data = base.PrepareConnectData(base.ServerAddress, this.AppId, this.CustomInitData);
            this.EnqueueInit(data);
            this.SendOutgoingCommands();
        }
    }
}