using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace SimpleHooks.Server
{
    class Program
    {
        static ConfigurationHelper _Config;
        static void Main()
        {
            _Config = new ConfigurationHelper();

            Start();
        }

        private static void Start()
        {
            Business.InstanceManager manager = new(
                new Log.SQL.Logger()
                {
                    MinLogType = (Log.Interface.LogModel.LogTypes)Enum.Parse(typeof(Log.Interface.LogModel.LogTypes),_Config.Logger_MinLogLevel,true),
                    ConnectionString = _Config.ConnectionString_Log,
                    FunctionName = _Config.Logger_Function
                },
                new Repo.SQL.SqlConnectionRepo() { ConnectionString = _Config.ConnectionString_SimpleHooks },
                new Repo.SQL.EventInstanceDataRepo(),
                new Repo.SQL.ListenerInstanceDataRepo(),
                new HttpClient.Simple.SimpleClient(),
                new Repo.SQL.EventDefinitionDataRepo(),
                new Repo.SQL.ListenerDefinitionDataRepo(),
                new Repo.SQL.EventIistenerDefinitionDataRepo(),
                new Repo.SQL.AppOptionDataRepo());

            var instances = manager.GetEventInstancesToProcess(DateTime.UtcNow);
            manager.Process(instances);
        }
    }
}
