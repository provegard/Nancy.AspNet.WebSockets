/* Copyright 2015 Per Rovegard
   Licensed under the MIT license. See LICENSE file in the root of the repo for the full license. */
using System;
using System.ComponentModel;
using System.Configuration;
using System.Web;

namespace Nancy.AspNet.WebSockets.Config
{
    public class NawsSection : ConfigurationSection
    {
        [ConfigurationProperty("httpHandler")]
        public HttpHandlerElement HttpHandler
        {
            get { return (HttpHandlerElement)this["httpHandler"]; }
            set { this["httpHandler"] = value; }
        }
    }

    public class HttpHandlerElement : ConfigurationElement
    {
        [ConfigurationProperty("type", IsRequired = true)]
        [TypeConverter(typeof(TypeNameConverter)), SubclassTypeValidator(typeof (IHttpAsyncHandler))]
        public Type Type
        {
            get { return (Type) this["type"]; }
            set { this["type"] = value; }
        }
    }
}
