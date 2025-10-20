using SimpleTools.SimpleHooks.ListenerPlugins.Anonymous;
using SimpleTools.SimpleHooks.ListenerPlugins.TypeA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DebugListeners
{
    internal class DebugTypeAListener
    {
        internal static void ExecuteListener1()
        {
            string eventData = string.Empty;
            string typeOptionsValue = "{}";
            var listenerInstanceId = 1;

            var listener = SetupListener();

            var task = listener.ExecuteAsync(listenerInstanceId, eventData, typeOptionsValue);

            task.Wait();

            Console.WriteLine(task.Result.ToString());
        }

        static TypeAListener SetupListener()
        {
            var listener = new SimpleTools.SimpleHooks.ListenerPlugins.TypeA.TypeAListener()
            {
                Url = "http://localhost:5011/api/sample",
                Headers = [ "content-type:application/json" ],
                Timeout = 1
            };

            return listener;
        }
    }
}
