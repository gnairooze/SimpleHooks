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
        private readonly System.Net.Http.HttpClient _Client = new System.Net.Http.HttpClient();

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
            var mediaType = headers.Single(t => t.Contains("content-type")).Split(":".ToCharArray())[1];
            StringContent data = new StringContent(body, Encoding.UTF8);
            this._Client.Timeout = TimeSpan.FromMinutes(timeout);

            foreach (var header in headers)
            {
                var splittedHeader = header.Split(":".ToCharArray());
                this._Client.DefaultRequestHeaders.Add(splittedHeader[0], splittedHeader[1]);
            }

            var response = this._Client.PostAsync(url, data).GetAwaiter().GetResult();

            string responseBody = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            List<string> responseHeaders = new List<string>();
            foreach (var header in response.Headers)
            {
                responseHeaders.Add($"{header.Key}:{header.Value}");
            }

            return new HttpClient.Interface.HttpResult()
            {
                Body = responseBody,
                Headers = responseHeaders,
                HttpCode = ((int)response.StatusCode)
            };
        }
    }
}
