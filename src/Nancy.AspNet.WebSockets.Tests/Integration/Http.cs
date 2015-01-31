using System;
using System.IO;
using System.Net;

namespace Nancy.AspNet.WebSockets.Tests.Integration
{
    internal static class Http
    {
        private static string ReadStreamText(Stream s)
        {
            // Assume UTF-8
            using (var reader = new StreamReader(s))
            {
                return reader.ReadToEnd();
            }
        }

        internal static string GetBodyAsString(string address)
        {
            var client = new WebClient();
            try
            {
                return ReadStreamText(client.OpenRead(address));
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.ProtocolError)
                {
                    var resp = ex.Response as HttpWebResponse;
                    var statusCode = resp.StatusCode;
                    var body = ReadStreamText(resp.GetResponseStream());
                    throw new Exception("Server responded with status " + statusCode + ": " + body);
                }
                throw;
            }
        }


    }
}
