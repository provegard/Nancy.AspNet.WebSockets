namespace Nancy.AspNet.WebSockets.Tests.Integration.Sites.StandardSite
{
    public class RootModule : NancyModule
    {
        public RootModule()
        {
            Get["/"] = _ => "root";
        }
    }
}
