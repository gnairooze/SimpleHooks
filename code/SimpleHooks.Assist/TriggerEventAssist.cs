using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using SimpleTools.SimpleHooks.Assist.Models;

namespace SimpleTools.SimpleHooks.Assist
{
    public class TriggerEventAssist
    {
        private readonly HttpClient _httpClient;
        private readonly string _url;
        private readonly bool _useAuthenticatedApi;

        public TriggerEventAssist(string url, bool useAuthenticatedApi = false)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentNullException(nameof(url));

            _httpClient = new HttpClient();

            _url = url;
            _useAuthenticatedApi = useAuthenticatedApi;
        }
        
        public async Task<string> ExecuteAsync(TriggerEventRequest eventRequestData, Credentials? cred = null)
        {
            try
            {
                var request = new
                {
                    eventRequestData.EventDefinitionId,
                    eventRequestData.ReferenceName,
                    eventRequestData.ReferenceValue,
                    EventData = (eventRequestData.EventData is string eventDataString) ? JsonSerializer.Deserialize<JsonElement>(eventDataString) : eventRequestData.EventData
                };

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                if(_useAuthenticatedApi)
                {
                    if(cred == null)
                        throw new ArgumentNullException(nameof(cred), "Credentials must be provided for authenticated API calls.");

                    var token = await TokenAssist.GetAccessTokenAsync(_httpClient, cred, "simplehooks_api.trigger_event");
                    if (string.IsNullOrWhiteSpace(token))
                        throw new Exception("Failed to obtain access token.");

                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                var response = await _httpClient.PostAsync(_url, content);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Failed to trigger event: {ex.Message}", ex);
            }
        }
    }
} 