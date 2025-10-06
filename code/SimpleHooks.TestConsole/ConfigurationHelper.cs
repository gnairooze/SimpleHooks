using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace SimpleTools.SimpleHooks.TestConsole
{
    internal class ConfigurationHelper(IConfigurationRoot config)
    {
        private readonly IConfigurationRoot _config = config;

        public string IdentityServerUrl => _config["identity-server-url"] ?? string.Empty;

        public string TriggerEventAnonymousUrl => _config["operations:trigger-event:anonymous-url"] ?? string.Empty;
        public string TriggerEventAuthenticatedUrl => _config["operations:trigger-event:authenticated-url"] ?? string.Empty;
        public string TriggerEventClientId => _config["operations:trigger-event:client-id"] ?? string.Empty;
        public string TriggerEventClientSecret => _config["operations:trigger-event:client-secret"] ?? string.Empty;

        public string CheckEventInstanceStatusAnonymousUrl => _config["operations:check-event-instance-status:anonymous-url"] ?? string.Empty;
        public string CheckEventInstanceStatusAuthenticatedUrl => _config["operations:check-event-instance-status:authenticated-url"] ?? string.Empty;
        public string CheckEventInstanceStatusClientId => _config["operations:check-event-instance-status:client-id"] ?? string.Empty;
        public string CheckEventInstanceStatusClientSecret => _config["operations:check-event-instance-status:client-secret"] ?? string.Empty;

        public string LoadDefinitionsAnonymousUrl => _config["operations:load-definitions:anonymous-url"] ?? string.Empty;
        public string LoadDefinitionsAuthenticatedUrl => _config["operations:load-definitions:authenticated-url"] ?? string.Empty;
        public string LoadDefinitionsClientId => _config["operations:load-definitions:client-id"] ?? string.Empty;
        public string LoadDefinitionsClientSecret => _config["operations:load-definitions:client-secret"] ?? string.Empty;
    }
}
