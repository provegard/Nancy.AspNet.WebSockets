using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Nancy.Testing;

namespace Nancy.AspNet.WebSockets.Testing
{
    public static class BrowserExtensions
    {
        /// <summary>
        /// Opens a WebSocket towards the specified path.
        /// </summary>
        /// <param name="browser">The Browser instance to use.</param>
        /// <param name="path">The server path.</param>
        /// <param name="setupSocket">WebSocket setup action. Will be called with a WebSocket instance, so that
        /// the test case can setup event handlers and communication with the server.</param>
        /// <param name="configure">Standard BrowserContext setup action. Will be passed to the Browser instance.
        /// </param>
        public static void OpenWebSocket(this Browser browser, string path, Action<WebSocket> setupSocket,
            Action<BrowserContext> configure = null)
        {
            // A WebSocket connection is setup in Nancy using a standard GET call but using a special
            // HTTP header.
            var browserResponse = browser.Get(path, ctx =>
            {
                // Do standard configuration, so that the test case can add query params, headers, etc.
                if (configure != null)
                    configure(ctx);

                // Indicate that this is a WebSocket connection
                ctx.Header(Constants.WebsocketIndicatorHeader, "***");
            });

            // Setup the collections that will be used to dispatch actions across client and server
            // threads.
            var forServer = new BlockingCollection<Action<IWebSocketHandler>>();
            var forClient = new BlockingCollection<Action<WebSocket>>();

            // Create the WebSocket instance and let the test case do socket setup.
            var socket = new WebSocket(forServer);
            setupSocket(socket);

            // Verify that MaxAge is valid.
            if (socket.MaxAge.TotalMilliseconds <= 0)
            {
                throw new InvalidOperationException("Max age must be greater than zero.");
            }

            // Create a CancellationTokenSource with a timeout. The default timeout is set in the WebSocket
            // constructor, but can be overridden through the MaxAge property.
            var cts = new CancellationTokenSource(socket.MaxAge);

            // Verify that we get the special IWebSocketHandler wrapper response. If not, raise the Error
            // event on the WebSocket instance with the response wrapper.
            var response = browserResponse.Context.Response as WebSocketHandlerWrapperResponse;
            if (response == null)
            {
                socket.RaiseError(browserResponse);
                return;
            }

            // Extract the IWebsocketHandler instance
            var handler = response.Handler;

            // The first call to the handler is the OnOpen call, where we pass an IWebSocketClient instance
            // that adds client calls to the 'forClient' collection. This simulates that a client has
            // connected to the server.
            forServer.Add(h => h.OnOpen(new ClientProxy(forClient)));
            
            // Start a dispatcher that consumes actions from the 'forServer' collection and invokes each
            // action on the IWebSocketHandler instance.
            var serverTask = Task.Factory.StartNew(() =>
            {
                foreach (var action in forServer.GetConsumingEnumerable(cts.Token))
                {
                    action(handler);
                }
            });

            // If 'forServer.CompleteAdding()' was called, the client initiated a socket close. The
            // following is needed to break out from the 'forClient' dispatcher below. It's a continuation
            // task so that it's run even when a 'forServer' action throws an exception.
            serverTask.ContinueWith(t => forClient.CompleteAdding());

            var ignoreExceptionFromServerTaskWait = false;
            try
            {
                // Raise the Opened event on the WebSocket instance, to simulate that the socket is now open.
                socket.RaiseOpened();

                // Dispatch actions destined for the client to the WebSocket.
                foreach (var action in forClient.GetConsumingEnumerable(cts.Token))
                {
                    action(socket);
                }

                // If 'forClient.CompleteAdding()' was called, the server disconnected the client. The
                // following is needed to break out from the server dispacher above.
                forServer.CompleteAdding();
            }
            catch
            {
                // We don't want any exception from the server task shadowing this exception
                ignoreExceptionFromServerTaskWait = true;
                throw;
            }
            finally
            {
                try
                {
                    // Finally wait for the server dispatcher thread to exit.
                    serverTask.Wait();
                }
                catch
                {
                    if (!ignoreExceptionFromServerTaskWait)
                        throw;
                }
            }
        }

        internal class ClientProxy : IWebSocketClient
        {
            private readonly BlockingCollection<Action<WebSocket>> _forClient;

            internal ClientProxy(BlockingCollection<Action<WebSocket>> forClient)
            {
                _forClient = forClient;
            }

            public void Send(string message)
            {
                _forClient.Add(s => s.RaiseMessageReceived(message));
            }

            public void Send(byte[] data)
            {
                _forClient.Add(s => s.RaiseDataReceived(data));
            }

            public void Close()
            {
                _forClient.Add(s => s.RaiseClosed());
                _forClient.CompleteAdding();
            }
        }
    }
}
