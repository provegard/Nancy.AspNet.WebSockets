/* Copyright 2015 Per Rovegard
   Licensed under the MIT license. See LICENSE file in the root of the repo for the full license. */
using System;
using System.Collections.Concurrent;
using Nancy.Testing;

namespace Nancy.AspNet.WebSockets.Testing
{
    /// <summary>
    /// Represents a WebSocket connection as seen from the client side. An instance of this class
    /// is used in tests that use the <see cref="BrowserExtensions.OpenWebSocket">OpenWebSocket</see>
    /// method.
    /// </summary>
    public class WebSocket
    {
        private readonly BlockingCollection<Action<IWebSocketHandler>> _forServer;
        private bool _isOpen;

        internal WebSocket(BlockingCollection<Action<IWebSocketHandler>> forServer)
        {
            _forServer = forServer;
            MaxAge = TimeSpan.FromMilliseconds(2000);
        }

        public event EventHandler<MessageEventArgs> MessageReceived;
        public event EventHandler<DataEventArgs> DataReceived;
        public event EventHandler Closed;
        public event EventHandler Opened;
        public event EventHandler<ErrorEventArgs> Error;

        public TimeSpan MaxAge { get; set; }

        public void Send(string message)
        {
            RequireOpen();
            _forServer.Add(h => h.OnMessage(message));
        }

        public void Send(byte[] data)
        {
            RequireOpen();
            _forServer.Add(h => h.OnData(data));
        }

        public void Close()
        {
            RequireOpen();
            RaiseClosed();
            _forServer.Add(h => h.OnClose());
            _forServer.CompleteAdding();
        }

        private void RequireOpen()
        {
            if (!_isOpen)
                throw new InvalidOperationException("Socket is not open");
        }

        internal void RaiseError(BrowserResponse resp)
        {
            if (Error != null)
                Error(this, new ErrorEventArgs(resp));
        }

        internal void RaiseMessageReceived(string message)
        {
            if (MessageReceived != null)
                MessageReceived(this, new MessageEventArgs(message));
        }

        internal void RaiseDataReceived(byte[] data)
        {
            if (DataReceived != null)
                DataReceived(this, new DataEventArgs(data));
        }

        internal void RaiseOpened()
        {
            _isOpen = true;
            if (Opened != null)
                Opened(this, new EventArgs());
        }

        internal void RaiseClosed()
        {
            if (Closed != null)
                Closed(this, new EventArgs());
        }
    }

    public class MessageEventArgs : EventArgs
    {
        public string Message { get; private set; }

        public MessageEventArgs(string message)
        {
            Message = message;
        }
    }

    public class DataEventArgs : EventArgs
    {
        private readonly byte[] _data;

        public byte[] Data
        {
            get { return (byte[])_data.Clone(); }
        }

        public DataEventArgs(byte[] data)
        {
            _data = (byte[])data.Clone();
        }
    }

    public class ErrorEventArgs : EventArgs
    {
        public BrowserResponse ErrorResponse { get; private set; }

        public ErrorEventArgs(BrowserResponse r)
        {
            ErrorResponse = r;
        }
    }

    public delegate void MessageEventHandler(object sender, MessageEventArgs args);

    public delegate void DataEventHandler(object sender, DataEventArgs args);

    public delegate void ErrorEventHandler(object sender, ErrorEventArgs args);

}