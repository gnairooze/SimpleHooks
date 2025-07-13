using System;
using SimpleTools.SimpleHooks.Business;
using SimpleTools.SimpleHooks.Log.SQL;
using SimpleTools.SimpleHooks.Repo.SQL;
using SimpleTools.SimpleHooks.HttpClient.Simple;
using SimpleTools.SimpleHooks.Log.Interface;


namespace SimpleTools.SimpleHooks.Server
{
    static class Program
    {
        static ConfigurationHelper _config;
        static void Main()
        {
            _config = new ConfigurationHelper();

            Start();
        }

        private static void Start()
        {
            Business.InstanceManager manager = new(
                new Logger()
                {
                    MinLogType = (LogModel.LogTypes)Enum.Parse(typeof(LogModel.LogTypes),_config.LoggerMinLogLevel,true),
                    ConnectionString = _config.ConnectionStringLog,
                    FunctionName = _config.LoggerFunction
                },
                new SqlConnectionRepo() { ConnectionString = _config.ConnectionStringSimpleHooks },
                new EventInstanceDataRepo(),
                new ListenerInstanceDataRepo(),
                new SimpleClient(),
                new EventDefinitionDataRepo(),
                new ListenerDefinitionDataRepo(),
                new EventIistenerDefinitionDataRepo(),
                new AppOptionDataRepo());

            var instances = manager.GetEventInstancesToProcess(DateTime.UtcNow, _config.GroupId);
            manager.Process(instances);
        }
    }
}
