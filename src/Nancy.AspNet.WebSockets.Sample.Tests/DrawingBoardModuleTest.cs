using System;
using Nancy.AspNet.WebSockets;
using Nancy.AspNet.WebSockets.Sample;
using Nancy.AspNet.WebSockets.Testing;
using Nancy.Testing;
using NSubstitute;
using NUnit.Framework;

namespace NancyWebsockets.Tests
{
    [TestFixture]
    public class The_DrawingBoardModule_websocket_support
    {
        private IWebSocketClient _client;
        private Browser _browser;

        [TestFixtureSetUp]
        public void Setup()
        {
            var service = new DrawingBoards();
            var board = service.Get("b1");

            _client = Substitute.For<IWebSocketClient>();
            board.Register("user1").OnOpen(_client);

            _browser = new Browser(with =>
            {
                with.Module<DrawingBoardModule>();
                with.Dependency<IDrawingBoards>(service);
            });
        }

        [SetUp]
        public void ClearMock()
        { 
            _client.ClearReceivedCalls();
        }

        [Test]
        public void Should_register_connected_user_with_board()
        {
            _browser.OpenWebSocket("/drawing/b1", socket =>
            {
                socket.Opened += (sender, args) => socket.Close();
            }, with => with.Query("name", "user2"));

            _client.Received().Send("User user2 connected to drawing board b1");
        }

        [Test]
        public void Should_register_user_with_name_Unknown_if_name_isnt_sent()
        {
            _browser.OpenWebSocket("/drawing/b1", socket =>
            {
                socket.Opened += (sender, args) => socket.Close();
            });

            _client.Received().Send("User Unknown connected to drawing board b1");
        }
    }
}
