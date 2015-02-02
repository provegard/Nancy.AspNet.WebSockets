using Nancy.AspNet.WebSockets;
using Nancy.AspNet.WebSockets.Sample;
using NSubstitute;
using NUnit.Framework;

namespace NancyWebsockets.Tests
{
    [TestFixture]
    public class The_DrawingBoard_class
    {
        [Test]
        public void Should_return_a_handler_upon_user_registration()
        {
            var db = new DrawingBoard("board");
            var handler = db.Register("user");
            Assert.IsNotNull(handler);
        }

        [Test]
        public void Should_broadcast_user_connection()
        {
            var db = new DrawingBoard("board");

            var handler = db.Register("user");
            var client1 = Substitute.For<IWebSocketClient>();
            handler.OnOpen(client1);

            var handler2 = db.Register("user2");
            handler2.OnOpen(Substitute.For<IWebSocketClient>());

            client1.Received().Send("User user2 connected to drawing board board");
        }

        [Test]
        public void Should_broadcast_user_message()
        {
            var db = new DrawingBoard("board");

            var handler = db.Register("user");
            var client1 = Substitute.For<IWebSocketClient>();
            handler.OnOpen(client1);

            var handler2 = db.Register("user2");
            handler2.OnOpen(Substitute.For<IWebSocketClient>());

            handler2.OnMessage("testing");

            client1.Received().Send("User user2 says: testing");
        }

        [Test]
        public void Should_not_send_user_message_to_self()
        {
            var db = new DrawingBoard("board");

            var handler = db.Register("user");
            var client1 = Substitute.For<IWebSocketClient>();
            handler.OnOpen(client1);

            handler.OnMessage("testing");

            client1.DidNotReceiveWithAnyArgs().Send("");
        }

        [Test]
        public void Should_broadcast_binary_data_as_is()
        {
            var db = new DrawingBoard("board");

            var handler = db.Register("user");
            var client1 = Substitute.For<IWebSocketClient>();
            handler.OnOpen(client1);

            var handler2 = db.Register("user2");
            handler2.OnOpen(Substitute.For<IWebSocketClient>());

            handler2.OnData(new byte[] {99});

            client1.Received().Send(Arg.Is<byte[]>(bytes => bytes.Length == 1 && bytes[0] == 99));
        }

        [Test]
        public void Should_broadcast_user_disconnect()
        {
            var db = new DrawingBoard("board");

            var handler = db.Register("user");
            var client1 = Substitute.For<IWebSocketClient>();
            handler.OnOpen(client1);

            var handler2 = db.Register("user2");
            handler2.OnOpen(Substitute.For<IWebSocketClient>());
            handler2.OnClose();

            client1.Received().Send("User user2 disconnected from drawing board board");
        }
    }
}
