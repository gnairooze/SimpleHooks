using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace HttpClient.Interface
{
    public interface IHttpClient
    {
        HttpResult Post(string url, List<string> headers, JObject body, int timeout);
    }
}
