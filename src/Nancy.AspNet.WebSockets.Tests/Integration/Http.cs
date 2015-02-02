using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;

namespace Nancy.AspNet.WebSockets.Tests.Integration
{
    internal static class Http
    {
        internal static string BodyAsString(this HttpResponse resp)
        {
            var ms = new MemoryStream();
            resp.Contents(ms);
            ms.Position = 0;
            // Assume UTF-8
            using (var reader = new StreamReader(ms))
            {
                return reader.ReadToEnd();
            }
        }

        internal static HttpResponse Get(string address)
        {
            var request = WebRequest.Create(address) as HttpWebRequest;
            try
            {
                var response = request.GetResponse() as HttpWebResponse;
                return new HttpResponse(response);
            }
            catch (WebException ex)
            {
                var resp = ex.Response as HttpWebResponse;
                return new HttpResponse(resp);
            }
        }
    }

    internal class HttpResponse
    {
        internal int StatusCode { get; private set; }
        internal Action<Stream> Contents { get; private set; }
        internal NameValueCollection Headers { get; private set; }

        internal HttpResponse(HttpWebResponse response)
        {
            StatusCode = (int) response.StatusCode;
            Contents = s =>
            {
                using (var source = response.GetResponseStream())
                {
                    source.CopyTo(s);
                }
            };
            Headers = response.Headers;
        }
    }

}
