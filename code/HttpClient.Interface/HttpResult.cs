using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace HttpClient.Interface
{
    public class HttpResult
    {
        public HttpResult()
        {
            this.Headers = new List<string>();
        }
        public JObject Body { get; set; }
        public List<string> Headers { get; set; }
        public int HttpCode { get; set; }
    }
}
