using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleHooks.Server
{
    internal class ConfigurationHelper
    {
        private readonly IConfigurationRoot _Config;
        public ConfigurationHelper()
        {
            _Config = new ConfigurationBuilder()
                           .SetBasePath(Directory.GetCurrentDirectory())
                           .AddJsonFile("appsettings.json")
                           .Build();
        }

        public string ConnectionString_Log
        {
            get
            {
                return _Config.GetSection("connectionStrings")["log"];
            }
        }

        public string ConnectionString_SimpleHooks
        {
            get
            {
                return _Config.GetSection("connectionStrings")["simpleHooks"];
            }
        }

        public string Logger_MinLogLevel
        {
            get
            {
                return _Config.GetSection("logger")["min-log-level"];
            }
        }

        public string Logger_Function
        {
            get
            {
                return _Config.GetSection("logger")["function"];
            }
        }
    }
}
