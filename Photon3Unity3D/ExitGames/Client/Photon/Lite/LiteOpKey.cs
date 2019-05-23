using System;

namespace ExitGames.Client.Photon.Lite
{
  public static class LiteOpKey
  {
    [Obsolete("Use GameId")]
    public const byte Asid = 255;
    [Obsolete("Use GameId")]
    public const byte RoomName = 255;
    public const byte GameId = 255;
    public const byte ActorNr = 254;
    public const byte TargetActorNr = 253;
    public const byte ActorList = 252;
    public const byte Properties = 251;
    public const byte Broadcast = 250;
    public const byte ActorProperties = 249;
    public const byte GameProperties = 248;
    public const byte Cache = 247;
    public const byte ReceiverGroup = 246;
    public const byte Data = 245;
    public const byte Code = 244;
    public const byte Group = 240;
    public const byte Remove = 239;
    public const byte Add = 238;
  }
}
