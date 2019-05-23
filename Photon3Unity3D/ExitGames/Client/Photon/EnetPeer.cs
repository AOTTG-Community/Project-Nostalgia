using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ExitGames.Client.Photon
{
    internal class EnetPeer : PeerBase
    {
        private const byte ControlChannelNumber = 255;
        private const int CRC_LENGTH = 4;

        private const int EncryptedDataGramHeaderSize = 7;
        private const int EncryptedHeaderSize = 5;
        private static readonly int BLOCK_SIZE = 16;
        private static readonly int HMAC_SIZE = 32;
        private static readonly int IV_SIZE = 16;
        private byte[] bufferForEncryption;
        internal EnetChannel[] channelArray = new EnetChannel[0];
        private Queue<NCommand> CommandQueue = new Queue<NCommand>();
        private Queue<int> commandsToRemove = new Queue<int>();
        private Queue<NCommand> commandsToResend = new Queue<NCommand>();
        private int fragmentLength = 0;
        private int fragmentLengthMtuValue = 0;
        private StreamBuffer outgoingAcknowledgementsPool;
        private List<NCommand> sentReliableCommands = new List<NCommand>();
        private byte[] udpBuffer;
        private int udpBufferIndex;
        private int udpBufferLength;
        private byte udpCommandCount;
        protected bool datagramEncryptedConnection;
        protected internal const short PeerIdForConnect = -1;
        protected internal const short PeerIdForConnectTrace = -2;
        internal const int UnsequencedWindowSize = 128;

        internal static readonly byte[] udpHeader0xF3 = new byte[]
        {
            243,
            2
        };

        internal static readonly byte[] messageHeader = EnetPeer.udpHeader0xF3;


        internal readonly int[] unsequencedWindow = new int[4];

        internal int challenge;
        internal int commandBufferSize = 100;
        internal int incomingUnsequencedGroupNumber;
        internal int outgoingUnsequencedGroupNumber;
        internal int reliableCommandsRepeated;

        internal int reliableCommandsSent;

        internal int serverSentTime;

        internal EnetPeer()
        {
            this.TrafficPackageHeaderSize = 12;
        }

        internal override int QueuedIncomingCommandsCount
        {
            get
            {
                int num = 0;
                EnetChannel[] obj = this.channelArray;
                lock (obj)
                {
                    for (int i = 0; i < this.channelArray.Length; i++)
                    {
                        EnetChannel enetChannel = this.channelArray[i];
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
                EnetChannel[] obj = this.channelArray;
                lock (obj)
                {
                    for (int i = 0; i < this.channelArray.Length; i++)
                    {
                        EnetChannel enetChannel = this.channelArray[i];
                        num += enetChannel.outgoingReliableCommandsList.Count;
                        num += enetChannel.outgoingUnreliableCommandsList.Count;
                    }
                }
                return num;
            }
        }

        private bool AreReliableCommandsInTransit()
        {
            EnetChannel[] obj = this.channelArray;
            lock (obj)
            {
                for (int i = 0; i < this.channelArray.Length; i++)
                {
                    EnetChannel enetChannel = this.channelArray[i];
                    bool flag = enetChannel.outgoingReliableCommandsList.Count > 0;
                    if (flag)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private int CalculateBufferLen()
        {
            int num = base.mtu;
            bool flag = this.datagramEncryptedConnection;
            int result;
            if (flag)
            {
                num = num - 7 - EnetPeer.HMAC_SIZE - EnetPeer.IV_SIZE;
                num = num / EnetPeer.BLOCK_SIZE * EnetPeer.BLOCK_SIZE;
                num--;
                result = num;
            }
            else
            {
                result = num;
            }
            return result;
        }

        private int CalculateInitialOffset()
        {
            bool flag = this.datagramEncryptedConnection;
            int result;
            if (flag)
            {
                result = 5;
            }
            else
            {
                int num = 12;
                bool crcEnabled = this.photonPeer.CrcEnabled;
                if (crcEnabled)
                {
                    num += 4;
                }
                result = num;
            }
            return result;
        }

        internal EnetChannel GetChannel(byte channelNumber)
        {
            return (channelNumber == byte.MaxValue) ? this.channelArray[this.channelArray.Length - 1] : this.channelArray[(int)channelNumber];
        }

        private int GetFragmentLength()
        {
            bool flag = this.fragmentLength == 0 || base.mtu != this.fragmentLengthMtuValue;
            if (flag)
            {
                int num = base.mtu;
                bool flag2 = this.datagramEncryptedConnection;
                if (flag2)
                {
                    num = num - 7 - EnetPeer.HMAC_SIZE - EnetPeer.IV_SIZE;
                    num = num / EnetPeer.BLOCK_SIZE * EnetPeer.BLOCK_SIZE;
                    return num - 5 - 36;
                }
                this.fragmentLength = num - 12 - 36;
                this.fragmentLengthMtuValue = base.mtu;
            }
            return this.fragmentLength;
        }

        private void SendDataEncrypted(byte[] data, int length)
        {
            bool flag = this.bufferForEncryption == null || this.bufferForEncryption.Length != base.mtu;
            if (flag)
            {
                this.bufferForEncryption = new byte[base.mtu];
            }
            byte[] array = this.bufferForEncryption;
            int num = 0;
            Protocol.Serialize(this.peerID, array, ref num);
            array[2] = 1;
            num++;
            Protocol.Serialize(this.challenge, array, ref num);
            data[0] = this.udpCommandCount;
            int num2 = 1;
            Protocol.Serialize(this.timeInt, data, ref num2);
            this.photonPeer.encryptor.Encrypt(data, length, array, ref num);
            Buffer.BlockCopy(this.photonPeer.encryptor.FinishHMAC(array, 0, num), 0, array, num, EnetPeer.HMAC_SIZE);
            this.SendToSocket(array, num + EnetPeer.HMAC_SIZE);
        }

        private void SendToSocket(byte[] data, int length)
        {
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

        internal void ApplyRandomizedSequenceNumbers()
        {
            bool flag = !this.photonPeer.randomizeSequenceNumbers;
            if (!flag)
            {
                EnetChannel[] obj = this.channelArray;
                lock (obj)
                {
                    foreach (EnetChannel enetChannel in this.channelArray)
                    {
                        int num = (int)this.photonPeer.sequenceNumberSource[(int)enetChannel.ChannelNumber % this.photonPeer.sequenceNumberSource.Length];
                        string debugReturn = string.Format("Channel {0} seqNr in: {1} out: {2}. randomize value: {3}", new object[]
                        {
                            enetChannel.ChannelNumber,
                            enetChannel.incomingReliableSequenceNumber,
                            enetChannel.outgoingReliableSequenceNumber,
                            num
                        });
                        base.EnqueueDebugReturn(DebugLevel.Info, debugReturn);
                        enetChannel.incomingReliableSequenceNumber = num;
                        bool randomizeSequenceNumbersWillSetOutgoingValue = this.photonPeer.randomizeSequenceNumbersWillSetOutgoingValue;
                        if (randomizeSequenceNumbersWillSetOutgoingValue)
                        {
                            enetChannel.outgoingReliableSequenceNumber = num;
                        }
                        else
                        {
                            enetChannel.outgoingReliableSequenceNumber += num;
                        }
                        enetChannel.outgoingReliableUnsequencedNumber = num;
                    }
                }
            }
        }

        internal string CommandListToString(NCommand[] list)
        {
            bool flag = base.debugOut < DebugLevel.All;
            string result;
            if (flag)
            {
                result = string.Empty;
            }
            else
            {
                StringBuilder stringBuilder = new StringBuilder();
                for (int i = 0; i < list.Length; i++)
                {
                    stringBuilder.Append(i + "=");
                    stringBuilder.Append(list[i]);
                    stringBuilder.Append(" # ");
                }
                result = stringBuilder.ToString();
            }
            return result;
        }

        internal override bool Connect(string ipport, string appID, object custom = null)
        {
            bool flag = this.peerConnectionState > ConnectionStateValue.Disconnected;
            bool result;
            if (flag)
            {
                base.Listener.DebugReturn(DebugLevel.Warning, "Connect() can't be called if peer is not Disconnected. Not connecting. peerConnectionState: " + this.peerConnectionState);
                result = false;
            }
            else
            {
                bool flag2 = base.debugOut >= DebugLevel.All;
                if (flag2)
                {
                    base.Listener.DebugReturn(DebugLevel.All, "Connect()");
                }
                base.ServerAddress = ipport;
                this.InitPeerBase();
                bool flag3 = base.SocketImplementation != null;
                if (flag3)
                {
                    this.PhotonSocket = (IPhotonSocket)Activator.CreateInstance(base.SocketImplementation, new object[]
                    {
                        this
                    });
                }
                else
                {
                    this.PhotonSocket = new SocketUdp(this);
                }
                bool flag4 = this.PhotonSocket == null;
                if (flag4)
                {
                    base.Listener.DebugReturn(DebugLevel.Error, "Connect() failed, because SocketImplementation or socket was null. Set PhotonPeer.SocketImplementation before Connect().");
                    result = false;
                }
                else
                {
                    bool flag5 = this.PhotonSocket.Connect();
                    if (flag5)
                    {
                        bool trafficStatsEnabled = base.TrafficStatsEnabled;
                        if (trafficStatsEnabled)
                        {
                            base.TrafficStatsOutgoing.ControlCommandBytes += 44;
                            base.TrafficStatsOutgoing.ControlCommandCount++;
                        }
                        this.peerConnectionState = ConnectionStateValue.Connecting;
                        result = true;
                    }
                    else
                    {
                        result = false;
                    }
                }
            }
            return result;
        }

        internal bool CreateAndEnqueueCommand(byte commandType, StreamBuffer payload, byte channelNumber)
        {
            EnetChannel channel = this.GetChannel(channelNumber);
            this.ByteCountLastOperation = 0;
            int num = this.GetFragmentLength();
            bool flag = payload == null || payload.Length <= num;
            if (flag)
            {
                NCommand ncommand = new NCommand(this, commandType, payload, channel.ChannelNumber);
                bool isFlaggedReliable = ncommand.IsFlaggedReliable;
                if (isFlaggedReliable)
                {
                    this.QueueOutgoingReliableCommand(ncommand);
                    this.ByteCountLastOperation = ncommand.Size;
                    bool trafficStatsEnabled = base.TrafficStatsEnabled;
                    if (trafficStatsEnabled)
                    {
                        base.TrafficStatsOutgoing.CountReliableOpCommand(ncommand.Size);
                        base.TrafficStatsGameLevel.CountOperation(ncommand.Size);
                    }
                }
                else
                {
                    this.QueueOutgoingUnreliableCommand(ncommand);
                    this.ByteCountLastOperation = ncommand.Size;
                    bool trafficStatsEnabled2 = base.TrafficStatsEnabled;
                    if (trafficStatsEnabled2)
                    {
                        base.TrafficStatsOutgoing.CountUnreliableOpCommand(ncommand.Size);
                        base.TrafficStatsGameLevel.CountOperation(ncommand.Size);
                    }
                }
            }
            else
            {
                bool flag2 = commandType == 14 || commandType == 11;
                int fragmentCount = (payload.Length + num - 1) / num;
                int startSequenceNumber = (flag2 ? channel.outgoingReliableUnsequencedNumber : channel.outgoingReliableSequenceNumber) + 1;
                byte[] buffer = payload.GetBuffer();
                int num2 = 0;
                for (int i = 0; i < payload.Length; i += num)
                {
                    bool flag3 = payload.Length - i < num;
                    if (flag3)
                    {
                        num = payload.Length - i;
                    }
                    StreamBuffer streamBuffer = PeerBase.MessageBufferPoolGet();
                    streamBuffer.Write(buffer, i, num);
                    NCommand ncommand2 = new NCommand(this, (byte)(flag2 ? 15 : 8), streamBuffer, channel.ChannelNumber);
                    ncommand2.fragmentNumber = num2;
                    ncommand2.startSequenceNumber = startSequenceNumber;
                    ncommand2.fragmentCount = fragmentCount;
                    ncommand2.totalLength = payload.Length;
                    ncommand2.fragmentOffset = i;
                    this.QueueOutgoingReliableCommand(ncommand2);
                    this.ByteCountLastOperation += ncommand2.Size;
                    bool trafficStatsEnabled3 = base.TrafficStatsEnabled;
                    if (trafficStatsEnabled3)
                    {
                        base.TrafficStatsOutgoing.CountFragmentOpCommand(ncommand2.Size);
                        base.TrafficStatsGameLevel.CountOperation(ncommand2.Size);
                    }
                    num2++;
                }
            }
            return true;
        }

        internal override void Disconnect()
        {
            bool flag = this.peerConnectionState == ConnectionStateValue.Disconnected || this.peerConnectionState == ConnectionStateValue.Disconnecting;
            if (!flag)
            {
                bool flag2 = this.sentReliableCommands != null;
                if (flag2)
                {
                    List<NCommand> obj = this.sentReliableCommands;
                    lock (obj)
                    {
                        this.sentReliableCommands.Clear();
                    }
                }
                EnetChannel[] obj2 = this.channelArray;
                lock (obj2)
                {
                    foreach (EnetChannel enetChannel in this.channelArray)
                    {
                        enetChannel.clearAll();
                    }
                }
                bool isSimulationEnabled = base.NetworkSimulationSettings.IsSimulationEnabled;
                base.NetworkSimulationSettings.IsSimulationEnabled = false;
                NCommand ncommand = new NCommand(this, 4, null, byte.MaxValue);
                this.QueueOutgoingReliableCommand(ncommand);
                this.SendOutgoingCommands();
                bool trafficStatsEnabled = base.TrafficStatsEnabled;
                if (trafficStatsEnabled)
                {
                    base.TrafficStatsOutgoing.CountControlCommand(ncommand.Size);
                }
                base.NetworkSimulationSettings.IsSimulationEnabled = isSimulationEnabled;
                this.PhotonSocket.Disconnect();
                this.peerConnectionState = ConnectionStateValue.Disconnected;
                base.EnqueueStatusCallback(StatusCode.Disconnect);
                this.datagramEncryptedConnection = false;
            }
        }

        internal override bool DispatchIncomingCommands()
        {
            int count = this.CommandQueue.Count;
            bool flag = count > 0;
            if (flag)
            {
                for (int i = 0; i < count; i++)
                {
                    Queue<NCommand> commandQueue = this.CommandQueue;
                    lock (commandQueue)
                    {
                        NCommand command = this.CommandQueue.Dequeue();
                        this.ExecuteCommand(command);
                    }
                }
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
            NCommand ncommand = null;
            EnetChannel[] obj = this.channelArray;
            lock (obj)
            {
                for (int j = 0; j < this.channelArray.Length; j++)
                {
                    EnetChannel enetChannel = this.channelArray[j];
                    bool flag3 = enetChannel.incomingUnsequencedCommandsList.Count > 0;
                    if (flag3)
                    {
                        ncommand = enetChannel.incomingUnsequencedCommandsList.Dequeue();
                        break;
                    }
                    bool flag4 = enetChannel.incomingUnreliableCommandsList.Count > 0;
                    if (flag4)
                    {
                        int num = int.MaxValue;
                        foreach (int num2 in enetChannel.incomingUnreliableCommandsList.Keys)
                        {
                            NCommand ncommand2 = enetChannel.incomingUnreliableCommandsList[num2];
                            bool flag5 = num2 < enetChannel.incomingUnreliableSequenceNumber || ncommand2.reliableSequenceNumber < enetChannel.incomingReliableSequenceNumber;
                            if (flag5)
                            {
                                this.commandsToRemove.Enqueue(num2);
                            }
                            else
                            {
                                int limitOfUnreliableCommands = this.photonPeer.LimitOfUnreliableCommands;
                                bool flag6 = limitOfUnreliableCommands > 0 && enetChannel.incomingUnreliableCommandsList.Count > limitOfUnreliableCommands;
                                if (flag6)
                                {
                                    this.commandsToRemove.Enqueue(num2);
                                }
                                else
                                {
                                    bool flag7 = num2 < num;
                                    if (flag7)
                                    {
                                        bool flag8 = ncommand2.reliableSequenceNumber > enetChannel.incomingReliableSequenceNumber;
                                        if (!flag8)
                                        {
                                            num = num2;
                                        }
                                    }
                                }
                            }
                        }
                        while (this.commandsToRemove.Count > 0)
                        {
                            enetChannel.incomingUnreliableCommandsList.Remove(this.commandsToRemove.Dequeue());
                        }
                        bool flag9 = num < int.MaxValue;
                        if (flag9)
                        {
                            ncommand = enetChannel.incomingUnreliableCommandsList[num];
                        }
                        bool flag10 = ncommand != null;
                        if (flag10)
                        {
                            enetChannel.incomingUnreliableCommandsList.Remove(ncommand.unreliableSequenceNumber);
                            enetChannel.incomingUnreliableSequenceNumber = ncommand.unreliableSequenceNumber;
                            break;
                        }
                    }
                    bool flag11 = ncommand == null && enetChannel.incomingReliableCommandsList.Count > 0;
                    if (flag11)
                    {
                        enetChannel.incomingReliableCommandsList.TryGetValue(enetChannel.incomingReliableSequenceNumber + 1, out ncommand);
                        bool flag12 = ncommand == null;
                        if (!flag12)
                        {
                            bool flag13 = ncommand.commandType != 8;
                            if (flag13)
                            {
                                enetChannel.incomingReliableSequenceNumber = ncommand.reliableSequenceNumber;
                                enetChannel.incomingReliableCommandsList.Remove(ncommand.reliableSequenceNumber);
                                break;
                            }
                            bool flag14 = ncommand.fragmentsRemaining > 0;
                            if (flag14)
                            {
                                ncommand = null;
                            }
                            else
                            {
                                enetChannel.incomingReliableSequenceNumber = ncommand.reliableSequenceNumber + ncommand.fragmentCount - 1;
                                enetChannel.incomingReliableCommandsList.Remove(ncommand.reliableSequenceNumber);
                            }
                            break;
                        }
                    }
                }
            }
            bool flag15 = ncommand != null && ncommand.Payload != null;
            if (flag15)
            {
                this.ByteCountCurrentDispatch = ncommand.Size;
                this.CommandInCurrentDispatch = ncommand;
                bool flag16 = this.DeserializeMessageAndCallback(ncommand.Payload);
                if (flag16)
                {
                    ncommand.FreePayload();
                    this.CommandInCurrentDispatch = null;
                    return true;
                }
                this.CommandInCurrentDispatch = null;
            }
            return false;
        }

        internal override bool EnqueueMessage(object message, SendOptions sendOptions)
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
                    StreamBuffer payload = base.SerializeMessageToMessage(message, sendOptions.Encrypt, EnetPeer.messageHeader, false);
                    byte commandType = 7;
                    bool flag5 = sendOptions.DeliveryMode == DeliveryMode.UnreliableUnsequenced;
                    if (flag5)
                    {
                        commandType = 11;
                    }
                    else
                    {
                        bool flag6 = sendOptions.DeliveryMode == DeliveryMode.ReliableUnsequenced;
                        if (flag6)
                        {
                            commandType = 14;
                        }
                        else
                        {
                            bool flag7 = sendOptions.DeliveryMode == DeliveryMode.Reliable;
                            if (flag7)
                            {
                                commandType = 6;
                            }
                        }
                    }
                    result = this.CreateAndEnqueueCommand(commandType, payload, channel);
                }
            }
            return result;
        }

        internal override bool EnqueueOperation(Dictionary<byte, object> parameters, byte opCode, SendOptions sendParams, EgMessageType messageType = EgMessageType.Operation)
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
                        " Not connected. PeerState: ",
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
                    byte commandType = 7;
                    bool flag5 = sendParams.DeliveryMode == DeliveryMode.UnreliableUnsequenced;
                    if (flag5)
                    {
                        commandType = 11;
                    }
                    else
                    {
                        bool flag6 = sendParams.DeliveryMode == DeliveryMode.ReliableUnsequenced;
                        if (flag6)
                        {
                            commandType = 14;
                        }
                        else
                        {
                            bool flag7 = sendParams.DeliveryMode == DeliveryMode.Reliable;
                            if (flag7)
                            {
                                commandType = 6;
                            }
                        }
                    }
                    StreamBuffer payload = this.SerializeOperationToMessage(opCode, parameters, messageType, sendParams.Encrypt);
                    result = this.CreateAndEnqueueCommand(commandType, payload, sendParams.Channel);
                }
            }
            return result;
        }

        internal bool ExecuteCommand(NCommand command)
        {
            bool result = true;
            switch (command.commandType)
            {
                case 1:
                case 16:
                    {
                        bool trafficStatsEnabled = base.TrafficStatsEnabled;
                        if (trafficStatsEnabled)
                        {
                            base.TrafficStatsIncoming.TimestampOfLastAck = SupportClass.GetTickCount();
                            base.TrafficStatsIncoming.CountControlCommand(command.Size);
                        }
                        this.timeLastAckReceive = this.timeInt;
                        this.lastRoundTripTime = this.timeInt - command.ackReceivedSentTime;
                        bool flag = this.lastRoundTripTime < 0 || this.lastRoundTripTime > this.roundTripTime * 4;
                        if (flag)
                        {
                            bool flag2 = base.debugOut >= DebugLevel.Info;
                            if (flag2)
                            {
                                base.EnqueueDebugReturn(DebugLevel.Info, string.Concat(new object[]
                                {
                            "LastRoundtripTime is suspicious: ",
                            this.lastRoundTripTime,
                            " for command: ",
                            command
                                }));
                            }
                            this.lastRoundTripTime = this.roundTripTime * 4;
                        }
                        NCommand ncommand = this.RemoveSentReliableCommand(command.ackReceivedReliableSequenceNumber, (int)command.commandChannelID, command.commandType == 16);
                        bool flag3 = this.CommandLog != null;
                        if (flag3)
                        {
                            this.CommandLog.Enqueue(new CmdLogReceivedAck(command, this.timeInt, this.roundTripTime, this.roundTripTimeVariance));
                            base.CommandLogResize();
                        }
                        bool flag4 = ncommand != null;
                        if (flag4)
                        {
                            ncommand.FreePayload();
                            bool flag5 = ncommand.commandType == 12;
                            if (flag5)
                            {
                                bool flag6 = this.lastRoundTripTime <= this.roundTripTime;
                                if (flag6)
                                {
                                    this.serverTimeOffset = this.serverSentTime + (this.lastRoundTripTime >> 1) - SupportClass.GetTickCount();
                                    this.serverTimeOffsetIsAvailable = true;
                                }
                                else
                                {
                                    this.FetchServerTimestamp();
                                }
                            }
                            else
                            {
                                base.UpdateRoundTripTimeAndVariance(this.lastRoundTripTime);
                                bool flag7 = ncommand.commandType == 4 && this.peerConnectionState == ConnectionStateValue.Disconnecting;
                                if (flag7)
                                {
                                    bool flag8 = base.debugOut >= DebugLevel.Info;
                                    if (flag8)
                                    {
                                        base.EnqueueDebugReturn(DebugLevel.Info, "Received disconnect ACK by server");
                                    }
                                    base.EnqueueActionForDispatch(delegate
                                    {
                                        this.PhotonSocket.Disconnect();
                                    });
                                }
                                else
                                {
                                    bool flag9 = ncommand.commandType == 2;
                                    if (flag9)
                                    {
                                        this.roundTripTime = this.lastRoundTripTime;
                                    }
                                }
                            }
                        }
                        break;
                    }
                case 2:
                case 5:
                    {
                        bool trafficStatsEnabled2 = base.TrafficStatsEnabled;
                        if (trafficStatsEnabled2)
                        {
                            base.TrafficStatsIncoming.CountControlCommand(command.Size);
                        }
                        break;
                    }
                case 3:
                    {
                        bool trafficStatsEnabled3 = base.TrafficStatsEnabled;
                        if (trafficStatsEnabled3)
                        {
                            base.TrafficStatsIncoming.CountControlCommand(command.Size);
                        }
                        bool flag10 = this.peerConnectionState == ConnectionStateValue.Connecting;
                        if (flag10)
                        {
                            byte[] buf = base.PrepareConnectData(base.ServerAddress, this.AppId, this.CustomInitData);
                            this.CreateAndEnqueueCommand(6, new StreamBuffer(buf), 0);
                            bool randomizeSequenceNumbers = this.photonPeer.randomizeSequenceNumbers;
                            if (randomizeSequenceNumbers)
                            {
                                this.ApplyRandomizedSequenceNumbers();
                            }
                            this.peerConnectionState = ConnectionStateValue.Connected;
                        }
                        break;
                    }
                case 4:
                    {
                        bool trafficStatsEnabled4 = base.TrafficStatsEnabled;
                        if (trafficStatsEnabled4)
                        {
                            base.TrafficStatsIncoming.CountControlCommand(command.Size);
                        }
                        StatusCode statusValue = StatusCode.DisconnectByServerReasonUnknown;
                        bool flag11 = command.reservedByte == 1;
                        if (flag11)
                        {
                            statusValue = StatusCode.DisconnectByServerLogic;
                        }
                        else
                        {
                            bool flag12 = command.reservedByte == 2;
                            if (flag12)
                            {
                                statusValue = StatusCode.DisconnectByServer;
                            }
                            else
                            {
                                bool flag13 = command.reservedByte == 3;
                                if (flag13)
                                {
                                    statusValue = StatusCode.DisconnectByServerUserLimit;
                                }
                            }
                        }
                        bool flag14 = base.debugOut >= DebugLevel.Info;
                        if (flag14)
                        {
                            base.Listener.DebugReturn(DebugLevel.Info, string.Concat(new object[]
                            {
                        "Server ",
                        base.ServerAddress,
                        " sent disconnect. PeerId: ",
                        (ushort)this.peerID,
                        " RTT/Variance:",
                        this.roundTripTime,
                        "/",
                        this.roundTripTimeVariance,
                        " reason byte: ",
                        command.reservedByte
                            }));
                        }
                        base.EnqueueStatusCallback(statusValue);
                        this.Disconnect();
                        break;
                    }
                case 6:
                    {
                        bool trafficStatsEnabled5 = base.TrafficStatsEnabled;
                        if (trafficStatsEnabled5)
                        {
                            base.TrafficStatsIncoming.CountReliableOpCommand(command.Size);
                        }
                        bool flag15 = this.peerConnectionState == ConnectionStateValue.Connected;
                        if (flag15)
                        {
                            result = this.QueueIncomingCommand(command);
                        }
                        break;
                    }
                case 7:
                    {
                        bool trafficStatsEnabled6 = base.TrafficStatsEnabled;
                        if (trafficStatsEnabled6)
                        {
                            base.TrafficStatsIncoming.CountUnreliableOpCommand(command.Size);
                        }
                        bool flag16 = this.peerConnectionState == ConnectionStateValue.Connected;
                        if (flag16)
                        {
                            result = this.QueueIncomingCommand(command);
                        }
                        break;
                    }
                case 8:
                case 15:
                    {
                        bool flag17 = this.peerConnectionState != ConnectionStateValue.Connected;
                        if (!flag17)
                        {
                            bool trafficStatsEnabled7 = base.TrafficStatsEnabled;
                            if (trafficStatsEnabled7)
                            {
                                base.TrafficStatsIncoming.CountFragmentOpCommand(command.Size);
                            }
                            bool flag18 = command.fragmentNumber > command.fragmentCount || command.fragmentOffset >= command.totalLength || command.fragmentOffset + command.Payload.Length > command.totalLength;
                            if (flag18)
                            {
                                bool flag19 = base.debugOut >= DebugLevel.Error;
                                if (flag19)
                                {
                                    base.Listener.DebugReturn(DebugLevel.Error, "Received fragment has bad size: " + command);
                                }
                            }
                            else
                            {
                                bool flag20 = command.commandType == 8;
                                EnetChannel channel = this.GetChannel(command.commandChannelID);
                                NCommand ncommand2 = null;
                                bool flag21 = channel.TryGetFragment(command.startSequenceNumber, flag20, out ncommand2);
                                bool flag22 = flag21 && ncommand2.fragmentsRemaining <= 0;
                                if (!flag22)
                                {
                                    bool flag23 = this.QueueIncomingCommand(command);
                                    bool flag24 = !flag23;
                                    if (!flag24)
                                    {
                                        bool flag25 = command.reliableSequenceNumber != command.startSequenceNumber;
                                        if (flag25)
                                        {
                                            bool flag26 = flag21;
                                            if (flag26)
                                            {
                                                ncommand2.fragmentsRemaining--;
                                            }
                                        }
                                        else
                                        {
                                            ncommand2 = command;
                                            ncommand2.fragmentsRemaining--;
                                            NCommand ncommand3 = null;
                                            int num = command.startSequenceNumber + 1;
                                            while (ncommand2.fragmentsRemaining > 0 && num < ncommand2.startSequenceNumber + ncommand2.fragmentCount)
                                            {
                                                bool flag27 = channel.TryGetFragment(num++, flag20, out ncommand3);
                                                if (flag27)
                                                {
                                                    ncommand2.fragmentsRemaining--;
                                                }
                                            }
                                        }
                                        bool flag28 = ncommand2 == null || ncommand2.fragmentsRemaining > 0;
                                        if (!flag28)
                                        {
                                            byte[] array = new byte[ncommand2.totalLength];
                                            for (int i = ncommand2.startSequenceNumber; i < ncommand2.startSequenceNumber + ncommand2.fragmentCount; i++)
                                            {
                                                NCommand ncommand4;
                                                bool flag29 = channel.TryGetFragment(i, flag20, out ncommand4);
                                                if (!flag29)
                                                {
                                                    throw new Exception("startCommand.fragmentsRemaining was 0 but not all fragments were found to be combined!");
                                                }
                                                Buffer.BlockCopy(ncommand4.Payload.GetBuffer(), 0, array, ncommand4.fragmentOffset, ncommand4.Payload.Length);
                                                ncommand4.FreePayload();
                                                channel.RemoveFragment(ncommand4.reliableSequenceNumber, flag20);
                                            }
                                            ncommand2.Payload = new StreamBuffer(array);
                                            ncommand2.Size = 12 * ncommand2.fragmentCount + ncommand2.totalLength;
                                            bool flag30 = !flag20;
                                            if (flag30)
                                            {
                                                channel.incomingUnsequencedCommandsList.Enqueue(ncommand2);
                                            }
                                            else
                                            {
                                                channel.incomingReliableCommandsList.Add(ncommand2.startSequenceNumber, ncommand2);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    }
                case 11:
                    {
                        bool trafficStatsEnabled8 = base.TrafficStatsEnabled;
                        if (trafficStatsEnabled8)
                        {
                            base.TrafficStatsIncoming.CountUnreliableOpCommand(command.Size);
                        }
                        bool flag31 = this.peerConnectionState == ConnectionStateValue.Connected;
                        if (flag31)
                        {
                            result = this.QueueIncomingCommand(command);
                        }
                        break;
                    }
                case 14:
                    {
                        bool trafficStatsEnabled9 = base.TrafficStatsEnabled;
                        if (trafficStatsEnabled9)
                        {
                            base.TrafficStatsIncoming.CountReliableOpCommand(command.Size);
                        }
                        bool flag32 = this.peerConnectionState == ConnectionStateValue.Connected;
                        if (flag32)
                        {
                            result = this.QueueIncomingCommand(command);
                        }
                        break;
                    }
            }
            return result;
        }

        internal override void FetchServerTimestamp()
        {
            bool flag = this.peerConnectionState != ConnectionStateValue.Connected || !this.ApplicationIsInitialized;
            if (flag)
            {
                bool flag2 = base.debugOut >= DebugLevel.Info;
                if (flag2)
                {
                    base.EnqueueDebugReturn(DebugLevel.Info, "FetchServerTimestamp() was skipped, as the client is not connected. Current ConnectionState: " + this.peerConnectionState);
                }
            }
            else
            {
                this.CreateAndEnqueueCommand(12, null, byte.MaxValue);
            }
        }

        internal override void InitPeerBase()
        {
            base.InitPeerBase();
            bool flag = this.photonPeer.PayloadEncryptionSecret != null && this.usedTransportProtocol == ConnectionProtocol.Udp;
            if (flag)
            {
                this.InitEncryption(this.photonPeer.PayloadEncryptionSecret);
            }
            bool flag2 = this.photonPeer.encryptor != null && this.photonPeer.decryptor != null;
            if (flag2)
            {
                this.isEncryptionAvailable = true;
            }
            this.peerID = (short)(this.photonPeer.EnableServerTracing ? -2 : -1);
            this.challenge = SupportClass.ThreadSafeRandom.Next();
            bool flag3 = this.udpBuffer == null || this.udpBuffer.Length != base.mtu;
            if (flag3)
            {
                this.udpBuffer = new byte[base.mtu];
            }
            this.reliableCommandsSent = 0;
            this.reliableCommandsRepeated = 0;
            EnetChannel[] obj = this.channelArray;
            lock (obj)
            {
                EnetChannel[] array = this.channelArray;
                bool flag4 = array.Length != (int)(base.ChannelCount + 1);
                if (flag4)
                {
                    array = new EnetChannel[(int)(base.ChannelCount + 1)];
                }
                for (byte b = 0; b < base.ChannelCount; b += 1)
                {
                    array[(int)b] = new EnetChannel(b, this.commandBufferSize);
                }
                array[(int)base.ChannelCount] = new EnetChannel(byte.MaxValue, this.commandBufferSize);
                this.channelArray = array;
            }
            List<NCommand> obj2 = this.sentReliableCommands;
            lock (obj2)
            {
                this.sentReliableCommands = new List<NCommand>(this.commandBufferSize);
            }
            this.outgoingAcknowledgementsPool = new StreamBuffer(0);
            base.CommandLogInit();
        }

        internal bool QueueIncomingCommand(NCommand command)
        {
            EnetChannel channel = this.GetChannel(command.commandChannelID);
            bool flag = channel == null;
            bool result;
            if (flag)
            {
                bool flag2 = base.debugOut >= DebugLevel.Error;
                if (flag2)
                {
                    base.Listener.DebugReturn(DebugLevel.Error, "Received command for non-existing channel: " + command.commandChannelID);
                }
                result = false;
            }
            else
            {
                bool flag3 = base.debugOut >= DebugLevel.All;
                if (flag3)
                {
                    base.Listener.DebugReturn(DebugLevel.All, string.Concat(new object[]
                    {
                        "queueIncomingCommand() ",
                        command,
                        " channel seq# r/u: ",
                        channel.incomingReliableSequenceNumber,
                        "/",
                        channel.incomingUnreliableSequenceNumber
                    }));
                }
                bool isFlaggedReliable = command.IsFlaggedReliable;
                if (isFlaggedReliable)
                {
                    bool isFlaggedUnsequenced = command.IsFlaggedUnsequenced;
                    if (isFlaggedUnsequenced)
                    {
                        result = channel.QueueIncomingReliableUnsequenced(command);
                    }
                    else
                    {
                        bool flag4 = command.reliableSequenceNumber <= channel.incomingReliableSequenceNumber;
                        if (flag4)
                        {
                            bool flag5 = base.debugOut >= DebugLevel.Info;
                            if (flag5)
                            {
                                base.Listener.DebugReturn(DebugLevel.Info, string.Concat(new object[]
                                {
                                    "incoming command ",
                                    command,
                                    " is old (not saving it). Dispatched incomingReliableSequenceNumber: ",
                                    channel.incomingReliableSequenceNumber
                                }));
                            }
                            result = false;
                        }
                        else
                        {
                            bool flag6 = channel.ContainsReliableSequenceNumber(command.reliableSequenceNumber);
                            if (flag6)
                            {
                                bool flag7 = base.debugOut >= DebugLevel.Info;
                                if (flag7)
                                {
                                    base.Listener.DebugReturn(DebugLevel.Info, string.Concat(new object[]
                                    {
                                        "Info: command was received before! Old/New: ",
                                        channel.FetchReliableSequenceNumber(command.reliableSequenceNumber),
                                        "/",
                                        command,
                                        " inReliableSeq#: ",
                                        channel.incomingReliableSequenceNumber
                                    }));
                                }
                                result = false;
                            }
                            else
                            {
                                channel.incomingReliableCommandsList.Add(command.reliableSequenceNumber, command);
                                result = true;
                            }
                        }
                    }
                }
                else
                {
                    bool flag8 = command.commandFlags == 0;
                    if (flag8)
                    {
                        bool flag9 = command.reliableSequenceNumber < channel.incomingReliableSequenceNumber;
                        if (flag9)
                        {
                            bool flag10 = base.debugOut >= DebugLevel.Info;
                            if (flag10)
                            {
                                base.Listener.DebugReturn(DebugLevel.Info, "incoming reliable-seq# < Dispatched-rel-seq#. not saved.");
                            }
                            result = true;
                        }
                        else
                        {
                            bool flag11 = command.unreliableSequenceNumber <= channel.incomingUnreliableSequenceNumber;
                            if (flag11)
                            {
                                bool flag12 = base.debugOut >= DebugLevel.Info;
                                if (flag12)
                                {
                                    base.Listener.DebugReturn(DebugLevel.Info, "incoming unreliable-seq# < Dispatched-unrel-seq#. not saved.");
                                }
                                result = true;
                            }
                            else
                            {
                                bool flag13 = channel.ContainsUnreliableSequenceNumber(command.unreliableSequenceNumber);
                                if (flag13)
                                {
                                    bool flag14 = base.debugOut >= DebugLevel.Info;
                                    if (flag14)
                                    {
                                        base.Listener.DebugReturn(DebugLevel.Info, string.Concat(new object[]
                                        {
                                            "command was received before! Old/New: ",
                                            channel.incomingUnreliableCommandsList[command.unreliableSequenceNumber],
                                            "/",
                                            command
                                        }));
                                    }
                                    result = false;
                                }
                                else
                                {
                                    channel.incomingUnreliableCommandsList.Add(command.unreliableSequenceNumber, command);
                                    result = true;
                                }
                            }
                        }
                    }
                    else
                    {
                        bool flag15 = command.commandFlags == 2;
                        if (flag15)
                        {
                            int unsequencedGroupNumber = command.unsequencedGroupNumber;
                            int num = command.unsequencedGroupNumber % 128;
                            bool flag16 = unsequencedGroupNumber >= this.incomingUnsequencedGroupNumber + 128;
                            if (flag16)
                            {
                                this.incomingUnsequencedGroupNumber = unsequencedGroupNumber - num;
                                for (int i = 0; i < this.unsequencedWindow.Length; i++)
                                {
                                    this.unsequencedWindow[i] = 0;
                                }
                            }
                            else
                            {
                                bool flag17 = unsequencedGroupNumber < this.incomingUnsequencedGroupNumber || (this.unsequencedWindow[num / 32] & 1 << num % 32) != 0;
                                if (flag17)
                                {
                                    return false;
                                }
                            }
                            this.unsequencedWindow[num / 32] |= 1 << num % 32;
                            channel.incomingUnsequencedCommandsList.Enqueue(command);
                            result = true;
                        }
                        else
                        {
                            result = false;
                        }
                    }
                }
            }
            return result;
        }

        internal void QueueOutgoingAcknowledgement(NCommand readCommand, int sendTime)
        {
            StreamBuffer obj = this.outgoingAcknowledgementsPool;
            lock (obj)
            {
                int offset;
                byte[] bufferAndAdvance = this.outgoingAcknowledgementsPool.GetBufferAndAdvance(20, out offset);
                NCommand.CreateAck(bufferAndAdvance, offset, readCommand, sendTime);
            }
        }

        internal void QueueOutgoingReliableCommand(NCommand command)
        {
            EnetChannel channel = this.GetChannel(command.commandChannelID);
            EnetChannel obj = channel;
            lock (obj)
            {
                bool flag = command.reliableSequenceNumber == 0;
                if (flag)
                {
                    bool isFlaggedUnsequenced = command.IsFlaggedUnsequenced;
                    if (isFlaggedUnsequenced)
                    {
                        EnetChannel enetChannel = channel;
                        int num = enetChannel.outgoingReliableUnsequencedNumber + 1;
                        enetChannel.outgoingReliableUnsequencedNumber = num;
                        command.reliableSequenceNumber = num;
                    }
                    else
                    {
                        EnetChannel enetChannel2 = channel;
                        int num = enetChannel2.outgoingReliableSequenceNumber + 1;
                        enetChannel2.outgoingReliableSequenceNumber = num;
                        command.reliableSequenceNumber = num;
                    }
                }
                channel.outgoingReliableCommandsList.Enqueue(command);
            }
        }

        internal void QueueOutgoingUnreliableCommand(NCommand command)
        {
            EnetChannel channel = this.GetChannel(command.commandChannelID);
            EnetChannel obj = channel;
            lock (obj)
            {
                bool isFlaggedUnsequenced = command.IsFlaggedUnsequenced;
                if (isFlaggedUnsequenced)
                {
                    command.reliableSequenceNumber = 0;
                    int num = this.outgoingUnsequencedGroupNumber + 1;
                    this.outgoingUnsequencedGroupNumber = num;
                    command.unsequencedGroupNumber = num;
                }
                else
                {
                    command.reliableSequenceNumber = channel.outgoingReliableSequenceNumber;
                    EnetChannel enetChannel = channel;
                    int num = enetChannel.outgoingUnreliableSequenceNumber + 1;
                    enetChannel.outgoingUnreliableSequenceNumber = num;
                    command.unreliableSequenceNumber = num;
                }
                channel.outgoingUnreliableCommandsList.Enqueue(command);
            }
        }

        internal void QueueSentCommand(NCommand command)
        {
            command.commandSentTime = this.timeInt;
            command.commandSentCount += 1;
            bool flag = command.roundTripTimeout == 0;
            if (flag)
            {
                command.roundTripTimeout = this.roundTripTime + 4 * this.roundTripTimeVariance;
                command.timeoutTime = this.timeInt + base.DisconnectTimeout;
            }
            else
            {
                bool flag2 = command.commandSentCount <= this.photonPeer.QuickResendAttempts + 1;
                if (!flag2)
                {
                    command.roundTripTimeout *= 2;
                }
            }
            List<NCommand> obj = this.sentReliableCommands;
            lock (obj)
            {
                bool flag3 = this.sentReliableCommands.Count == 0;
                if (flag3)
                {
                    int num = command.commandSentTime + command.roundTripTimeout;
                    bool flag4 = num < this.timeoutInt;
                    if (flag4)
                    {
                        this.timeoutInt = num;
                    }
                }
                this.reliableCommandsSent++;
                this.sentReliableCommands.Add(command);
            }
        }

        internal override void ReceiveIncomingCommands(byte[] inBuff, int dataLength)
        {
            this.timestampOfLastReceive = SupportClass.GetTickCount();
            try
            {
                int num = 0;
                short num2;
                Protocol.Deserialize(out num2, inBuff, ref num);
                byte b = inBuff[num++];
                bool flag = b == 1;
                int num3;
                byte b2;
                if (flag)
                {
                    bool flag2 = this.photonPeer.decryptor == null;
                    if (flag2)
                    {
                        base.EnqueueDebugReturn(DebugLevel.Error, "Got encrypted packet, but encryption is not set up. Packet ignored");
                        return;
                    }
                    this.datagramEncryptedConnection = true;
                    bool flag3 = !this.photonPeer.decryptor.CheckHMAC(inBuff, dataLength);
                    if (flag3)
                    {
                        this.packetLossByCrc++;
                        bool flag4 = this.peerConnectionState != ConnectionStateValue.Disconnected && base.debugOut >= DebugLevel.Info;
                        if (flag4)
                        {
                            base.EnqueueDebugReturn(DebugLevel.Info, "Ignored package due to wrong HMAC.");
                        }
                        return;
                    }
                    Protocol.Deserialize(out num3, inBuff, ref num);
                    inBuff = this.photonPeer.decryptor.DecryptBufferWithIV(inBuff, num, dataLength - num - EnetPeer.HMAC_SIZE, out dataLength);
                    dataLength = inBuff.Length;
                    num = 0;
                    b2 = inBuff[num++];
                    Protocol.Deserialize(out this.serverSentTime, inBuff, ref num);
                    this.bytesIn += (long)(12 + EnetPeer.IV_SIZE + EnetPeer.HMAC_SIZE + dataLength + (EnetPeer.BLOCK_SIZE - dataLength % EnetPeer.BLOCK_SIZE));
                }
                else
                {
                    bool flag5 = this.datagramEncryptedConnection;
                    if (flag5)
                    {
                        base.EnqueueDebugReturn(DebugLevel.Warning, "Got not encrypted packet, but expected only encrypted. Packet ignored");
                        return;
                    }
                    b2 = inBuff[num++];
                    Protocol.Deserialize(out this.serverSentTime, inBuff, ref num);
                    Protocol.Deserialize(out num3, inBuff, ref num);
                    bool flag6 = b == 204;
                    if (flag6)
                    {
                        int num4;
                        Protocol.Deserialize(out num4, inBuff, ref num);
                        this.bytesIn += 4L;
                        num -= 4;
                        Protocol.Serialize(0, inBuff, ref num);
                        uint num5 = SupportClass.CalculateCrc(inBuff, dataLength);
                        bool flag7 = num4 != (int)num5;
                        if (flag7)
                        {
                            this.packetLossByCrc++;
                            bool flag8 = this.peerConnectionState != ConnectionStateValue.Disconnected && base.debugOut >= DebugLevel.Info;
                            if (flag8)
                            {
                                base.EnqueueDebugReturn(DebugLevel.Info, string.Format("Ignored package due to wrong CRC. Incoming:  {0:X} Local: {1:X}", (uint)num4, num5));
                            }
                            return;
                        }
                    }
                    this.bytesIn += 12L;
                }
                bool trafficStatsEnabled = base.TrafficStatsEnabled;
                if (trafficStatsEnabled)
                {
                    TrafficStats trafficStatsIncoming = base.TrafficStatsIncoming;
                    int totalPacketCount = trafficStatsIncoming.TotalPacketCount;
                    trafficStatsIncoming.TotalPacketCount = totalPacketCount + 1;
                    base.TrafficStatsIncoming.TotalCommandsInPackets += (int)b2;
                }
                bool flag9 = (int)b2 > this.commandBufferSize || b2 <= 0;
                if (flag9)
                {
                    base.EnqueueDebugReturn(DebugLevel.Error, string.Concat(new object[]
                    {
                        "too many/few incoming commands in package: ",
                        b2,
                        " > ",
                        this.commandBufferSize
                    }));
                }
                bool flag10 = num3 != this.challenge;
                if (flag10)
                {
                    this.packetLossByChallenge++;
                    bool flag11 = this.peerConnectionState != ConnectionStateValue.Disconnected && base.debugOut >= DebugLevel.All;
                    if (flag11)
                    {
                        base.EnqueueDebugReturn(DebugLevel.All, string.Concat(new object[]
                        {
                            "Info: Ignoring received package due to wrong challenge. Challenge in-package!=local:",
                            num3,
                            "!=",
                            this.challenge,
                            " Commands in it: ",
                            b2
                        }));
                    }
                }
                else
                {
                    this.timeInt = SupportClass.GetTickCount() - this.timeBase;
                    for (int i = 0; i < (int)b2; i++)
                    {
                        NCommand ncommand = new NCommand(this, inBuff, ref num);
                        bool flag12 = ncommand.commandType != 1 && ncommand.commandType != 16;
                        if (flag12)
                        {
                            Queue<NCommand> commandQueue = this.CommandQueue;
                            lock (commandQueue)
                            {
                                this.CommandQueue.Enqueue(ncommand);
                            }
                        }
                        else
                        {
                            this.ExecuteCommand(ncommand);
                        }
                        bool isFlaggedReliable = ncommand.IsFlaggedReliable;
                        if (isFlaggedReliable)
                        {
                            bool flag13 = this.InReliableLog != null;
                            if (flag13)
                            {
                                this.InReliableLog.Enqueue(new CmdLogReceivedReliable(ncommand, this.timeInt, this.roundTripTime, this.roundTripTimeVariance, this.timeInt - this.timeLastSendOutgoing, this.timeInt - this.timeLastSendAck));
                                base.CommandLogResize();
                            }
                            this.QueueOutgoingAcknowledgement(ncommand, this.serverSentTime);
                            bool trafficStatsEnabled2 = base.TrafficStatsEnabled;
                            if (trafficStatsEnabled2)
                            {
                                base.TrafficStatsIncoming.TimestampOfLastReliableCommand = SupportClass.GetTickCount();
                                base.TrafficStatsOutgoing.CountControlCommand(20);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                bool flag14 = base.debugOut >= DebugLevel.Error;
                if (flag14)
                {
                    base.EnqueueDebugReturn(DebugLevel.Error, string.Format("Exception while reading commands from incoming data: {0}", ex));
                }
                SupportClass.WriteStackTrace(ex);
            }
        }

        internal NCommand RemoveSentReliableCommand(int ackReceivedReliableSequenceNumber, int ackReceivedChannel, bool isUnsequenced)
        {
            NCommand ncommand = null;
            List<NCommand> obj = this.sentReliableCommands;
            lock (obj)
            {
                foreach (NCommand ncommand2 in this.sentReliableCommands)
                {
                    bool flag = ncommand2 != null && ncommand2.reliableSequenceNumber == ackReceivedReliableSequenceNumber && ncommand2.IsFlaggedUnsequenced == isUnsequenced && (int)ncommand2.commandChannelID == ackReceivedChannel;
                    if (flag)
                    {
                        ncommand = ncommand2;
                        break;
                    }
                }
                bool flag2 = ncommand != null;
                if (flag2)
                {
                    this.sentReliableCommands.Remove(ncommand);
                    bool flag3 = this.sentReliableCommands.Count > 0;
                    if (flag3)
                    {
                        this.timeoutInt = this.timeInt + 25;
                    }
                }
                else
                {
                    bool flag4 = base.debugOut >= DebugLevel.All && this.peerConnectionState != ConnectionStateValue.Connected && this.peerConnectionState != ConnectionStateValue.Disconnecting;
                    if (flag4)
                    {
                        base.EnqueueDebugReturn(DebugLevel.All, string.Format("No sent command for ACK (Ch: {0} Sq#: {1}). PeerState: {2}.", ackReceivedReliableSequenceNumber, ackReceivedChannel, this.peerConnectionState));
                    }
                }
            }
            return ncommand;
        }

        internal override bool SendAcksOnly()
        {
            bool flag = this.peerConnectionState == ConnectionStateValue.Disconnected;
            bool result;
            if (flag)
            {
                result = false;
            }
            else
            {
                bool flag2 = this.PhotonSocket == null || !this.PhotonSocket.Connected;
                if (flag2)
                {
                    result = false;
                }
                else
                {
                    byte[] obj = this.udpBuffer;
                    lock (obj)
                    {
                        int num = 0;
                        this.udpBufferIndex = this.CalculateInitialOffset();
                        this.udpBufferLength = this.CalculateBufferLen();
                        this.udpCommandCount = 0;
                        this.timeInt = SupportClass.GetTickCount() - this.timeBase;
                        StreamBuffer obj2 = this.outgoingAcknowledgementsPool;
                        lock (obj2)
                        {
                            num = this.SerializeAckToBuffer();
                            this.timeLastSendAck = this.timeInt;
                        }
                        bool flag3 = this.timeInt > this.timeoutInt && this.sentReliableCommands.Count > 0;
                        if (flag3)
                        {
                            List<NCommand> obj3 = this.sentReliableCommands;
                            lock (obj3)
                            {
                                foreach (NCommand ncommand in this.sentReliableCommands)
                                {
                                    bool flag4 = ncommand != null && ncommand.roundTripTimeout != 0 && this.timeInt - ncommand.commandSentTime > ncommand.roundTripTimeout;
                                    if (flag4)
                                    {
                                        ncommand.commandSentCount = 1;
                                        ncommand.roundTripTimeout = 0;
                                        ncommand.timeoutTime = int.MaxValue;
                                        ncommand.commandSentTime = this.timeInt;
                                    }
                                }
                            }
                        }
                        bool flag5 = this.udpCommandCount <= 0;
                        if (flag5)
                        {
                            result = false;
                        }
                        else
                        {
                            bool trafficStatsEnabled = base.TrafficStatsEnabled;
                            if (trafficStatsEnabled)
                            {
                                TrafficStats trafficStatsOutgoing = base.TrafficStatsOutgoing;
                                int totalPacketCount = trafficStatsOutgoing.TotalPacketCount;
                                trafficStatsOutgoing.TotalPacketCount = totalPacketCount + 1;
                                base.TrafficStatsOutgoing.TotalCommandsInPackets += (int)this.udpCommandCount;
                            }
                            this.SendData(this.udpBuffer, this.udpBufferIndex);
                            result = (num > 0);
                        }
                    }
                }
            }
            return result;
        }

        internal void SendData(byte[] data, int length)
        {
            try
            {
                bool flag = this.datagramEncryptedConnection;
                if (flag)
                {
                    this.SendDataEncrypted(data, length);
                }
                else
                {
                    int num = 0;
                    Protocol.Serialize(this.peerID, data, ref num);
                    data[2] = (byte)(this.photonPeer.CrcEnabled ? 204 : 0);
                    data[3] = this.udpCommandCount;
                    num = 4;
                    Protocol.Serialize(this.timeInt, data, ref num);
                    Protocol.Serialize(this.challenge, data, ref num);
                    bool crcEnabled = this.photonPeer.CrcEnabled;
                    if (crcEnabled)
                    {
                        Protocol.Serialize(0, data, ref num);
                        uint value = SupportClass.CalculateCrc(data, length);
                        num -= 4;
                        Protocol.Serialize((int)value, data, ref num);
                    }
                    this.bytesOut += (long)length;
                    this.SendToSocket(data, length);
                }
            }
            catch (Exception ex)
            {
                bool flag2 = base.debugOut >= DebugLevel.Error;
                if (flag2)
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
                    byte[] obj = this.udpBuffer;
                    lock (obj)
                    {
                        int num = 0;
                        this.udpBufferIndex = this.CalculateInitialOffset();
                        this.udpBufferLength = this.CalculateBufferLen();
                        this.udpCommandCount = 0;
                        this.timeInt = SupportClass.GetTickCount() - this.timeBase;
                        this.timeLastSendOutgoing = this.timeInt;
                        StreamBuffer obj2 = this.outgoingAcknowledgementsPool;
                        lock (obj2)
                        {
                            bool flag3 = this.outgoingAcknowledgementsPool.Length > 0;
                            if (flag3)
                            {
                                num = this.SerializeAckToBuffer();
                                this.timeLastSendAck = this.timeInt;
                            }
                        }
                        bool flag4 = !base.IsSendingOnlyAcks && this.timeInt > this.timeoutInt && this.sentReliableCommands.Count > 0;
                        if (flag4)
                        {
                            List<NCommand> obj3 = this.sentReliableCommands;
                            lock (obj3)
                            {
                                this.commandsToResend.Clear();
                                for (int i = 0; i < this.sentReliableCommands.Count; i++)
                                {
                                    NCommand ncommand = this.sentReliableCommands[i];
                                    bool flag5 = ncommand != null && this.timeInt - ncommand.commandSentTime > ncommand.roundTripTimeout;
                                    if (flag5)
                                    {
                                        bool flag6 = (int)ncommand.commandSentCount > this.photonPeer.SentCountAllowance || this.timeInt > ncommand.timeoutTime;
                                        if (flag6)
                                        {
                                            bool flag7 = base.debugOut >= DebugLevel.Warning;
                                            if (flag7)
                                            {
                                                base.Listener.DebugReturn(DebugLevel.Warning, string.Concat(new object[]
                                                {
                                                    "Timeout-disconnect! Command: ",
                                                    ncommand,
                                                    " now: ",
                                                    this.timeInt,
                                                    " challenge: ",
                                                    Convert.ToString(this.challenge, 16)
                                                }));
                                            }
                                            bool flag8 = this.CommandLog != null;
                                            if (flag8)
                                            {
                                                this.CommandLog.Enqueue(new CmdLogSentReliable(ncommand, this.timeInt, this.roundTripTime, this.roundTripTimeVariance, true));
                                                base.CommandLogResize();
                                            }
                                            this.peerConnectionState = ConnectionStateValue.Zombie;
                                            base.EnqueueStatusCallback(StatusCode.TimeoutDisconnect);
                                            this.Disconnect();
                                            return false;
                                        }
                                        this.commandsToResend.Enqueue(ncommand);
                                    }
                                }
                                while (this.commandsToResend.Count > 0)
                                {
                                    NCommand ncommand2 = this.commandsToResend.Dequeue();
                                    this.QueueOutgoingReliableCommand(ncommand2);
                                    this.sentReliableCommands.Remove(ncommand2);
                                    this.reliableCommandsRepeated++;
                                    bool flag9 = base.debugOut >= DebugLevel.Info;
                                    if (flag9)
                                    {
                                        base.Listener.DebugReturn(DebugLevel.Info, string.Format("Resending: {0}. times out after: {1} sent: {3} now: {2} rtt/var: {4}/{5} last recv: {6}", new object[]
                                        {
                                            ncommand2,
                                            ncommand2.roundTripTimeout,
                                            this.timeInt,
                                            ncommand2.commandSentTime,
                                            this.roundTripTime,
                                            this.roundTripTimeVariance,
                                            SupportClass.GetTickCount() - this.timestampOfLastReceive
                                        }));
                                    }
                                }
                            }
                        }
                        bool flag10 = !base.IsSendingOnlyAcks && this.peerConnectionState == ConnectionStateValue.Connected && base.timePingInterval > 0 && this.sentReliableCommands.Count == 0 && this.timeInt - this.timeLastAckReceive > base.timePingInterval && !this.AreReliableCommandsInTransit() && this.udpBufferIndex + 12 < this.udpBufferLength;
                        if (flag10)
                        {
                            NCommand ncommand3 = new NCommand(this, 5, null, byte.MaxValue);
                            this.QueueOutgoingReliableCommand(ncommand3);
                            bool trafficStatsEnabled = base.TrafficStatsEnabled;
                            if (trafficStatsEnabled)
                            {
                                base.TrafficStatsOutgoing.CountControlCommand(ncommand3.Size);
                            }
                        }
                        bool flag11 = !base.IsSendingOnlyAcks;
                        if (flag11)
                        {
                            EnetChannel[] obj4 = this.channelArray;
                            lock (obj4)
                            {
                                for (int j = 0; j < this.channelArray.Length; j++)
                                {
                                    EnetChannel enetChannel = this.channelArray[j];
                                    num += this.SerializeToBuffer(enetChannel.outgoingReliableCommandsList);
                                    num += this.SerializeToBuffer(enetChannel.outgoingUnreliableCommandsList);
                                }
                            }
                        }
                        bool flag12 = this.udpCommandCount <= 0;
                        if (flag12)
                        {
                            result = false;
                        }
                        else
                        {
                            bool trafficStatsEnabled2 = base.TrafficStatsEnabled;
                            if (trafficStatsEnabled2)
                            {
                                TrafficStats trafficStatsOutgoing = base.TrafficStatsOutgoing;
                                int totalPacketCount = trafficStatsOutgoing.TotalPacketCount;
                                trafficStatsOutgoing.TotalPacketCount = totalPacketCount + 1;
                                base.TrafficStatsOutgoing.TotalCommandsInPackets += (int)this.udpCommandCount;
                            }
                            this.SendData(this.udpBuffer, this.udpBufferIndex);
                            result = (num > 0);
                        }
                    }
                }
            }
            return result;
        }

        internal int SerializeAckToBuffer()
        {
            this.outgoingAcknowledgementsPool.Seek(0L, SeekOrigin.Begin);
            while (this.outgoingAcknowledgementsPool.Position + 20 <= this.outgoingAcknowledgementsPool.Length)
            {
                bool flag = this.udpBufferIndex + 20 > this.udpBufferLength;
                if (flag)
                {
                    bool flag2 = base.debugOut >= DebugLevel.Info;
                    if (flag2)
                    {
                        base.Listener.DebugReturn(DebugLevel.Info, string.Concat(new object[]
                        {
                            "UDP package is full. Commands in Package: ",
                            this.udpCommandCount,
                            ". bytes left in queue: ",
                            this.outgoingAcknowledgementsPool.Position
                        }));
                    }
                    break;
                }
                int srcOffset;
                byte[] bufferAndAdvance = this.outgoingAcknowledgementsPool.GetBufferAndAdvance(20, out srcOffset);
                Buffer.BlockCopy(bufferAndAdvance, srcOffset, this.udpBuffer, this.udpBufferIndex, 20);
                this.udpBufferIndex += 20;
                this.udpCommandCount += 1;
            }
            this.outgoingAcknowledgementsPool.Compact();
            this.outgoingAcknowledgementsPool.Position = this.outgoingAcknowledgementsPool.Length;
            return this.outgoingAcknowledgementsPool.Length / 20;
        }

        internal override StreamBuffer SerializeOperationToMessage(byte opCode, Dictionary<byte, object> parameters, EgMessageType messageType, bool encrypt)
        {
            encrypt = (encrypt && !this.datagramEncryptedConnection);
            StreamBuffer streamBuffer = PeerBase.MessageBufferPoolGet();
            streamBuffer.SetLength(0L);
            bool flag = !encrypt;
            if (flag)
            {
                streamBuffer.Write(EnetPeer.messageHeader, 0, EnetPeer.messageHeader.Length);
            }
            this.serializationProtocol.SerializeOperationRequest(streamBuffer, opCode, parameters, false);
            bool flag2 = encrypt;
            if (flag2)
            {
                byte[] array = this.CryptoProvider.Encrypt(streamBuffer.GetBuffer(), 0, streamBuffer.Length);
                streamBuffer.SetLength(0L);
                streamBuffer.Write(EnetPeer.messageHeader, 0, EnetPeer.messageHeader.Length);
                streamBuffer.Write(array, 0, array.Length);
            }
            byte[] buffer = streamBuffer.GetBuffer();
            bool flag3 = messageType != EgMessageType.Operation;
            if (flag3)
            {
                buffer[EnetPeer.messageHeader.Length - 1] = (byte)messageType;
            }
            bool flag4 = encrypt;
            if (flag4)
            {
                buffer[EnetPeer.messageHeader.Length - 1] = (byte)(buffer[EnetPeer.messageHeader.Length - 1] | 128);
            }
            return streamBuffer;
        }

        internal int SerializeToBuffer(Queue<NCommand> commandList)
        {
            while (commandList.Count > 0)
            {
                NCommand ncommand = commandList.Peek();
                bool flag = ncommand == null;
                if (flag)
                {
                    commandList.Dequeue();
                }
                else
                {
                    bool flag2 = this.udpBufferIndex + ncommand.Size > this.udpBufferLength;
                    if (flag2)
                    {
                        bool flag3 = base.debugOut >= DebugLevel.Info;
                        if (flag3)
                        {
                            base.Listener.DebugReturn(DebugLevel.Info, string.Concat(new object[]
                            {
                                "UDP package is full. Commands in Package: ",
                                this.udpCommandCount,
                                ". Commands left in queue: ",
                                commandList.Count
                            }));
                        }
                        break;
                    }
                    ncommand.SerializeHeader(this.udpBuffer, ref this.udpBufferIndex);
                    bool flag4 = ncommand.SizeOfPayload > 0;
                    if (flag4)
                    {
                        Buffer.BlockCopy(ncommand.Serialize(), 0, this.udpBuffer, this.udpBufferIndex, ncommand.SizeOfPayload);
                        this.udpBufferIndex += ncommand.SizeOfPayload;
                    }
                    this.udpCommandCount += 1;
                    bool isFlaggedReliable = ncommand.IsFlaggedReliable;
                    if (isFlaggedReliable)
                    {
                        this.QueueSentCommand(ncommand);
                        bool flag5 = this.CommandLog != null;
                        if (flag5)
                        {
                            this.CommandLog.Enqueue(new CmdLogSentReliable(ncommand, this.timeInt, this.roundTripTime, this.roundTripTimeVariance, false));
                            base.CommandLogResize();
                        }
                    }
                    else
                    {
                        ncommand.FreePayload();
                    }
                    commandList.Dequeue();
                }
            }
            return commandList.Count;
        }

        internal override void StopConnection()
        {
            bool flag = this.PhotonSocket != null;
            if (flag)
            {
                this.PhotonSocket.Disconnect();
            }
            this.peerConnectionState = ConnectionStateValue.Disconnected;
            bool flag2 = base.Listener != null;
            if (flag2)
            {
                base.Listener.OnStatusChanged(StatusCode.Disconnect);
            }
        }

        public override void OnConnect()
        {
            this.QueueOutgoingReliableCommand(new NCommand(this, 2, null, byte.MaxValue));
        }
    }
}