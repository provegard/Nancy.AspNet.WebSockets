using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy.Testing;

namespace Nancy.AspNet.WebSockets.Testing
{
    public static class BrowserExtensions
    {
        public static void WebSocketConnect(this Browser browser, string path, Action<BrowserContext> configure = null)
        {
            var response = browser.Get(path, ctx =>
            {
                if (configure != null)
                    configure(ctx);
                ctx.Header(Constants.WebsocketHeaderMarker, "***");
            });
        }
    }
}
