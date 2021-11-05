using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HttpClient.Interface
{
    public interface IHttpClient
    {
        HttpResult Post(string url, List<string> headers, string body, int timeout);
    }
}
