using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleHooks.Web.Helper
{
    public class ConfigurationHelper
    {
        private readonly IConfiguration _Config;
        public ConfigurationHelper(IConfiguration config)
        {
            this._Config = config;
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
