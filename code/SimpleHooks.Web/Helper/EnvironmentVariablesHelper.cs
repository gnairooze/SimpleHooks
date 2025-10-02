using System;

namespace SimpleTools.SimpleHooks.Web
{
    public class EnvironmentVariablesHelper
    {
        public static string ConnectionStringLog => Environment.GetEnvironmentVariable("SIMPLE_HOOKS_CONNECTIONSTRING_LOG") ?? string.Empty;
        public static string ConnectionStringSimpleHooks => Environment.GetEnvironmentVariable("SIMPLE_HOOKS_CONNECTIONSTRING_SIMPLE_HOOKS") ?? string.Empty;
        public static string LoggerMinLogLevel => Environment.GetEnvironmentVariable("SIMPLE_HOOKS_LOGGER_MIN_LOG_LEVEL") ?? string.Empty;
        public static string LoggerFunction => Environment.GetEnvironmentVariable("SIMPLE_HOOKS_LOGGER_FUNCTION") ?? string.Empty;
    }
}
