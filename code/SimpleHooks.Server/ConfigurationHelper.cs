using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace SimpleTools.SimpleHooks.Server
{
    internal class ConfigurationHelper
    {
        private readonly IConfigurationRoot _config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json")
            .Build();

        public string ConnectionStringLog
        {
            get => _config.GetSection("connectionStrings")["log"]; 
            set => _config.GetSection("connectionStrings")["log"] = value;
        }

        public string ConnectionStringSimpleHooks
        {
            get => _config.GetSection("connectionStrings")["simpleHooks"];
            set => _config.GetSection("connectionStrings")["simpleHooks"] = value;
        }

        public string LoggerMinLogLevel
        {
            get => _config.GetSection("logger")["min-log-level"]; 
            set => _config.GetSection("logger")["min-log-level"] = value;
        }

        public string LoggerFunction
        {
            get => _config.GetSection("logger")["function"];
            set => _config.GetSection("logger")["function"] = value;
        }

        public int GroupId
        {
            get => int.Parse(_config.GetSection("group-id").Value);
            set => _config.GetSection("group-id").Value = value.ToString();
        }
    }
}
