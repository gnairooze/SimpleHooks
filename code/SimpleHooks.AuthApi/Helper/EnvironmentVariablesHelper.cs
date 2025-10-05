using System;

namespace SimpleTools.SimpleHooks.AuthApi
{
    public class EnvironmentVariablesHelper
    {
        public static string ConnectionStringLog => Environment.GetEnvironmentVariable("SIMPLE_HOOKS_CONNECTIONSTRING_LOG") ?? string.Empty;
        public static string ConnectionStringSimpleHooks => Environment.GetEnvironmentVariable("SIMPLE_HOOKS_CONNECTIONSTRING_SIMPLE_HOOKS") ?? string.Empty;
        public static string LoggerMinLogLevel => Environment.GetEnvironmentVariable("SIMPLE_HOOKS_LOGGER_MIN_LOG_LEVEL") ?? string.Empty;
        public static string LoggerFunction => Environment.GetEnvironmentVariable("SIMPLE_HOOKS_LOGGER_FUNCTION") ?? string.Empty;
        public static string IdentityServerAuthority => Environment.GetEnvironmentVariable("SIMPLE_HOOKS_IDENTITYSERVER_AUTHORITY") ?? string.Empty;
        public static string IdentityServerAudience => Environment.GetEnvironmentVariable("SIMPLE_HOOKS_IDENTITYSERVER_AUDIENCE") ?? string.Empty;
        public static string IdentityServerIntrospectionEndpoint => Environment.GetEnvironmentVariable("SIMPLE_HOOKS_IDENTITYSERVER_INTROSPECTIONENDPOINT") ?? string.Empty;
        public static string IdentityServerClientId => Environment.GetEnvironmentVariable("SIMPLE_HOOKS_IDENTITYSERVER_CLIENTID") ?? string.Empty;
        public static string IdentityServerClientSecret => Environment.GetEnvironmentVariable("SIMPLE_HOOKS_IDENTITYSERVER_CLIENTSECRET") ?? string.Empty;
    }
}
