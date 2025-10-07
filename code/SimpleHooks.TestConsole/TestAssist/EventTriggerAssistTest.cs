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

        private static Assist.Models.TriggerEventRequest CreateTriggerEventRequestDataText()
        {
            return new Assist.Models.TriggerEventRequest()
            {
                EventData = "{\"name\": \"test.user.334\"}",
                EventDefinitionId = 1,
                ReferenceName = "order-id",
                ReferenceValue = Guid.NewGuid().ToString()
            };
        }

        private static Assist.Models.TriggerEventRequest CreateTriggerEventRequestDataObject()
        {
            return new Assist.Models.TriggerEventRequest()
            {
                EventData = new { name = "test.user.333" },
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

        internal void TestAnonymousExecuteDataText()
        {
            ConsoleHelper.WriteConsoleStart(MethodBase.GetCurrentMethod()!.Name);

            try
            {
                var assist = new Assist.TriggerEventAssist(_configHelper.TriggerEventAnonymousUrl);

                var data = CreateTriggerEventRequestDataText();

                Task<string> actual = assist.ExecuteAsync(data);
                actual.Wait();

                ConsoleHelper.WriteConsoleCompleted(MethodBase.GetCurrentMethod()!.Name, actual.Result);
            }
            catch (Exception e)
            {
                ConsoleHelper.WriteConsoleError(MethodBase.GetCurrentMethod()!.Name, e);
            }
        }

        internal void TestAnonymousExecuteDataObject()
        {
            ConsoleHelper.WriteConsoleStart(MethodBase.GetCurrentMethod()!.Name);

            try
            {
                var assist = new Assist.TriggerEventAssist(_configHelper.TriggerEventAnonymousUrl);

                var data = CreateTriggerEventRequestDataObject();

                Task<string> actual = assist.ExecuteAsync(data);
                actual.Wait();

                ConsoleHelper.WriteConsoleCompleted(MethodBase.GetCurrentMethod()!.Name, actual.Result);
            }
            catch (Exception e)
            {
                ConsoleHelper.WriteConsoleError(MethodBase.GetCurrentMethod()!.Name, e);
            }
        }

        internal void TestAuthenticatedExecuteDataText()
        {
            ConsoleHelper.WriteConsoleStart(MethodBase.GetCurrentMethod()!.Name);

            try
            {
                var assist = new Assist.TriggerEventAssist(_configHelper.TriggerEventAuthenticatedUrl, true);

                var data = CreateTriggerEventRequestDataText();

                var cred = CreateCredentials();

                Task<string> actual = assist.ExecuteAsync(data, cred);
                actual.Wait();

                ConsoleHelper.WriteConsoleCompleted(MethodBase.GetCurrentMethod()!.Name, actual.Result);
            }
            catch (Exception e)
            {
                ConsoleHelper.WriteConsoleError(MethodBase.GetCurrentMethod()!.Name, e);
            }
        }

        internal void TestAuthenticatedExecuteDataObject()
        {
            ConsoleHelper.WriteConsoleStart(MethodBase.GetCurrentMethod()!.Name);

            try
            {
                var assist = new Assist.TriggerEventAssist(_configHelper.TriggerEventAuthenticatedUrl, true);

                var data = CreateTriggerEventRequestDataObject();

                var cred = CreateCredentials();

                Task<string> actual = assist.ExecuteAsync(data, cred);
                actual.Wait();

                ConsoleHelper.WriteConsoleCompleted(MethodBase.GetCurrentMethod()!.Name, actual.Result);
            }
            catch (Exception e)
            { 
                ConsoleHelper.WriteConsoleError(MethodBase.GetCurrentMethod()!.Name, e);
            }
        }
    }
}
