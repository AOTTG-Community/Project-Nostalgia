// Decompiled with JetBrains decompiler
// Type: ExitGames.Client.Photon.Lite.LiteOpCode
// Assembly: Photon3Unity3D, Version=4.0.0.4, Culture=neutral, PublicKeyToken=null
// MVID: 45094ECB-AE08-4341-BE26-E524C8B0C677
// Assembly location: D:\Development\Attack on Titan\Assemblies\Photon3Unity3D.dll

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
