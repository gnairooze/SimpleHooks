using System.Collections.Generic;

namespace SimpleTools.SimpleHooks.HttpClient.Interface
{
    public interface IHttpClient
    {
        HttpResult Post(string url, List<string> headers, string body, int timeout);
    }
}
