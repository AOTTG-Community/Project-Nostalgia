// Decompiled with JetBrains decompiler
// Type: ExitGames.Client.Photon.IPhotonPeerListener
// Assembly: Photon3Unity3D, Version=4.0.0.4, Culture=neutral, PublicKeyToken=null
// MVID: 45094ECB-AE08-4341-BE26-E524C8B0C677
// Assembly location: D:\Development\Attack on Titan\Assemblies\Photon3Unity3D.dll

namespace ExitGames.Client.Photon
{
  public interface IPhotonPeerListener
  {
    void DebugReturn(DebugLevel level, string message);

    void OnOperationResponse(OperationResponse operationResponse);

    void OnStatusChanged(StatusCode statusCode);

    void OnEvent(EventData eventData);
  }
}
