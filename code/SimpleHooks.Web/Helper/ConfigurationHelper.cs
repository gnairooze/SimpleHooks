using Microsoft.Extensions.Configuration;

namespace SimpleTools.SimpleHooks.Web.Helper
{
    public class ConfigurationHelper(IConfiguration config)
    {
        public string ConnectionStringLog
        {
            get => config.GetSection("connectionStrings")["log"];
            set => config.GetSection("connectionStrings")["log"] = value;
        }

        public string ConnectionStringSimpleHooks
        {
            get => config.GetSection("connectionStrings")["simpleHooks"];
            set => config.GetSection("connectionStrings")["simpleHooks"] = value;
        }

        public string LoggerMinLogLevel
        {
            get => config.GetSection("logger")["min-log-level"];
            set => config.GetSection("logger")["min-log-level"] = value;
        }

        public string LoggerFunction
        {
            get => config.GetSection("logger")["function"];
            set => config.GetSection("logger")["function"] = value;
        }
    }
}
