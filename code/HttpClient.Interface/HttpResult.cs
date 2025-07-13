using System.Collections.Generic;

namespace SimpleTools.SimpleHooks.HttpClient.Interface
{
    public class HttpResult
    {
        public string Body { get; set; }
        public List<string> Headers { get; set; } = new List<string>();
        public int HttpCode { get; set; }

        public override string ToString()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }
    }
}
