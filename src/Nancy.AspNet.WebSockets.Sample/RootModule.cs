namespace Nancy.AspNet.WebSockets.Sample
{
    public class RootModule : NancyModule
    {
        public RootModule()
        {
            Get["/"] = _ => View["Index"];
        }
    }
}