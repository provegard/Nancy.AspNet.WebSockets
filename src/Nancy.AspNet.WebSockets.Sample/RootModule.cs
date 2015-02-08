/* Copyright 2015 Per Rovegard
   Licensed under the MIT license. See LICENSE file in the root of the repo for the full license. */
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