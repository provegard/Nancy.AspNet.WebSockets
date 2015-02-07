using System;
using System.Threading.Tasks;
using NUnit.Framework;
using WebSocket4Net;

namespace Nancy.AspNet.WebSockets.Tests.Integration
{
    [TestFixture]
    public class The_WebSocketAwareHttpHandler_class_with_site_not_overriding_the_default_nancy_handler : IisExpressBasedTest
    {
        public The_WebSocketAwareHttpHandler_class_with_site_not_overriding_the_default_nancy_handler()
            : base("StandardSite")
        {
        }

        [Test]
        public void Should_handle_a_standard_request()
        {
            var text = Http.Get("http://" + GetHost()).BodyAsString();
            Assert.AreEqual("root", text);
        }

        [Test]
        public async void Should_forward_a_websocket_message_of_string_type_to_a_handler()
        {
            var tcs = new TaskCompletionSource<string>();
            var websocket = new WebSocket("ws://" + GetHost() + "/ws/reverse");
            websocket.Error += (sender, e) => tcs.SetException(e.Exception);
            websocket.MessageReceived += (sender, e) => tcs.SetResult(e.Message);
            websocket.Open();

            // Send data as soon as the websocket is open
            websocket.Opened += (sender, e) => websocket.Send("hello world");

            var result = await tcs.Task.GetResultWithin(TimeSpan.FromMilliseconds(3000));
            Assert.AreEqual("dlrow olleh", result);
        }

        [Test]
        public async void Should_forward_a_websocket_message_of_byte_array_type_to_a_handler()
        {
            var tcs = new TaskCompletionSource<byte[]>();
            var websocket = new WebSocket("ws://" + GetHost() + "/ws/reverse");
            websocket.Error += (sender, e) => tcs.SetException(e.Exception);
            websocket.DataReceived += (sender, e) => tcs.SetResult(e.Data);
            websocket.Open();

            // Send data as soon as the websocket is open
            websocket.Opened += (sender, e) => websocket.Send(new byte[] {1, 2, 3}, 0, 3);

            var result = await tcs.Task.GetResultWithin(TimeSpan.FromMilliseconds(3000));
            CollectionAssert.AreEqual(new byte[] {3, 2, 1}, result);
        }

        [Test]
        public async void Should_notify_a_handler_when_the_client_closes_the_socket()
        {
            var clientName = Guid.NewGuid().ToString();
            var tcs = new TaskCompletionSource<bool>();
            var websocket = new WebSocket("ws://" + GetHost() + "/ws/reverse?name=" + clientName);
            websocket.Error += (sender, e) => tcs.SetException(e.Exception);
            websocket.Open();

            // Close the socket immediately
            websocket.Opened += (sender, e) =>
            {
                websocket.Close();
                tcs.SetResult(true);
            };

            // Wait until we have closed the socket
            await tcs.Task.GetResultWithin(TimeSpan.FromMilliseconds(3000));

            // Get the log
            var newlineSeparated = Http.Get("http://" + GetHost() + "/log").BodyAsString();
            var messages = newlineSeparated.Split('\n');

            CollectionAssert.Contains(messages, "client " + clientName + " closed");
        }
    }
}
