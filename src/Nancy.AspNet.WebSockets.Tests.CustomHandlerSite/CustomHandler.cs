using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nancy.Hosting.Aspnet;

namespace Nancy.AspNet.WebSockets.Tests.CustomHandlerSite
{
    public class CustomHandler : IHttpAsyncHandler
    {
        private readonly IHttpAsyncHandler _inner = new NancyHttpRequestHandler();

        public void ProcessRequest(HttpContext context)
        {
            throw new NotImplementedException();
        }

        public bool IsReusable { get { return false; } }

        public IAsyncResult BeginProcessRequest(HttpContext context, AsyncCallback cb, object extraData)
        {
            return _inner.BeginProcessRequest(context, ar =>
            {
                context.Response.AddHeader("ThisIs", "Custom");
                if (cb != null)
                   cb(ar);
            }, extraData);
        }

        public void EndProcessRequest(IAsyncResult result)
        {
            _inner.EndProcessRequest(result);
        }
    }
}