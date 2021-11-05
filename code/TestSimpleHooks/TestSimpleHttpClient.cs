using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestSimpleHooks
{
    internal class TestSimpleHttpClient
    {
        public static void Test1()
        {
            HttpClient.Simple.SimpleClient client = new();

            List<string> headers = new()
            {
                "content-type:application/json",
                "version:1.0"
            };
            var result = client.Post("https://webhook.site/95aa0ef9-d975-4968-b9f6-cf7bf8d15ed5", headers, "{'test':'value'}", 1);

            Console.WriteLine(result.HttpCode);
            Console.WriteLine(result.Body);
            foreach (var item in result.Headers)
            {
                Console.WriteLine(item);
            }
            
        }
    }
}
