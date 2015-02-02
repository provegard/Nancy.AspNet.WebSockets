namespace Nancy.AspNet.WebSockets.Tests.CustomHandlerSite
{
    public class RootModule : NancyModule
    {
        public RootModule()
        {
            Get["/"] = _ => "custom";
        }
    }
}