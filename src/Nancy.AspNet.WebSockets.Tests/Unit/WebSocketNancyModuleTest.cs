using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy.Testing;
using NSubstitute;
using NUnit.Framework;

namespace Nancy.AspNet.WebSockets.Tests.Unit
{
    [TestFixture]
    public class The_WebSocketNancyModule_class
    {
        [Test]
        public void Should_return_a_special_response_object_for_a_websocket_request()
        {
            var browser = new Browser(with => with.Module<WsModule>());
            var response = browser.Get("/ws", ctx => ctx.Header(Constants.WebsocketHeaderMarker, "***"));
            Assert.That(response.Context.Response, Is.InstanceOf<WebSocketHandlerWrapperResponse>());
        }

        [Test]
        public void Should_return_the_handler_created_by_the_route()
        {
            var browser = new Browser(with => with.Module<WsModule>());
            var response = browser.Get("/ws", ctx => ctx.Header(Constants.WebsocketHeaderMarker, "***"));
            var handler = ((WebSocketHandlerWrapperResponse) response.Context.Response).Handler;

            var client = Substitute.For<IWebSocketClient>();
            handler.OnOpen(client);
            client.Received().Send("this is it");
        }

        [Test]
        public void Should_respond_with_method_not_allowed_if_not_a_websocket_request()
        {
            var browser = new Browser(with => with.Module<WsModule>());
            var response = browser.Get("/ws");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.MethodNotAllowed));
        }

        public class WsModule : WebSocketNancyModule
        {
            public WsModule()
            {
                WebSocket["/ws"] = _ => CreateHandler();
            }

            private IWebSocketHandler CreateHandler()
            {
                var handler = Substitute.For<IWebSocketHandler>();
                handler.WhenForAnyArgs(h => h.OnOpen(null)).Do(ci => ci.Arg<IWebSocketClient>().Send("this is it"));
                return handler;
            }
        }
    }
}
