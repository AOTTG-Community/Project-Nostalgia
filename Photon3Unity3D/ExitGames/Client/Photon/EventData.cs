using System.Collections.Generic;

namespace ExitGames.Client.Photon
{
    public class EventData
    {
        private static byte CustomDataKey = 245;
        private static byte SenderKey = 254;
        private int sender = -1;
        public byte Code;

        public Dictionary<byte, object> Parameters;

        public object CustomData
        {
            get
            {
                return this[EventData.CustomDataKey];
            }
        }

        public int Sender
        {
            get
            {
                bool flag = this.sender >= 0;
                int result;
                if (flag)
                {
                    result = this.sender;
                }
                else
                {
                    this.sender = 0;
                    object obj = this[EventData.SenderKey];
                    bool flag2 = obj is int;
                    if (flag2)
                    {
                        this.sender = (int)obj;
                    }
                    result = this.sender;
                }
                return result;
            }
        }

        public object this[byte key]
        {
            get
            {
                object result;
                this.Parameters.TryGetValue(key, out result);
                return result;
            }
            set
            {
                this.Parameters[key] = value;
            }
        }

        public override string ToString()
        {
            return string.Format("Event {0}.", this.Code.ToString());
        }

        public string ToStringFull()
        {
            return string.Format("Event {0}: {1}", this.Code, SupportClass.DictionaryToString(this.Parameters));
        }
    }
}