using SimpleTools.SimpleHooks.Assist.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SimpleTools.SimpleHooks.Assist
{
    public class CheckEventInstanceStatusAssist
    {
        private readonly HttpClient _httpClient;
        private readonly string _url;
        private readonly bool _useAuthenticatedApi;

        public CheckEventInstanceStatusAssist (string url, bool useAuthenticatedApi = false)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentNullException(nameof(url));

            _httpClient = new HttpClient();

            _url = url;
            _useAuthenticatedApi = useAuthenticatedApi;
        }

        public async Task<int> ExecuteAsync(string eventInstanceBusinessId, Credentials? cred = null)
        {
            try
            {
                if (_useAuthenticatedApi)
                {
                    if (cred == null)
                        throw new ArgumentNullException(nameof(cred), "Credentials must be provided for authenticated API calls.");

                    var token = await TokenAssist.GetAccessTokenAsync(_httpClient, cred, "simplehooks_api.get_event_instance_status");
                    if (string.IsNullOrWhiteSpace(token))
                        throw new Exception("Failed to obtain access token.");

                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                var response = await _httpClient.GetAsync($"{_url}?businessid={eventInstanceBusinessId}");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();

                var statusResponse = JsonSerializer.Deserialize<JsonElement>(content);

                if (statusResponse.TryGetProperty("status", out var status))
                {
                    var statusInteger = status.GetInt32();

                    return statusInteger!;
                }

                throw new InvalidOperationException($"response without status: {statusResponse}");
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Failed to trigger event: {ex.Message}", ex);
            }
        }
    }
}
