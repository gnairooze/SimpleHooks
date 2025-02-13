using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HttpClient.Simple
{
    public class SimpleClient : HttpClient.Interface.IHttpClient
    {
        /// <summary>
        /// post the request to listeners
        /// </summary>
        /// <param name="url"></param>
        /// <param name="headers">must contain at least content-type. format [key]:[value]</param>
        /// <param name="body"></param>
        /// <param name="timeout">in minutes</param>
        /// <returns></returns>
        public HttpClient.Interface.HttpResult Post(string url, List<string> headers, string body, int timeout)
        {
            System.Net.Http.HttpClient client = new System.Net.Http.HttpClient();
            
            PrepareContentTypeHeader(headers);

            var mediaType = headers.Single(t => t.Contains("content-type")).Split(":".ToCharArray())[1].Trim();
            StringContent data = new StringContent(body, Encoding.UTF8, mediaType);
            client.Timeout = TimeSpan.FromMinutes(timeout);

            foreach (var header in headers)
            {
                if (string.IsNullOrEmpty(header)) continue;

                var splittedHeader = header.Split(":".ToCharArray());

                if (splittedHeader[0] == "content-type") continue;

                data.Headers.Add(splittedHeader[0], splittedHeader[1]);
            }

            var response = client.PostAsync(url, data).GetAwaiter().GetResult();

            string responseBody = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            List<string> responseHeaders = new List<string>();
            foreach (var header in response.Headers)
            {
                responseHeaders.Add($"{header.Key}:{String.Join(",", header.Value)}");
            }

            var result = new HttpClient.Interface.HttpResult()
            {
                Body = responseBody,
                Headers = responseHeaders,
                HttpCode = ((int)response.StatusCode)
            };

            client.Dispose();

            return result;
        }

        private static void PrepareContentTypeHeader(List<string> headers)
        {
            //if headers contains any variation of content-type, update the key to be content-type
            if (headers.Any(t => t.Contains("Content-Type")))
            {
                var header = headers.Single(t => t.Contains("Content-Type"));
                headers.Remove(header);
                headers.Add(header.ToLower());
            }

            //if headers not contain content-type, add it with value application/json
            if (!headers.Any(t => t.Contains("content-type")))
            {
                headers.Add("content-type:application/json");
            }
        }
    }
}
