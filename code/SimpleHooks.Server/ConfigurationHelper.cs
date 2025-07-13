using Microsoft.Extensions.Configuration;
using System.IO;

namespace SimpleTools.SimpleHooks.Server
{
    internal class ConfigurationHelper
    {
        private readonly IConfigurationRoot _config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        public string ConnectionStringLog => _config.GetSection("connectionStrings")["log"];

        public string ConnectionStringSimpleHooks => _config.GetSection("connectionStrings")["simpleHooks"];

        public string LoggerMinLogLevel => _config.GetSection("logger")["min-log-level"];

        public string LoggerFunction => _config.GetSection("logger")["function"];

        public int GroupId => int.Parse(_config.GetSection("group-id").Value);
    }
}
