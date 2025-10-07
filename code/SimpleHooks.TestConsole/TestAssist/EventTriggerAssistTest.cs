using SimpleTools.SimpleHooks.Assist;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace SimpleTools.SimpleHooks.TestConsole.TestAssist
{
    internal class EventTriggerAssistTest(ConfigurationHelper configHelper)
    {
        private readonly ConfigurationHelper _configHelper = configHelper;

        private Assist.Models.TriggerEventRequest CreateTriggerEventRequestDataText()
        {
            return new Assist.Models.TriggerEventRequest()
            {
                EventData = "{\"name\": \"test.user.21\"}",
                EventDefinitionId = 1,
                ReferenceName = "order-id",
                ReferenceValue = Guid.NewGuid().ToString()
            };
        }

        private Assist.Models.TriggerEventRequest CreateTriggerEventRequestDataObject()
        {
            return new Assist.Models.TriggerEventRequest()
            {
                EventData = new { name = "test.user.22" },
                EventDefinitionId = 1,
                ReferenceName = "order-id",
                ReferenceValue = Guid.NewGuid().ToString()
            };
        }

        private Assist.Models.Credentials CreateCredentials()
        {
            return new Assist.Models.Credentials()
            {
                IdentityServerUrl = _configHelper.IdentityServerUrl,
                ClientId = _configHelper.TriggerEventClientId,
                ClientSecret = _configHelper.TriggerEventClientSecret
            };
        }

        private void WriteConsoleStart(string methodName)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{methodName} started");
        }

        private void WriteConsoleCompleted(string methodName, string result)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{methodName} completed - result: {result}");
            Console.WriteLine("-------");
            Console.WriteLine();
        }

        private void WriteConsoleError(string methodName, Exception e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"{methodName} threw error: {e}");
            Console.WriteLine("-------");
            Console.WriteLine();
        }

        internal void TestAnonymousExecuteDataText()
        {
            WriteConsoleStart(MethodBase.GetCurrentMethod()!.Name);

            try
            {
                var assist = new Assist.TriggerEventAssist(configHelper.TriggerEventAnonymousUrl);

                var data = CreateTriggerEventRequestDataText();

                Task<string> actual = assist.ExecuteAsync(data);
                actual.Wait();

                WriteConsoleCompleted(MethodBase.GetCurrentMethod()!.Name, actual.Result);
            }
            catch (Exception e)
            {
                WriteConsoleError(MethodBase.GetCurrentMethod()!.Name, e);
            }
        }

        internal void TestAnonymousExecuteDataObject()
        {
            WriteConsoleStart(MethodBase.GetCurrentMethod()!.Name);

            try
            {
                var assist = new Assist.TriggerEventAssist(configHelper.TriggerEventAnonymousUrl);

                var data = CreateTriggerEventRequestDataObject();

                Task<string> actual = assist.ExecuteAsync(data);
                actual.Wait();

                WriteConsoleCompleted(MethodBase.GetCurrentMethod()!.Name, actual.Result);
            }
            catch (Exception e)
            {
                WriteConsoleError(MethodBase.GetCurrentMethod()!.Name, e);
            }
        }

        internal void TestAuthenticatedExecuteDataText()
        {
            WriteConsoleStart(MethodBase.GetCurrentMethod()!.Name);

            try
            {
                var assist = new Assist.TriggerEventAssist(configHelper.TriggerEventAuthenticatedUrl, true);

                var data = CreateTriggerEventRequestDataText();

                var cred = CreateCredentials();

                Task<string> actual = assist.ExecuteAsync(data, cred);
                actual.Wait();

                WriteConsoleCompleted(MethodBase.GetCurrentMethod()!.Name, actual.Result);
            }
            catch (Exception e)
            {
                WriteConsoleError(MethodBase.GetCurrentMethod()!.Name, e);
            }
        }

        internal void TestAuthenticatedExecuteDataObject()
        {
            WriteConsoleStart(MethodBase.GetCurrentMethod()!.Name);

            try
            {
                var assist = new Assist.TriggerEventAssist(configHelper.TriggerEventAuthenticatedUrl, true);

                var data = CreateTriggerEventRequestDataObject();

                var cred = CreateCredentials();

                Task<string> actual = assist.ExecuteAsync(data, cred);
                actual.Wait();

                WriteConsoleCompleted(MethodBase.GetCurrentMethod()!.Name, actual.Result);
            }
            catch (Exception e)
            {
                WriteConsoleError(MethodBase.GetCurrentMethod()!.Name, e);
            }
        }
    }
}
