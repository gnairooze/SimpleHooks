using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SimpleTools.SimpleHooks.TestConsole.TestAssist
{
    internal class LoadDefinitionsAssistTest(ConfigurationHelper configHelper)
    {
        private readonly ConfigurationHelper _configHelper = configHelper;

        private Assist.Models.Credentials CreateCredentials()
        {
            return new Assist.Models.Credentials()
            {
                IdentityServerUrl = _configHelper.IdentityServerUrl,
                ClientId = _configHelper.LoadDefinitionsClientId,
                ClientSecret = _configHelper.LoadDefinitionsClientSecret
            };
        }

        internal void TestAnonymousExecute()
        {
            ConsoleHelper.WriteConsoleStart(MethodBase.GetCurrentMethod()!.Name);

            try
            {
                var assist = new Assist.LoadDefinitionsAssist(_configHelper.LoadDefinitionsAnonymousUrl);

                Task<string> actual = assist.ExecuteAsync();
                actual.Wait();

                ConsoleHelper.WriteConsoleCompleted(MethodBase.GetCurrentMethod()!.Name, actual.Result);
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
                var assist = new Assist.LoadDefinitionsAssist(_configHelper.LoadDefinitionsAnonymousUrl, true);

                var cred = CreateCredentials();

                Task<string> actual = assist.ExecuteAsync(cred);
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
