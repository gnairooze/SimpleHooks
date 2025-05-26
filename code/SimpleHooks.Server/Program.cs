using System;

namespace SimpleHooks.Server
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
                new Log.SQL.Logger()
                {
                    MinLogType = (Log.Interface.LogModel.LogTypes)Enum.Parse(typeof(Log.Interface.LogModel.LogTypes),_config.LoggerMinLogLevel,true),
                    ConnectionString = _config.ConnectionStringLog,
                    FunctionName = _config.LoggerFunction
                },
                new Repo.SQL.SqlConnectionRepo() { ConnectionString = _config.ConnectionStringSimpleHooks },
                new Repo.SQL.EventInstanceDataRepo(),
                new Repo.SQL.ListenerInstanceDataRepo(),
                new HttpClient.Simple.SimpleClient(),
                new Repo.SQL.EventDefinitionDataRepo(),
                new Repo.SQL.ListenerDefinitionDataRepo(),
                new Repo.SQL.EventIistenerDefinitionDataRepo(),
                new Repo.SQL.AppOptionDataRepo());

            var instances = manager.GetEventInstancesToProcess(DateTime.UtcNow, _config.GroupId);
            manager.Process(instances);
        }
    }
}
