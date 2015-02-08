/* Copyright 2015 Per Rovegard
   Licensed under the MIT license. See LICENSE file in the root of the repo for the full license. */
namespace Nancy.AspNet.WebSockets
{
    public static class Constants
    {
        /// <summary>
        /// Used as header name to mark a request as being a WebSocket request. Note that this is
        /// not a valid header name according to RFC 2616, and thus it shouldn't be possible to
        /// spoof it.
        /// </summary>
        public const string WebsocketIndicatorHeader = "__[wsr]__";
    }
}