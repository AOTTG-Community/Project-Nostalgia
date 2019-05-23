namespace ExitGames.Client.Photon
{
  public enum PeerStateValue : byte
  {
    Disconnected = 0,
    Connecting = 1,
    Connected = 3,
    Disconnecting = 4,
    InitializingApplication = 10,
  }
}
