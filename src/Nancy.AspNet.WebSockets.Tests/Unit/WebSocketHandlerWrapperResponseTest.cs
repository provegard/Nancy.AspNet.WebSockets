using NSubstitute;
using NUnit.Framework;

namespace Nancy.AspNet.WebSockets.Tests.Unit
{
    [TestFixture]
    public class The_WebSocketHandlerWrapperResponse_class
    {
        [Test]
        public void Should_wrap_and_expose_a_WebSocketHandler()
        {
            var handler = Substitute.For<IWebSocketHandler>();
            var resp = new WebSocketHandlerWrapperResponse(handler);
            Assert.That(resp.Handler, Is.SameAs(handler));
        }
    }
}
