using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SimpleTools.SimpleHooks.TestConsole.TestAssist
{
    internal class ReadEventInstanceStatusAssistTest(ConfigurationHelper configHelper)
    {
        private readonly ConfigurationHelper _configHelper = configHelper;

        private string _businessId = string.Empty;

        private Assist.Models.Credentials CreateCredentials()
        {
            return new Assist.Models.Credentials()
            {
                IdentityServerUrl = _configHelper.IdentityServerUrl,
                ClientId = _configHelper.CheckEventInstanceStatusClientId,
                ClientSecret = _configHelper.CheckEventInstanceStatusClientSecret
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

        private string GetBusinessId()
        {
            if (!string.IsNullOrEmpty(_businessId))
            {
                return _businessId;
            }

            var assist = new Assist.TriggerEventAssist(_configHelper.TriggerEventAnonymousUrl);

            var request = CreateTriggerEventRequestDataObject();

            var actual = assist.ExecuteAsync(request);
            actual.Wait();
            var response = actual.Result;

            // Parse the JSON response to extract the businessId
            var businessIdResponse = JsonSerializer.Deserialize<JsonElement>(response);
            if (businessIdResponse.TryGetProperty("businessId", out var businessIdJson))
            {
                var businessId = businessIdJson.GetString();
                if (string.IsNullOrEmpty(businessId))
                {
                    throw new InvalidOperationException("businessId is empty");
                }

                _businessId = businessId;
                return _businessId;
            }

            throw new InvalidOperationException("businessId not found in response");
        }

        internal void TestAnonymousExecute()
        {
            ConsoleHelper.WriteConsoleStart(MethodBase.GetCurrentMethod()!.Name);

            try
            {
                var assist = new Assist.CheckEventInstanceStatusAssist(_configHelper.CheckEventInstanceStatusAnonymousUrl);

                var businessId = GetBusinessId();

                Task<int> actual = assist.ExecuteAsync(businessId);
                actual.Wait();

                ConsoleHelper.WriteConsoleCompleted(MethodBase.GetCurrentMethod()!.Name, actual.Result.ToString());
            }
            catch (Exception e)
            {
                ConsoleHelper.WriteConsoleError(MethodBase.GetCurrentMethod()!.Name, e);
            }
        }

        internal void TestAuthenticatedExecute()
        {
            ConsoleHelper.WriteConsoleStart(MethodBase.GetCurrentMethod()!.Name);

            try
            {
                var assist = new Assist.CheckEventInstanceStatusAssist(_configHelper.CheckEventInstanceStatusAuthenticatedUrl, true);

                var businessId = GetBusinessId();

                var cred = CreateCredentials();

                Task<int> actual = assist.ExecuteAsync(businessId, cred);
                actual.Wait();

                ConsoleHelper.WriteConsoleCompleted(MethodBase.GetCurrentMethod()!.Name, actual.Result.ToString());
            }
            catch (Exception e)
            {
                ConsoleHelper.WriteConsoleError(MethodBase.GetCurrentMethod()!.Name, e);
            }
        }
    }
}
