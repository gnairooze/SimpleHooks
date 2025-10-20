using SimpleTools.SimpleHooks.ListenerPlugins.Anonymous;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DebugListeners
{
    internal class DebugAnonymousListener
    {
        internal static void ExecuteListener1()
        {
            string eventData = @"
{
  ""data"": {
    ""key1"": ""value1"",
    ""key2"": ""value2"",
    ""key3"": ""value3""
  },
  ""simpleHooksMetadata"": { 
    ""eventDefinitionId"": 1,
    ""eventDefinitionName"": ""Test Event"",
    ""eventBusinessId"": ""88878721-0a73-4c9d-9b63-aedb28ef22f1"",
    ""eventCreateDate"": ""2025-10-10T19:21:29.31"",
    ""eventReferenceName"": ""sale.id"",
    ""eventReferenceValue"": ""abd2003""
    }
}
";
            string typeOptionsValue = "{}";
            var listenerInstanceId = 1;

            var listener = SetupListener();

            var task = listener.ExecuteAsync(listenerInstanceId, eventData, typeOptionsValue);

            task.Wait();

            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            Console.WriteLine($"result: {task.Result.Succeeded} - {task.Result.Message}");
            Console.WriteLine("---");
            for (int i = 0; i < task.Result.Logs.Count; i++)
            {
                var log = task.Result.Logs[i];
                Console.WriteLine(JsonSerializer.Serialize(log, options));
                Console.WriteLine("---");
            }
        }

        static AnonymousListener SetupListener()
        {
            var listener = new SimpleTools.SimpleHooks.ListenerPlugins.Anonymous.AnonymousListener()
            {
                Url = "http://localhost:5011/api/sample",
                Headers = [ "content-type:application/json" ],
                Timeout = 1
            };

            return listener;
        }
    }
}
