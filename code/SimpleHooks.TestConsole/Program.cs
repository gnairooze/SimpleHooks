using Microsoft.Extensions.Configuration;
using SimpleTools.SimpleHooks.TestConsole;

IConfigurationRoot _config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json")
    .Build();

var configHelper = new ConfigurationHelper(_config);

#region EventTriggerAssistTest
var eventTriggerAssistTest = new SimpleTools.SimpleHooks.TestConsole.TestAssist.EventTriggerAssistTest(configHelper);

eventTriggerAssistTest.TestAnonymousExecuteDataObject();
eventTriggerAssistTest.TestAnonymousExecuteDataText();
eventTriggerAssistTest.TestAuthenticatedExecuteDataObject();
eventTriggerAssistTest.TestAuthenticatedExecuteDataText();
#endregion


