using HttpClient.Interface;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestSimpleHooks
{
    internal class HttpClientDemo : HttpClient.Interface.IHttpClient
    {
        public HttpResult Post(string url, List<string> headers, string body, int timeout)
        {
            List<string> responseHedaers = new() {
                "'content-type':'application/json'"
            };

            if (body.Contains("success"))
            {
                return new HttpResult()
                {
                    Body = body,
                    Headers = responseHedaers,
                    HttpCode = 200
                };
            }

            return new HttpResult()
            {
                Body = body,
                Headers = responseHedaers,
                HttpCode = 400
            };
        }
    }
}
