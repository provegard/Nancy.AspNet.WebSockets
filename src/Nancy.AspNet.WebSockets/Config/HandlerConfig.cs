/* Copyright 2015 Per Rovegard
   Licensed under the MIT license. See LICENSE file in the root of the repo for the full license. */
using System;
using System.Configuration;
using System.Linq;
using Nancy.Hosting.Aspnet;

namespace Nancy.AspNet.WebSockets.Config
{
    public class HandlerConfig
    {
        public Type HandlerType { get; private set; }

        public HandlerConfig(Configuration config)
        {
            var handlerType = FindHandlerType(config.Sections)
                              ?? config.SectionGroups.Cast<ConfigurationSectionGroup>()
                                  .Select(grp => FindHandlerType(grp.Sections)).FirstOrDefault(t => t != null)
                              ?? typeof (NancyHttpRequestHandler);
            HandlerType = handlerType;
        }

        private Type FindHandlerType(ConfigurationSectionCollection sections)
        {
            return sections.OfType<NawsSection>().Select(nawsSection => nawsSection.HttpHandler.Type).FirstOrDefault();
        }
    }
}
