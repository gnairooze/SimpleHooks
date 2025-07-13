using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SimpleTools.SimpleHooks.TriggerEventHelper
{
    public class TriggerEventClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _url;

        public TriggerEventClient(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentNullException(nameof(url));

            _url = url;
            _httpClient = new HttpClient();
        }

        /// <summary>
        /// Triggers an event by sending a POST request to the SimpleHooks.Web TriggerEventController
        /// </summary>
        /// <param name="eventDefinitionId">The ID of the event definition</param>
        /// <param name="referenceName">The reference name for the event</param>
        /// <param name="referenceValue">The reference value for the event</param>
        /// <param name="eventData">The event data as a JSON object</param>
        /// <returns>The response from the server</returns>
        public async Task<string> TriggerEventAsync(long eventDefinitionId, string referenceName, string referenceValue, object eventData)
        {
            try
            {
                var request = new
                {
                    eventDefinitionId,
                    referenceName,
                    referenceValue,
                    EventData = eventData
                };

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(_url, content);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Failed to trigger event: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Triggers an event by sending a POST request to the SimpleHooks.Web TriggerEventController
        /// </summary>
        /// <param name="eventDefinitionId">The ID of the event definition</param>
        /// <param name="referenceName">The reference name for the event</param>
        /// <param name="referenceValue">The reference value for the event</param>
        /// <param name="eventData">The event data as a JSON string</param>
        /// <returns>The response from the server</returns>
        public async Task<string> TriggerEventAsync(long eventDefinitionId, string referenceName, string referenceValue, string eventData)
        {
            try
            {
                var request = new
                {
                    eventDefinitionId,
                    referenceName,
                    referenceValue,
                    EventData = JsonSerializer.Deserialize<JsonElement>(eventData)
                };

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_url}/api/TriggerEvent", content);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Failed to trigger event: {ex.Message}", ex);
            }
            catch (JsonException ex)
            {
                throw new Exception($"Invalid event data JSON: {ex.Message}", ex);
            }
        }
    }
} 