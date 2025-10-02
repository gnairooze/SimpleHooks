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

            #region if environment variables are set related confiration settings with them
            if (!String.IsNullOrEmpty(EnvironmentVariablesHelper.ConnectionStringSimpleHooks))
            {
                _config.ConnectionStringSimpleHooks = EnvironmentVariablesHelper.ConnectionStringSimpleHooks;
            }

            if (!String.IsNullOrEmpty(EnvironmentVariablesHelper.ConnectionStringLog))
            {
                _config.ConnectionStringLog = EnvironmentVariablesHelper.ConnectionStringLog;
            }

            if (!String.IsNullOrEmpty(EnvironmentVariablesHelper.LoggerMinLogLevel))
            {
                _config.LoggerMinLogLevel = EnvironmentVariablesHelper.LoggerMinLogLevel;
            }

            if (!String.IsNullOrEmpty(EnvironmentVariablesHelper.LoggerFunction))
            {
                _config.LoggerFunction = EnvironmentVariablesHelper.LoggerFunction;
            }
            #endregion

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
