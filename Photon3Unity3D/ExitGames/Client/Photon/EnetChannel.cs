using System.Collections.Generic;

namespace ExitGames.Client.Photon
{
    internal class EnetChannel
    {
        private int reliableUnsequencedNumbersCompletelyReceived;
        private HashSet<int> reliableUnsequencedNumbersReceived = new HashSet<int>();
        internal byte ChannelNumber;

        internal Dictionary<int, NCommand> incomingReliableCommandsList;

        internal int incomingReliableSequenceNumber;
        internal Dictionary<int, NCommand> incomingUnreliableCommandsList;

        internal int incomingUnreliableSequenceNumber;
        internal Queue<NCommand> incomingUnsequencedCommandsList;

        internal Dictionary<int, NCommand> incomingUnsequencedFragments;

        internal Queue<NCommand> outgoingReliableCommandsList;

        internal int outgoingReliableSequenceNumber;
        internal int outgoingReliableUnsequencedNumber;
        internal Queue<NCommand> outgoingUnreliableCommandsList;
        internal int outgoingUnreliableSequenceNumber;

        public EnetChannel(byte channelNumber, int commandBufferSize)
        {
            this.ChannelNumber = channelNumber;
            this.incomingReliableCommandsList = new Dictionary<int, NCommand>(commandBufferSize);
            this.incomingUnreliableCommandsList = new Dictionary<int, NCommand>(commandBufferSize);
            this.incomingUnsequencedCommandsList = new Queue<NCommand>();
            this.incomingUnsequencedFragments = new Dictionary<int, NCommand>();
            this.outgoingReliableCommandsList = new Queue<NCommand>(commandBufferSize);
            this.outgoingUnreliableCommandsList = new Queue<NCommand>(commandBufferSize);
        }

        public void clearAll()
        {
            lock (this)
            {
                this.incomingReliableCommandsList.Clear();
                this.incomingUnreliableCommandsList.Clear();
                this.incomingUnsequencedCommandsList.Clear();
                this.incomingUnsequencedFragments.Clear();
                this.outgoingReliableCommandsList.Clear();
                this.outgoingUnreliableCommandsList.Clear();
            }
        }

        public bool ContainsReliableSequenceNumber(int reliableSequenceNumber)
        {
            return this.incomingReliableCommandsList.ContainsKey(reliableSequenceNumber);
        }

        public bool ContainsUnreliableSequenceNumber(int unreliableSequenceNumber)
        {
            return this.incomingUnreliableCommandsList.ContainsKey(unreliableSequenceNumber);
        }

        public NCommand FetchReliableSequenceNumber(int reliableSequenceNumber)
        {
            return this.incomingReliableCommandsList[reliableSequenceNumber];
        }

        public NCommand FetchUnreliableSequenceNumber(int unreliableSequenceNumber)
        {
            return this.incomingUnreliableCommandsList[unreliableSequenceNumber];
        }

        public bool QueueIncomingReliableUnsequenced(NCommand command)
        {
            bool flag = command.reliableSequenceNumber <= this.reliableUnsequencedNumbersCompletelyReceived;
            bool result;
            if (flag)
            {
                result = false;
            }
            else
            {
                bool flag2 = this.reliableUnsequencedNumbersReceived.Contains(command.reliableSequenceNumber);
                if (flag2)
                {
                    result = false;
                }
                else
                {
                    bool flag3 = command.reliableSequenceNumber == this.reliableUnsequencedNumbersCompletelyReceived + 1;
                    if (flag3)
                    {
                        this.reliableUnsequencedNumbersCompletelyReceived++;
                    }
                    else
                    {
                        this.reliableUnsequencedNumbersReceived.Add(command.reliableSequenceNumber);
                    }
                    while (this.reliableUnsequencedNumbersReceived.Contains(this.reliableUnsequencedNumbersCompletelyReceived + 1))
                    {
                        this.reliableUnsequencedNumbersCompletelyReceived++;
                        this.reliableUnsequencedNumbersReceived.Remove(this.reliableUnsequencedNumbersCompletelyReceived);
                    }
                    bool flag4 = command.commandType == 15;
                    if (flag4)
                    {
                        this.incomingUnsequencedFragments.Add(command.reliableSequenceNumber, command);
                    }
                    else
                    {
                        this.incomingUnsequencedCommandsList.Enqueue(command);
                    }
                    result = true;
                }
            }
            return result;
        }

        public void RemoveFragment(int reliableSequenceNumber, bool isSequenced)
        {
            if (isSequenced)
            {
                this.incomingReliableCommandsList.Remove(reliableSequenceNumber);
            }
            else
            {
                this.incomingUnsequencedFragments.Remove(reliableSequenceNumber);
            }
        }

        public bool TryGetFragment(int reliableSequenceNumber, bool isSequenced, out NCommand fragment)
        {
            bool result;
            if (isSequenced)
            {
                result = this.incomingReliableCommandsList.TryGetValue(reliableSequenceNumber, out fragment);
            }
            else
            {
                result = this.incomingUnsequencedFragments.TryGetValue(reliableSequenceNumber, out fragment);
            }
            return result;
        }
    }
}