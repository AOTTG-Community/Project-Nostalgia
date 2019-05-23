using System.Collections;
using System.Collections.Generic;

namespace ExitGames.Client.Photon
{
  public class OperationResponse
  {
    public byte OperationCode;
    public short ReturnCode;
    public string DebugMessage;
    public Dictionary<byte, object> Parameters;

    public object this[byte parameterCode]
    {
      get
      {
        object obj;
        this.Parameters.TryGetValue(parameterCode, out obj);
        return obj;
      }
      set
      {
        this.Parameters[parameterCode] = value;
      }
    }

    public override string ToString()
    {
      return string.Format("OperationResponse {0}: ReturnCode: {1}.", (object) this.OperationCode, (object) this.ReturnCode);
    }

    public string ToStringFull()
    {
      return string.Format("OperationResponse {0}: ReturnCode: {1} ({3}). Parameters: {2}", (object) this.OperationCode, (object) this.ReturnCode, (object) SupportClass.DictionaryToString((IDictionary) this.Parameters), (object) this.DebugMessage);
    }
  }
}
