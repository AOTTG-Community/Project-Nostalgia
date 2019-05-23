// Decompiled with JetBrains decompiler
// Type: ExitGames.Client.Photon.Lite.LitePropertyTypes
// Assembly: Photon3Unity3D, Version=4.0.0.4, Culture=neutral, PublicKeyToken=null
// MVID: 45094ECB-AE08-4341-BE26-E524C8B0C677
// Assembly location: D:\Development\Attack on Titan\Assemblies\Photon3Unity3D.dll

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
