﻿using System.Collections.Generic;

namespace ExitGames.Client.Photon
{
    public class OperationResponse
    {
        public string DebugMessage;
        public byte OperationCode;

        public Dictionary<byte, object> Parameters;
        public short ReturnCode;

        public object this[byte parameterCode]
        {
            get
            {
                object result;
                this.Parameters.TryGetValue(parameterCode, out result);
                return result;
            }
            set
            {
                this.Parameters[parameterCode] = value;
            }
        }

        public override string ToString()
        {
            return string.Format("OperationResponse {0}: ReturnCode: {1}.", this.OperationCode, this.ReturnCode);
        }

        public string ToStringFull()
        {
            return string.Format("OperationResponse {0}: ReturnCode: {1} ({3}). Parameters: {2}", new object[]
            {
                this.OperationCode,
                this.ReturnCode,
                SupportClass.DictionaryToString(this.Parameters),
                this.DebugMessage
            });
        }
    }
}