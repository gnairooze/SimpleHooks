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
    public class LoadDefinitionsAssist
    {
        private readonly HttpClient _httpClient;
        private readonly string _url;
        private readonly bool _useAuthenticatedApi;

        public LoadDefinitionsAssist(string url, bool useAuthenticatedApi = false)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentNullException(nameof(url));

            _httpClient = new HttpClient();

            _url = url;
            _useAuthenticatedApi = useAuthenticatedApi;
        }

        public async Task<string> ExecuteAsync(Credentials? cred = null)
        {
            try
            {
                if (_useAuthenticatedApi)
                {
                    if (cred == null)
                        throw new ArgumentNullException(nameof(cred), "Credentials must be provided for authenticated API calls.");

                    var token = await TokenAssist.GetAccessTokenAsync(_httpClient, cred, "simplehooks_api.load_definitions");
                    if (string.IsNullOrWhiteSpace(token))
                        throw new Exception("Failed to obtain access token.");

                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                var response = await _httpClient.PostAsync(_url, null);
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
