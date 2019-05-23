namespace ExitGames.Client.Photon
{
    public interface IPhotonPeerListener
    {
        void DebugReturn(DebugLevel level, string message);

        void OnEvent(EventData eventData);

        void OnOperationResponse(OperationResponse operationResponse);

        void OnStatusChanged(StatusCode statusCode);
    }
}