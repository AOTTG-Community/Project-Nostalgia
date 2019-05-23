// Decompiled with JetBrains decompiler
// Type: ExitGames.Client.Photon.CustomType
// Assembly: Photon3Unity3D, Version=4.0.0.4, Culture=neutral, PublicKeyToken=null
// MVID: 45094ECB-AE08-4341-BE26-E524C8B0C677
// Assembly location: D:\Development\Attack on Titan\Assemblies\Photon3Unity3D.dll

using System;

namespace ExitGames.Client.Photon
{
  internal class CustomType
  {
    public readonly byte Code;
    public readonly Type Type;
    public readonly SerializeMethod SerializeFunction;
    public readonly DeserializeMethod DeserializeFunction;

    public CustomType(Type type, byte code, SerializeMethod serializeFunction, DeserializeMethod deserializeFunction)
    {
      this.Type = type;
      this.Code = code;
      this.SerializeFunction = serializeFunction;
      this.DeserializeFunction = deserializeFunction;
    }
  }
}
