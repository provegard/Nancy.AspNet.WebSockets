namespace Nancy.AspNet.WebSockets.Tests.StandardSite
{
    public class LogModule : NancyModule
    {
        public LogModule(ILog log)
        {
            Get["/log"] = _ => string.Join("\n", log.GetMessages());
        }
    }
}