using SimpleTools.SimpleHooks.HttpClient.Interface;
using System.Collections.Generic;

namespace SimpleTools.SimpleHooks.TestSimpleHooks
{
    internal class HttpClientDemo : IHttpClient
    {
        public HttpResult Post(string url, List<string> headers, string body, int timeout)
        {
            List<string> responseHeaders = ["'content-type':'application/json'"];

            if (body.Contains("success"))
            {
                return new HttpResult()
                {
                    Body = body,
                    Headers = responseHeaders,
                    HttpCode = 200
                };
            }

            return new HttpResult()
            {
                Body = body,
                Headers = responseHeaders,
                HttpCode = 400
            };
        }
    }
}
