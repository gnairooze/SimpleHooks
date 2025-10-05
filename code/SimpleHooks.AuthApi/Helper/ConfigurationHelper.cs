using Microsoft.Extensions.Configuration;

namespace SimpleTools.SimpleHooks.AuthApi.Helper
{
    public class ConfigurationHelper(IConfiguration config)
    {
        public string ConnectionStringLog
        {
            get => config.GetSection("connectionStrings")["log"] ?? string.Empty;
            set => config.GetSection("connectionStrings")["log"] = value;
        }

        public string ConnectionStringSimpleHooks
        {
            get => config.GetSection("connectionStrings")["simpleHooks"] ?? string.Empty;
            set => config.GetSection("connectionStrings")["simpleHooks"] = value;
        }

        public string LoggerMinLogLevel
        {
            get => config.GetSection("logger")["min-log-level"] ?? string.Empty;
            set => config.GetSection("logger")["min-log-level"] = value;
        }

        public string LoggerFunction
        {
            get => config.GetSection("logger")["function"] ?? string.Empty;
            set => config.GetSection("logger")["function"] = value;
        }

        public string IdentityServerAuthority
        {
            get => config.GetSection("IdentityServer")["Authority"] ?? string.Empty;
            set => config.GetSection("IdentityServer")["Authority"] = value;
        }

        public string IdentityServerAudience
        {
            get => config.GetSection("IdentityServer")["Audience"] ?? string.Empty;
            set => config.GetSection("IdentityServer")["Audience"] = value;
        }

        public string IdentityServerIntrospectionEndpoint 
        {
            get => config.GetSection("IdentityServer")["IntrospectionEndpoint"] ?? string.Empty;
            set => config.GetSection("IdentityServer")["IntrospectionEndpoint"] = value;
        }

        public string IdentityServerClientId
        {
            get => config.GetSection("IdentityServer")["ClientId"] ?? string.Empty;
            set => config.GetSection("IdentityServer")["ClientId"] = value;
        }

        public string IdentityServerClientSecret
        {
            get => config.GetSection("IdentityServer")["ClientSecret"] ?? string.Empty;
            set => config.GetSection("IdentityServer")["ClientSecret"] = value;
        }
    }
}
