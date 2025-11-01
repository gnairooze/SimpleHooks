using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SimpleTools.SimpleHooks.ListenerPlugins.TypeA
{
    /*
     {
         "identityProviderUrl": "https://auth.example.com/token",
         "clientId": "client_id",
         "clientSecret": "client_secret",
         "scope": "api.read api.write"
       }
     */
    public class TypeAOptions
    {
        [JsonPropertyName("identityProviderUrl")]
        public string IdentityProviderUrl { get; set; } = string.Empty;
        [JsonPropertyName("clientId")]
        public string ClientId { get; set; } = string.Empty;
        [JsonPropertyName("clientSecret")]
        public string ClientSecret { get; set; } = string.Empty;
        [JsonPropertyName("scope")]
        public string Scope { get; set; } = string.Empty;
    }
}
