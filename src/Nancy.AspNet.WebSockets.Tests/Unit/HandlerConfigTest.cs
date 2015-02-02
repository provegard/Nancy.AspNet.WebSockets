using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Nancy.AspNet.WebSockets.Config;
using Nancy.Hosting.Aspnet;
using NUnit.Framework;

namespace Nancy.AspNet.WebSockets.Tests.Unit
{
    [TestFixture]
    public class HandlerConfigTest
    {
        private HandlerConfig CreateConfig(string resourceName)
        {
            var configuration = GetType().OpenConfigurationFromResource(resourceName);
            return new HandlerConfig(configuration);
        }

        [Test]
        public void Should_extract_handler_type_from_section()
        {
            var config = CreateConfig("WebConfigs.Custom_handler.config");
            Assert.That(config.HandlerType, Is.EqualTo(typeof(FakeHttpHandler)));
        }

        [Test]
        public void Should_extract_handler_type_from_section_in_group()
        {
            var config = CreateConfig("WebConfigs.Custom_handler_in_group.config");
            Assert.That(config.HandlerType, Is.EqualTo(typeof(FakeHttpHandler)));
        }

        [Test]
        public void Should_require_IHttpAsyncHandler_type()
        {
            Assert.Catch<ConfigurationErrorsException>(() => CreateConfig("WebConfigs.Wrong_handler_type.config"));
        }

        [Test]
        public void Should_require_type_on_httpHandler()
        {
            Assert.Catch<ConfigurationErrorsException>(() => CreateConfig("WebConfigs.Omitted_handler_type.config"));
        }

        [Test]
        public void Should_provide_default_handler_if_no_custom_specified()
        {
            var config = CreateConfig("WebConfigs.No_custom_handler.config");
            Assert.That(config.HandlerType, Is.EqualTo(typeof(NancyHttpRequestHandler)));
        }
    }
}
