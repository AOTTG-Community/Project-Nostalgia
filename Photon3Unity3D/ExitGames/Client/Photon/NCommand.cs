using System;

namespace ExitGames.Client.Photon
{
    internal class NCommand : IComparable<NCommand>
    {
        private byte[] commandHeader;
        internal const int CmdSizeAck = 20;
        internal const int CmdSizeConnect = 44;
        internal const int CmdSizeDisconnect = 12;
        internal const int CmdSizeFragmentHeader = 32;
        internal const int CmdSizeMaxHeader = 36;
        internal const int CmdSizeMinimum = 12;
        internal const int CmdSizePing = 12;
        internal const int CmdSizeReliableHeader = 12;
        internal const int CmdSizeUnreliableHeader = 16;
        internal const int CmdSizeUnsequensedHeader = 16;
        internal const int CmdSizeVerifyConnect = 44;
        internal const byte CT_ACK = 1;
        internal const byte CT_CONNECT = 2;
        internal const byte CT_DISCONNECT = 4;
        internal const byte CT_EG_ACK_UNSEQUENCED = 16;
        internal const byte CT_EG_SEND_FRAGMENT_UNSEQUENCED = 15;
        internal const byte CT_EG_SEND_RELIABLE_UNSEQUENCED = 14;
        internal const byte CT_EG_SEND_UNRELIABLE_PROCESSED = 13;
        internal const byte CT_EG_SERVERTIME = 12;
        internal const byte CT_NONE = 0;
        internal const byte CT_PING = 5;
        internal const byte CT_SENDFRAGMENT = 8;
        internal const byte CT_SENDRELIABLE = 6;
        internal const byte CT_SENDUNRELIABLE = 7;
        internal const byte CT_SENDUNSEQUENCED = 11;
        internal const byte CT_VERIFYCONNECT = 3;
        internal const byte FV_RELIABLE = 1;
        internal const byte FV_RELIBALE_UNSEQUENCED = 3;
        internal const byte FV_UNRELIABLE = 0;
        internal const byte FV_UNRELIABLE_UNSEQUENCED = 2;
        internal const int HEADER_UDP_PACK_LENGTH = 12;
        internal int ackReceivedReliableSequenceNumber;
        internal int ackReceivedSentTime;
        internal byte commandChannelID;
        internal byte commandFlags;
        internal byte commandSentCount;
        internal int commandSentTime;
        internal byte commandType;
        internal int fragmentCount;
        internal int fragmentNumber;
        internal int fragmentOffset;
        internal int fragmentsRemaining;
        internal StreamBuffer Payload;
        internal int reliableSequenceNumber;

        internal byte reservedByte = 4;
        internal int roundTripTimeout;
        internal int Size;
        internal int SizeOfHeader;
        internal int startSequenceNumber;
        internal int timeoutTime;
        internal int totalLength;
        internal int unreliableSequenceNumber;

        internal int unsequencedGroupNumber;

        internal NCommand(EnetPeer peer, byte commandType, StreamBuffer payload, byte channel)
        {
            this.commandType = commandType;
            this.commandFlags = 1;
            this.commandChannelID = channel;
            this.Payload = payload;
            this.Size = 12;
            switch (this.commandType)
            {
                case 2:
                    {
                        this.Size = 44;
                        byte[] array = new byte[32];
                        array[0] = 0;
                        array[1] = 0;
                        int num = 2;
                        Protocol.Serialize((short)peer.mtu, array, ref num);
                        array[4] = 0;
                        array[5] = 0;
                        array[6] = 128;
                        array[7] = 0;
                        array[11] = 254;
                        array[15] = 0;
                        array[19] = 0;
                        array[22] = 19;
                        array[23] = 136;
                        array[27] = 2;
                        array[31] = 2;
                        this.Payload = new StreamBuffer(array);
                        break;
                    }
                case 4:
                    {
                        this.Size = 12;
                        bool flag = peer.peerConnectionState != ConnectionStateValue.Connected;
                        if (flag)
                        {
                            this.commandFlags = 2;
                            bool flag2 = peer.peerConnectionState == ConnectionStateValue.Zombie;
                            if (flag2)
                            {
                                this.reservedByte = 2;
                            }
                        }
                        break;
                    }
                case 6:
                    this.Size = 12 + payload.Length;
                    break;

                case 7:
                    this.Size = 16 + payload.Length;
                    this.commandFlags = 0;
                    break;

                case 8:
                    this.Size = 32 + payload.Length;
                    break;

                case 11:
                    this.Size = 16 + payload.Length;
                    this.commandFlags = 2;
                    break;

                case 14:
                    this.Size = 12 + payload.Length;
                    this.commandFlags = 3;
                    break;

                case 15:
                    this.Size = 32 + payload.Length;
                    this.commandFlags = 3;
                    break;
            }
        }

        internal NCommand(EnetPeer peer, byte[] inBuff, ref int readingOffset)
        {
            int num = readingOffset;
            readingOffset = num + 1;
            this.commandType = inBuff[num];
            num = readingOffset;
            readingOffset = num + 1;
            this.commandChannelID = inBuff[num];
            num = readingOffset;
            readingOffset = num + 1;
            this.commandFlags = inBuff[num];
            num = readingOffset;
            readingOffset = num + 1;
            this.reservedByte = inBuff[num];
            Protocol.Deserialize(out this.Size, inBuff, ref readingOffset);
            Protocol.Deserialize(out this.reliableSequenceNumber, inBuff, ref readingOffset);
            peer.bytesIn += (long)this.Size;
            int num2 = 0;
            switch (this.commandType)
            {
                case 1:
                case 16:
                    Protocol.Deserialize(out this.ackReceivedReliableSequenceNumber, inBuff, ref readingOffset);
                    Protocol.Deserialize(out this.ackReceivedSentTime, inBuff, ref readingOffset);
                    break;

                case 3:
                    {
                        short peerID;
                        Protocol.Deserialize(out peerID, inBuff, ref readingOffset);
                        readingOffset += 30;
                        bool flag = peer.peerID == -1 || peer.peerID == -2;
                        if (flag)
                        {
                            peer.peerID = peerID;
                        }
                        break;
                    }
                case 6:
                case 14:
                    num2 = this.Size - 12;
                    break;

                case 7:
                    Protocol.Deserialize(out this.unreliableSequenceNumber, inBuff, ref readingOffset);
                    num2 = this.Size - 16;
                    break;

                case 8:
                case 15:
                    Protocol.Deserialize(out this.startSequenceNumber, inBuff, ref readingOffset);
                    Protocol.Deserialize(out this.fragmentCount, inBuff, ref readingOffset);
                    Protocol.Deserialize(out this.fragmentNumber, inBuff, ref readingOffset);
                    Protocol.Deserialize(out this.totalLength, inBuff, ref readingOffset);
                    Protocol.Deserialize(out this.fragmentOffset, inBuff, ref readingOffset);
                    num2 = this.Size - 32;
                    this.fragmentsRemaining = this.fragmentCount;
                    break;

                case 11:
                    Protocol.Deserialize(out this.unsequencedGroupNumber, inBuff, ref readingOffset);
                    num2 = this.Size - 16;
                    break;
            }
            bool flag2 = num2 != 0;
            if (flag2)
            {
                StreamBuffer streamBuffer = PeerBase.MessageBufferPoolGet();
                streamBuffer.Write(inBuff, readingOffset, num2);
                this.Payload = streamBuffer;
                this.Payload.Position = 0;
                readingOffset += num2;
            }
        }

        protected internal bool IsFlaggedReliable
        {
            get
            {
                return (this.commandFlags & 1) > 0;
            }
        }

        protected internal bool IsFlaggedUnsequenced
        {
            get
            {
                return (this.commandFlags & 2) > 0;
            }
        }

        protected internal int SizeOfPayload
        {
            get
            {
                return (this.Payload != null) ? this.Payload.Length : 0;
            }
        }

        internal static void CreateAck(byte[] buffer, int offset, NCommand commandToAck, int sentTime)
        {
            buffer[offset++] = (byte)(commandToAck.IsFlaggedUnsequenced ? 16 : 1);
            buffer[offset++] = commandToAck.commandChannelID;
            buffer[offset++] = 0;
            buffer[offset++] = commandToAck.reservedByte;
            Protocol.Serialize(20, buffer, ref offset);
            Protocol.Serialize(0, buffer, ref offset);
            Protocol.Serialize(commandToAck.reliableSequenceNumber, buffer, ref offset);
            Protocol.Serialize(sentTime, buffer, ref offset);
        }

        internal byte[] Serialize()
        {
            return this.Payload.GetBuffer();
        }

        internal void SerializeHeader(byte[] buffer, ref int bufferIndex)
        {
            bool flag = this.commandHeader != null;
            if (!flag)
            {
                this.SizeOfHeader = 12;
                bool flag2 = this.commandType == 7;
                if (flag2)
                {
                    this.SizeOfHeader = 16;
                }
                else
                {
                    bool flag3 = this.commandType == 11;
                    if (flag3)
                    {
                        this.SizeOfHeader = 16;
                    }
                    else
                    {
                        bool flag4 = this.commandType == 8 || this.commandType == 15;
                        if (flag4)
                        {
                            this.SizeOfHeader = 32;
                        }
                    }
                }
                int num = bufferIndex;
                bufferIndex = num + 1;
                buffer[num] = this.commandType;
                num = bufferIndex;
                bufferIndex = num + 1;
                buffer[num] = this.commandChannelID;
                num = bufferIndex;
                bufferIndex = num + 1;
                buffer[num] = this.commandFlags;
                num = bufferIndex;
                bufferIndex = num + 1;
                buffer[num] = this.reservedByte;
                Protocol.Serialize(this.Size, buffer, ref bufferIndex);
                Protocol.Serialize(this.reliableSequenceNumber, buffer, ref bufferIndex);
                bool flag5 = this.commandType == 7;
                if (flag5)
                {
                    Protocol.Serialize(this.unreliableSequenceNumber, buffer, ref bufferIndex);
                }
                else
                {
                    bool flag6 = this.commandType == 11;
                    if (flag6)
                    {
                        Protocol.Serialize(this.unsequencedGroupNumber, buffer, ref bufferIndex);
                    }
                    else
                    {
                        bool flag7 = this.commandType == 8 || this.commandType == 15;
                        if (flag7)
                        {
                            Protocol.Serialize(this.startSequenceNumber, buffer, ref bufferIndex);
                            Protocol.Serialize(this.fragmentCount, buffer, ref bufferIndex);
                            Protocol.Serialize(this.fragmentNumber, buffer, ref bufferIndex);
                            Protocol.Serialize(this.totalLength, buffer, ref bufferIndex);
                            Protocol.Serialize(this.fragmentOffset, buffer, ref bufferIndex);
                        }
                    }
                }
            }
        }

        public int CompareTo(NCommand other)
        {
            bool flag = other == null;
            int result;
            if (flag)
            {
                result = 1;
            }
            else
            {
                int num = this.reliableSequenceNumber - other.reliableSequenceNumber;
                bool flag2 = this.IsFlaggedReliable || num != 0;
                if (flag2)
                {
                    result = num;
                }
                else
                {
                    result = this.unreliableSequenceNumber - other.unreliableSequenceNumber;
                }
            }
            return result;
        }

        public void FreePayload()
        {
            bool flag = this.Payload != null;
            if (flag)
            {
                PeerBase.MessageBufferPoolPut(this.Payload);
            }
            this.Payload = null;
        }

        public override string ToString()
        {
            bool flag = this.commandType == 1 || this.commandType == 16;
            string result;
            if (flag)
            {
                result = string.Format("CMD({1} ack for c#:{0} s#/time {2}/{3})", new object[]
                {
                    this.commandChannelID,
                    this.commandType,
                    this.ackReceivedReliableSequenceNumber,
                    this.ackReceivedSentTime
                });
            }
            else
            {
                result = string.Format("CMD({1} c#:{0} r/u: {2}/{3} st/r#/rt:{4}/{5}/{6})", new object[]
                {
                    this.commandChannelID,
                    this.commandType,
                    this.reliableSequenceNumber,
                    this.unreliableSequenceNumber,
                    this.commandSentTime,
                    this.commandSentCount,
                    this.timeoutTime
                });
            }
            return result;
        }
    }
}