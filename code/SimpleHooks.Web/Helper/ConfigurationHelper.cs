using Microsoft.Extensions.Configuration;

namespace SimpleHooks.Web.Helper
{
    public class ConfigurationHelper(IConfiguration config)
    {
        public string ConnectionStringLog => config.GetSection("connectionStrings")["log"];

        public string ConnectionStringSimpleHooks => config.GetSection("connectionStrings")["simpleHooks"];

        public string LoggerMinLogLevel => config.GetSection("logger")["min-log-level"];

        public string LoggerFunction => config.GetSection("logger")["function"];
    }
}
