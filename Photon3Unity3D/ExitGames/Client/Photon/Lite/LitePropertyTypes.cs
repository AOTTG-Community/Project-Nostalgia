using System;

namespace ExitGames.Client.Photon.Lite
{
  [Flags]
  public enum LitePropertyTypes : byte
  {
    None = 0,
    Game = 1,
    Actor = 2,
    GameAndActor = Actor | Game,
  }
}
