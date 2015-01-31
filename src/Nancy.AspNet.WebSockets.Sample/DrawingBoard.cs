using System.Collections.Concurrent;

namespace Nancy.AspNet.WebSockets.Sample
{
    public class DrawingBoard
    {
        private readonly string _name;
        private readonly BlockingCollection<Handler> _handlers = new BlockingCollection<Handler>(); 

        public DrawingBoard(string name)
        {
            _name = name;
        }

        public IWebSocketHandler Register(string user)
        {
            var handler = new Handler(this, user);
            _handlers.Add(handler);
            return handler;
        }

        public class Handler : IWebSocketHandler
        {
            private readonly DrawingBoard _drawingBoard;
            private readonly string _user;
            private IWebSocketClient _client;

            public Handler(DrawingBoard drawingBoard, string user)
            {
                _drawingBoard = drawingBoard;
                _user = user;
            }

            private void SendToAll(string message)
            {
                foreach (var handler in _drawingBoard._handlers)
                {
                    if (handler != this)
                    {
                        handler._client.Send(message);
                    }
                }
            }

            private void SendToAll(byte[] message)
            {
                foreach (var handler in _drawingBoard._handlers)
                {
                    if (handler != this)
                    {
                        handler._client.Send(message);
                    }
                }
            }

            public void OnOpen(IWebSocketClient client)
            {
                _client = client;
                SendToAll(string.Format("User {0} connected to drawing board {1}", _user,
                    _drawingBoard._name));
            }

            public void OnMessage(string message)
            {
                SendToAll(string.Format("User {0} says: {1}", _user, message));
            }

            public void OnMessage(byte[] message)
            {
                SendToAll(message);
            }

            public void OnClose()
            {
                SendToAll(string.Format("User {0} disconnected from drawing board {1}", _user,
                    _drawingBoard._name));
            }

            public void OnError()
            {
            }
        }
    }
}