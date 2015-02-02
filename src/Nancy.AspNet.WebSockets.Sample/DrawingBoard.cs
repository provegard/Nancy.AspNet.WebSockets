using System;
using System.Collections.Generic;

namespace Nancy.AspNet.WebSockets.Sample
{
    public class DrawingBoard
    {
        private readonly string _name;
        private readonly IList<Handler> _handlers = new List<Handler>();
        private readonly object _guard = new object();

        public DrawingBoard(string name)
        {
            _name = name;
        }

        public IWebSocketHandler Register(string user)
        {
            var handler = new Handler(this, user);
            lock (_guard) _handlers.Add(handler);
            return handler;
        }

        internal void Deregister(Handler h)
        {
            lock (_guard) _handlers.Remove(h);
        }

        internal void ForEachHandler(Action<Handler> a)
        {
            lock (_guard)
            {
                foreach (var handler in _handlers)
                {
                    a(handler);
                }
            }
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
                _drawingBoard.ForEachHandler(handler =>
                {
                    if (handler != this)
                    {
                        handler._client.Send(message);
                    }
                });
            }

            private void SendToAll(byte[] message)
            {
                _drawingBoard.ForEachHandler(handler =>
                {
                    if (handler != this)
                    {
                        handler._client.Send(message);
                    }
                });
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

            public void OnData(byte[] message)
            {
                SendToAll(message);
            }

            public void OnClose()
            {
                SendToAll(string.Format("User {0} disconnected from drawing board {1}", _user,
                    _drawingBoard._name));
                _drawingBoard.Deregister(this);
            }

            public void OnError()
            {
            }
        }
    }
}