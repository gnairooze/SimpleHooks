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
        public HttpResult Post(string url, List<string> headers, JObject body, int timeout)
        {
            List<string> responseHedaers = new() {
                "'content-type':'application/json'"
            };

            if (body.ContainsKey("success"))
            {
                body["result"] = "succeeded";

                return new HttpResult()
                {
                    Body = body,
                    Headers = responseHedaers,
                    HttpCode = 200
                };
            }

            body["result"] = "failed";

            return new HttpResult()
            {
                Body = body,
                Headers = responseHedaers,
                HttpCode = 400
            };
        }
    }
}
