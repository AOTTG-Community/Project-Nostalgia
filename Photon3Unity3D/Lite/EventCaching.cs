// Decompiled with JetBrains decompiler
// Type: ExitGames.Client.Photon.Lite.EventCaching
// Assembly: Photon3Unity3D, Version=4.0.0.4, Culture=neutral, PublicKeyToken=null
// MVID: 45094ECB-AE08-4341-BE26-E524C8B0C677
// Assembly location: D:\Development\Attack on Titan\Assemblies\Photon3Unity3D.dll

using System;

namespace ExitGames.Client.Photon.Lite
{
  public enum EventCaching : byte
  {
    DoNotCache = 0,
    [Obsolete] MergeCache = 1,
    [Obsolete] ReplaceCache = 2,
    [Obsolete] RemoveCache = 3,
    AddToRoomCache = 4,
    AddToRoomCacheGlobal = 5,
    RemoveFromRoomCache = 6,
    RemoveFromRoomCacheForActorsLeft = 7,
    SliceIncreaseIndex = 10,
    SliceSetIndex = 11,
    SlicePurgeIndex = 12,
    SlicePurgeUpToIndex = 13,
  }
}
