using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Hosting;
using Microsoft.Web.WebSockets;
using Nancy.AspNet.WebSockets.Config;
using Nancy.Hosting.Aspnet;

namespace Nancy.AspNet.WebSockets
{
    /// <summary>
    /// A special HTTP handler that handles both WebSocket requests and plain requests and dispatches to an
    /// inner Nancy handler. In the WebSocket request case, the request is annotated so that a special WebSocket
    /// route machinery can return a WebSocket handler, which subsequently is passed to Microsoft.WebSockets
    /// that will handle the client-server communication.
    /// </summary>
    public class WebSocketAwareHttpHandler : IHttpAsyncHandler
    {
        private readonly IHttpAsyncHandler _handler;

        public WebSocketAwareHttpHandler()
        {
            var path = HostingEnvironment.ApplicationVirtualPath;
            var webConfig = WebConfigurationManager.OpenWebConfiguration(path);

            // Parse WebConfig to a HandlerConfig object, then read the inner handler type.
            // We'll get a default if no custom handler has been specified.
            var handlerConfig = new HandlerConfig(webConfig);
            var handlerType = handlerConfig.HandlerType;

            // Create the inner handler. Assume it has a default constructor.
            _handler = (IHttpAsyncHandler) Activator.CreateInstance(handlerType);
        }

        public void ProcessRequest(HttpContext context)
        {
            // Nancy doesn't support non-async requests, so there's no point for us to do it.
            throw new NotSupportedException();
        }

        public bool IsReusable
        {
            get { return false; }
        }

        public IAsyncResult BeginProcessRequest(HttpContext context, AsyncCallback cb, object extraData)
        {
            // Check if the request is a WebSocket request. This requires IIS 7.5 in integrated pipeline mode.
            // IIS Express works well also. Web.Config must indicate the 4.5 runtime:
            // <httpRuntime targetFramework="4.5" />
            if (context.IsWebSocketRequest)
            {
                // Add a special header to indicate that this is a WebSocket request. This is necessary since
                // Nancy's context and request objects are strictly separated from those from the host (ASP.NET)
                // in this case. But the headers are copied, so that's our way in.
                context.Request.Headers.Add(Constants.WebsocketIndicatorHeader, "_");

                // Dispatch the request to the handler and intercept the completion by providing a custom
                // callback.
                return _handler.BeginProcessRequest(context, ar =>
                {
                    // Nancy uses a special Task as the async result. If for some reason the inner handler isn't
                    // a Nancy handler, we don't want to crash and burn, so we must do some type checking here.
                    // Furthermore, we don't want to wait on a task here or use a faulted task, so make sure
                    // the task has completed successfully.
                    var tsk = ar as Task<Tuple<NancyContext, HttpContextBase>>;
                    if (tsk != null && tsk.Status == TaskStatus.RanToCompletion)
                    {
                        // Ok, we have a completed Nancy task. See if the route returned our special WebSocket
                        // handler response.
                        var resp = tsk.Result.Item1.Response as WebSocketHandlerWrapperResponse;
                        if (resp != null)
                        {
                            // It did! Extract the handler and let Microsoft.WebSockets do the rest.
                            context.AcceptWebSocketRequest(new WebSocketProxy(resp.Handler));
                            ar = Task.FromResult(0);
                        }
                    }

                    // Done intercepting, now invoke the top-level callback with the async result.
                    if (cb != null)
                    {
                        cb(ar);
                    }
                }, extraData);
            }

            // Non-WebSocket request, so just dispatch to the inner handler.
            return _handler.BeginProcessRequest(context, cb, extraData);
        }

        public void EndProcessRequest(IAsyncResult result)
        {
            // In BeginProcessRequest, we return a Task<int> when we handle a WebSocket request. In this case,
            // we don't want to invoke the inner handler since it's handled by Microsoft.WebSockets.
            var ar = result as Task<int>;
            if (ar == null)
            {
                _handler.EndProcessRequest(result);
            }
        }

        /// <summary>
        /// A proxy class that sits inbetween Microsoft.WebSockets and the Nancy world. It inherits from
        /// WebSocketHandler which does most of the heavy lifting of communicating with the client. The
        /// IWebSocketClient interface means that this instance can be passed to the IWebSocketHandler
        /// instance without exposing any Microsoft.WebSockets types. The methods of IWebSocketClient
        /// overlap with those of WebSocketHandler, so there is nothing to implement for that interface.
        /// </summary>
        private class WebSocketProxy : WebSocketHandler, IWebSocketClient
        {
            private readonly IWebSocketHandler _handler;

            internal WebSocketProxy(IWebSocketHandler handler)
            {
                _handler = handler;
            }

            public override void OnOpen()
            {
                _handler.OnOpen(this);
            }

            public override void OnMessage(string message)
            {
                _handler.OnMessage(message);
            }

            public override void OnMessage(byte[] data)
            {
                _handler.OnData(data);
            }

            public override void OnError()
            {
                _handler.OnError();
            }

            public override void OnClose()
            {
                _handler.OnClose();
            }
        }

    }
}