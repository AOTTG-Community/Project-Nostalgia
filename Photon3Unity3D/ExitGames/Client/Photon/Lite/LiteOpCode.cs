using System;

namespace ExitGames.Client.Photon.Lite
{
  public static class LiteOpCode
  {
    [Obsolete("Exchanging encrpytion keys is done internally in the lib now. Don't expect this operation-result.")]
    public const byte ExchangeKeysForEncryption = 250;
    public const byte Join = 255;
    public const byte Leave = 254;
    public const byte RaiseEvent = 253;
    public const byte SetProperties = 252;
    public const byte GetProperties = 251;
    public const byte ChangeGroups = 248;
  }
}
