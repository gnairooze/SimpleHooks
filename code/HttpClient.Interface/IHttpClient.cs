using System.Collections.Generic;

namespace HttpClient.Interface
{
    public interface IHttpClient
    {
        HttpResult Post(string url, List<string> headers, string body, int timeout);
    }
}
