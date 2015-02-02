using NUnit.Framework;

namespace Nancy.AspNet.WebSockets.Tests.Integration
{
    [TestFixture]
    public class The_WebSocketAwareHttpHandler_class_with_site_specifying_a_custom_http_handler : IisExpressBasedTest
    {
        public The_WebSocketAwareHttpHandler_class_with_site_specifying_a_custom_http_handler()
            : base("CustomHandlerSite")
        {
        }

        [Test]
        public void Should_handle_a_standard_request()
        {
            var text = Http.Get("http://" + GetHost()).BodyAsString();
            Assert.That(text, Is.EqualTo("custom"));
        }

        [Test]
        public void Should_invoke_the_custom_handler()
        {
            var headers = Http.Get("http://" + GetHost()).Headers;
            Assert.That(headers.Get("ThisIs"), Is.EqualTo("Custom"));
        }
    }
}
