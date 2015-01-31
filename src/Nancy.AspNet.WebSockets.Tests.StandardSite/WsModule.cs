namespace Nancy.AspNet.WebSockets.Tests.StandardSite
{
    public class WsModule : WebSocketNancyModule
    {
        public WsModule(ILog log) : base("/ws")
        {
            WebSocket["/reverse"] = _ => new ReverseHandler(log, GetClientName());
        }

        private string GetClientName()
        {
            return Request.Query.name ?? "unknown";
        }
    }
}