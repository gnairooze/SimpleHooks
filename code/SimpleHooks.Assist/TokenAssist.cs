using SimpleTools.SimpleHooks.Assist.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SimpleTools.SimpleHooks.Assist
{
    internal class TokenAssist
    {
        internal static async Task<string?> GetAccessTokenAsync(HttpClient httpClient, Credentials cred, string scope)
        {
            Console.WriteLine("1. Requesting access token from Identity Server...");

            var tokenRequest = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("client_id", cred.ClientId),
                new KeyValuePair<string, string>("client_secret", cred.ClientSecret),
                new KeyValuePair<string, string>("scope", scope)
            });

            var response = await httpClient.PostAsync($"{cred.IdentityServerUrl}/connect/token", tokenRequest);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Token Response: [SUCCESS - Token details masked for security]");

                // Parse the JSON response to extract the access_token
                var tokenResponse = JsonSerializer.Deserialize<JsonElement>(content);
                if (tokenResponse.TryGetProperty("access_token", out var accessToken))
                {
                    return accessToken.GetString();
                }

                throw new InvalidOperationException($"response without access_token: {tokenResponse}");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Token request failed: {response.StatusCode} - {errorContent}");

                throw new InvalidOperationException($"failed to get token: {response.StatusCode} - {errorContent}");
            }
        }
    }
}
