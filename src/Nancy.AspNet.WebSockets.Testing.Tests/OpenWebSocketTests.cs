using System;
using System.Diagnostics;
using Nancy.Testing;
using NSubstitute;
using NUnit.Framework;

namespace Nancy.AspNet.WebSockets.Testing.Tests
{
    [TestFixture]
    public class The_OpenWebSocket_extension_method
    {
        private IWebSocketHandler _handler;
        private Browser _browser;

        [SetUp]
        public void Prepare()
        {
            _handler = Substitute.For<IWebSocketHandler>();
            var module = new WsModule(_handler);
            _browser = new Browser(with => with.Module(module));
        }

        [Test]
        public void Should_raise_opened_event_on_socket_instance()
        {
            var openCount = 0;
            _browser.OpenWebSocket("/ws", socket =>
            {
                socket.Opened += (sender, args) =>
                {
                    openCount++;
                    socket.Close();
                };
            });
            Assert.That(openCount, Is.EqualTo(1));
        }

        [Test]
        public void Should_invoke_open_method_on_handler()
        {
            _browser.OpenWebSocket("/ws", socket =>
            {
                socket.Opened += (sender, args) => socket.Close();
            });
            _handler.ReceivedWithAnyArgs().OnOpen(null);
        }

        [Test]
        public void Should_raise_closed_event_once_on_socket_instance_when_closing_through_socket()
        {
            var closeListener = Substitute.For<EventHandler>();
            _browser.OpenWebSocket("/ws", socket =>
            {
                socket.Opened += (sender, args) => socket.Close();
                socket.Closed += closeListener;
            });
            closeListener.ReceivedWithAnyArgs(1)(null, null);
        }

        [Test]
        public void Should_send_message_to_handler()
        {
            _browser.OpenWebSocket("/ws", socket =>
            {
                socket.Opened += (sender, args) =>
                {
                    socket.Send("test");
                    socket.Close();
                };
            });
            _handler.Received().OnMessage("test");
        }

        [Test]
        public void Should_send_data_to_handler()
        {
            _browser.OpenWebSocket("/ws", socket =>
            {
                socket.Opened += (sender, args) =>
                {
                    socket.Send(new byte[] {42});
                    socket.Close();
                };
            });
            _handler.Received().OnData(Arg.Is<byte[]>(arg => arg.Length == 1 && arg[0] == 42));
        }

        [Test]
        public void Should_raise_closed_event_on_socket_instance_when_closing_through_handler()
        {
            IWebSocketClient client = null;
            _handler.WhenForAnyArgs(h => h.OnOpen(null)).Do(ci => client = ci.Arg<IWebSocketClient>());
            _handler.When(h => h.OnMessage("close")).Do(_ => client.Close());

            var closeListener = Substitute.For<EventHandler>();
            _browser.OpenWebSocket("/ws", socket =>
            {
                socket.Opened += (sender, args) => socket.Send("close");
                socket.Closed += closeListener;
            });
            closeListener.ReceivedWithAnyArgs(1)(null, null);
        }

        [Test]
        public void Should_raise_error_event_on_socket_when_route_doesnt_exist()
        {
            var errorListener = Substitute.For<EventHandler<BrowserExtensions.ErrorEventArgs>>();
            _browser.OpenWebSocket("/notfound", socket =>
            {
                socket.Error += errorListener;
            });
            errorListener.Received(1)(Arg.Any<object>(),
                Arg.Is<BrowserExtensions.ErrorEventArgs>(
                    args => args.ErrorResponse.StatusCode == HttpStatusCode.NotFound));
        }

        [Test]
        public void Should_not_permit_send_message_before_open()
        {
            Assert.Catch<InvalidOperationException>(() => _browser.OpenWebSocket("/ws", socket => socket.Send("test")));
        }

        [Test]
        public void Should_not_permit_send_data_before_open()
        {
            Assert.Catch<InvalidOperationException>(
                () => _browser.OpenWebSocket("/ws", socket => socket.Send(new byte[] {99})));
        }

        [Test]
        public void Should_not_permit_close_before_open()
        {
            Assert.Catch<InvalidOperationException>(
                () => _browser.OpenWebSocket("/ws", socket => socket.Close()));
        }

        [Test]
        public void Should_deliver_handler_message_via_messagereceived_event()
        {
            _handler.WhenForAnyArgs(h => h.OnOpen(null)).Do(ci =>
            {
                var client = ci.Arg<IWebSocketClient>();
                client.Send("hello");
                client.Close();
            });
            var messageListener = Substitute.For<EventHandler<BrowserExtensions.MessageEventArgs>>();
            _browser.OpenWebSocket("/ws", socket =>
            {
                socket.MessageReceived += messageListener;
            });
            messageListener.Received(1)(Arg.Any<object>(),
                Arg.Is<BrowserExtensions.MessageEventArgs>(args => args.Message == "hello"));
        }

        [Test]
        public void Should_deliver_handler_data_via_datareceived_event()
        {
            _handler.WhenForAnyArgs(h => h.OnOpen(null)).Do(ci =>
            {
                var client = ci.Arg<IWebSocketClient>();
                client.Send(new byte[] {99});
                client.Close();
            });
            var dataListener = Substitute.For<EventHandler<BrowserExtensions.DataEventArgs>>();
            _browser.OpenWebSocket("/ws", socket =>
            {
                socket.DataReceived += dataListener;
            });
            dataListener.Received(1)(Arg.Any<object>(),
                Arg.Is<BrowserExtensions.DataEventArgs>(args => args.Data.Length == 1 && args.Data[0] == 99));
        }

        [Test]
        public void Should_cancel_after_timeout_if_socket_isnt_closed()
        {
            Assert.Catch<OperationCanceledException>(() => _browser.OpenWebSocket("/ws", socket => { }));
        }

        [Test]
        public void Should_have_configurable_cancel_timeout()
        {
            var sw = Stopwatch.StartNew();
            try
            {
                _browser.OpenWebSocket("/ws", socket => { socket.MaxAge = TimeSpan.FromMilliseconds(200); });
            }
            catch (OperationCanceledException)
            {
                // ignore
            }
            sw.Stop();
            Assert.That(sw.ElapsedMilliseconds, Is.LessThan(500));
        }

        public class WsModule : WebSocketNancyModule
        {
            public WsModule(IWebSocketHandler handler)
            {
                WebSocket["/ws"] = _ => handler;
            }
        }

    }
}
