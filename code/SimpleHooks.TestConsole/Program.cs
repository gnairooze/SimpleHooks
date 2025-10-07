using Microsoft.Extensions.Configuration;
using SimpleTools.SimpleHooks.TestConsole;

IConfigurationRoot _config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json")
    .Build();

var configHelper = new ConfigurationHelper(_config);

#region EventTriggerAssistTest
Console.WriteLine("---EventTriggerAssistTest---");
var eventTriggerAssistTest = new SimpleTools.SimpleHooks.TestConsole.TestAssist.EventTriggerAssistTest(configHelper);

eventTriggerAssistTest.TestAnonymousExecuteDataObject();
eventTriggerAssistTest.TestAnonymousExecuteDataText();
eventTriggerAssistTest.TestAuthenticatedExecuteDataObject();
eventTriggerAssistTest.TestAuthenticatedExecuteDataText();

Console.WriteLine("===================");
#endregion

#region ReadEventInstanceStatusAssistTest
Console.WriteLine("---ReadEventInstanceStatusAssistTest---");
var readEventInstanceStatusAssistTest =
    new SimpleTools.SimpleHooks.TestConsole.TestAssist.ReadEventInstanceStatusAssistTest(configHelper);

readEventInstanceStatusAssistTest.TestAnonymousExecute();
readEventInstanceStatusAssistTest.TestAuthenticatedExecute();
Console.WriteLine("===================");
#endregion

#region LoadDefinitionsAssistTest
Console.WriteLine("---LoadDefinitionsAssistTest---");
var loadDefinitionsAssistTest =
    new SimpleTools.SimpleHooks.TestConsole.TestAssist.LoadDefinitionsAssistTest(configHelper);
loadDefinitionsAssistTest.TestAnonymousExecute();
loadDefinitionsAssistTest.TestAuthenticatedExecute();
Console.WriteLine("===================");
#endregion
