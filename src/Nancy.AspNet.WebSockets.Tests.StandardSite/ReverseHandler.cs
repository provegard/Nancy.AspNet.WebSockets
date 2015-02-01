using System.Linq;

namespace Nancy.AspNet.WebSockets.Tests.StandardSite
{
    internal class ReverseHandler : IWebSocketHandler
    {
        private readonly ILog _log;
        private readonly string _clientName;
        private IWebSocketClient _client;

        public ReverseHandler(ILog log, string clientName)
        {
            _log = log;
            _clientName = clientName;
        }

        public void OnOpen(IWebSocketClient client)
        {
            _client = client;
        }

        public void OnMessage(string message)
        {
            var response = new string(message.Reverse().ToArray());
            _client.Send(response);
        }

        public void OnData(byte[] message)
        {
            var response = message.Reverse().ToArray();
            _client.Send(response);
        }

        public void OnClose()
        {
            _log.Log("client " + _clientName + " closed");
        }

        public void OnError()
        {
        }
    }
}