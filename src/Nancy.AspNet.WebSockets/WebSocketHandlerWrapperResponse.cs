namespace Nancy.AspNet.WebSockets
{
    public class WebSocketHandlerWrapperResponse : Response
    {
        public IWebSocketHandler Handler { get; private set; }

        public WebSocketHandlerWrapperResponse(IWebSocketHandler handler)
        {
            Handler = handler;
        }
    }
}