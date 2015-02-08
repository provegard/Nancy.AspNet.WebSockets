/* Copyright 2015 Per Rovegard
   Licensed under the MIT license. See LICENSE file in the root of the repo for the full license. */
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