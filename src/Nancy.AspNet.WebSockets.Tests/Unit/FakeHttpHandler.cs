using System;
using System.Web;

namespace Nancy.AspNet.WebSockets.Tests.Unit
{
    public class FakeHttpHandler : IHttpAsyncHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            throw new NotImplementedException();
        }

        public bool IsReusable { get; private set; }
        public IAsyncResult BeginProcessRequest(HttpContext context, AsyncCallback cb, object extraData)
        {
            throw new NotImplementedException();
        }

        public void EndProcessRequest(IAsyncResult result)
        {
            throw new NotImplementedException();
        }
    }
}
