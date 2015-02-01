namespace Nancy.AspNet.WebSockets
{
    /// <summary>
    /// Defines methods for responding to client communication over a WebSocket connection. 
    /// </summary>
    public interface IWebSocketHandler
    {
        /// <summary>
        /// Invoked when a WebSocket connection is opened to a client.
        /// </summary>
        /// <param name="client">The connected client</param>
        void OnOpen(IWebSocketClient client);

        /// <summary>
        /// Invoked when the client sends a text message.
        /// </summary>
        /// <param name="message">The message from the client</param>
        void OnMessage(string message);

        /// <summary>
        /// Invoked when the client sends binary data
        /// </summary>
        /// <param name="data">The data sent by the client</param>
        void OnData(byte[] data);

        /// <summary>
        /// Invoked when the connection is closed.
        /// </summary>
        void OnClose();

        /// <summary>
        /// Invoked when an error occurs.
        /// </summary>
        void OnError();
    }
}
