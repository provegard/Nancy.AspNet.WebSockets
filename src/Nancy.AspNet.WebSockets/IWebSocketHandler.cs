namespace Nancy.AspNet.WebSockets
{
    public interface IWebSocketHandler
    {
        void OnOpen(IWebSocketClient client);
        void OnMessage(string message);
        void OnMessage(byte[] message);
        void OnClose();
        void OnError();
    }
}
