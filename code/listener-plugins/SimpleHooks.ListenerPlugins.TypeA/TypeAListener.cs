using SimpleTools.SimpleHooks.ListenerInterfaces;
using SimpleTools.SimpleHooks.HttpClient.Interface;
using SimpleTools.SimpleHooks.Log.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json;

namespace SimpleTools.SimpleHooks.ListenerPlugins.TypeA
{
    public class TypeAListener : IListener
    {
        // IListener properties - set by ListenerPluginManager
        public string Url { get; set; }
        public int Timeout { get; set; }
        public List<string> Headers { get; set; }
        public string TypeOptionsValue { get; set; }

        private readonly IHttpClient _httpClient;
        private string _cachedToken;
        private DateTime _tokenExpiration;

        public TypeAListener()
        {
            _httpClient = new SimpleTools.SimpleHooks.HttpClient.Simple.SimpleClient();
        }

        public async Task<ListenerResult> ExecuteAsync(string eventData, string typeOptions)
        {
            var result = new ListenerResult();
            int logCounter = 0;

            try
            {
                // Log start
                result.Logs.Add(logCounter++, new LogModel
                {
                    LogType = LogModel.LogTypes.Information,
                    NotesA = $"TypeA plugin executing call to {Url}",
                    CreateDate = DateTime.UtcNow
                });

                // Parse TypeOptionsValue to get auth configuration
                TypeAOptions authConfig = null;
                try
                {
                    authConfig = JsonSerializer.Deserialize<TypeAOptions>(typeOptions);
                }
                catch (Exception ex)
                {
                    result.Succeeded = false;
                    result.Message = "Failed to parse authentication configuration";
                    result.Logs.Add(logCounter++, new LogModel
                    {
                        LogType = LogModel.LogTypes.Error,
                        NotesA = $"JSON parsing error: {ex.Message}",
                        CreateDate = DateTime.UtcNow
                    });
                    return result;
                }

                if (authConfig == null || string.IsNullOrWhiteSpace(authConfig.IdentityProviderUrl))
                {
                    result.Succeeded = false;
                    result.Message = "Invalid or missing authentication configuration";
                    result.Logs.Add(logCounter++, new LogModel
                    {
                        LogType = LogModel.LogTypes.Error,
                        NotesA = "TypeOptions is missing or invalid",
                        CreateDate = DateTime.UtcNow
                    });
                    return result;
                }

                // Get bearer token (cached or fresh)
                var tokenResult = await GetBearerToken(authConfig, result, logCounter);
                string token = tokenResult.token;
                logCounter = tokenResult.logCounter;

                if (string.IsNullOrWhiteSpace(token))
                {
                    result.Succeeded = false;
                    result.Message = "Failed to obtain bearer token";
                    return result;
                }

                // Add Authorization header
                var headersWithAuth = new List<string>(Headers ?? new List<string>());
                headersWithAuth.Add($"Authorization: Bearer {token}");

                // Make HTTP call
                var httpResult = _httpClient.Post(Url, headersWithAuth, eventData, Timeout);

                // Check result
                if (httpResult.HttpCode >= 200 && httpResult.HttpCode < 300)
                {
                    result.Succeeded = true;
                    result.Message = $"HTTP call succeeded with status code {httpResult.HttpCode}";

                    result.Logs.Add(logCounter++, new LogModel
                    {
                        LogType = LogModel.LogTypes.Information,
                        NotesA = $"Success: {httpResult.HttpCode} - {httpResult.Body}",
                        CreateDate = DateTime.UtcNow
                    });
                }
                else if (httpResult.HttpCode == 401)
                {
                    // Token might be expired, try refreshing
                    result.Logs.Add(logCounter++, new LogModel
                    {
                        LogType = LogModel.LogTypes.Warning,
                        NotesA = "Received 401, attempting token refresh",
                        CreateDate = DateTime.UtcNow
                    });

                    _cachedToken = null; // Invalidate cache
                    tokenResult = await GetBearerToken(authConfig, result, logCounter);
                    token = tokenResult.token;
                    logCounter = tokenResult.logCounter;

                    // Retry with new token
                    headersWithAuth = new List<string>(Headers ?? new List<string>());
                    headersWithAuth.Add($"Authorization: Bearer {token}");
                    httpResult = _httpClient.Post(Url, headersWithAuth, eventData, Timeout);

                    if (httpResult.HttpCode >= 200 && httpResult.HttpCode < 300)
                    {
                        result.Succeeded = true;
                        result.Message = $"HTTP call succeeded after token refresh: {httpResult.HttpCode}";
                    }
                    else
                    {
                        result.Succeeded = false;
                        result.Message = $"HTTP call failed after token refresh: {httpResult.HttpCode}";
                    }

                    result.Logs.Add(logCounter++, new LogModel
                    {
                        LogType = LogModel.LogTypes.Information,
                        NotesA = $"Retry result: {httpResult.HttpCode}",
                        CreateDate = DateTime.UtcNow
                    });
                }
                else
                {
                    result.Succeeded = false;
                    result.Message = $"HTTP call failed with status code {httpResult.HttpCode}";

                    result.Logs.Add(logCounter++, new LogModel
                    {
                        LogType = LogModel.LogTypes.Error,
                        NotesA = $"Failed: {httpResult.HttpCode} - {httpResult.Body}",
                        CreateDate = DateTime.UtcNow
                    });
                }
            }
            catch (Exception ex)
            {
                result.Succeeded = false;
                result.Message = $"Exception during execution: {ex.Message}";

                result.Logs.Add(logCounter++, new LogModel
                {
                    LogType = LogModel.LogTypes.Error,
                    NotesA = $"Exception: {ex.Message}\n{ex.StackTrace}",
                    CreateDate = DateTime.UtcNow
                });
            }

            return result;
        }

        private async Task<(string token, int logCounter)> GetBearerToken(TypeAOptions config, ListenerResult result, int logCounter)
        {
            // Check cache
            if (!string.IsNullOrWhiteSpace(_cachedToken) && DateTime.UtcNow < _tokenExpiration)
            {
                result.Logs.Add(logCounter++, new LogModel
                {
                    LogType = LogModel.LogTypes.Information,
                    NotesA = "Using cached bearer token",
                    CreateDate = DateTime.UtcNow
                });
                return (_cachedToken, logCounter);
            }

            try
            {
                // Request new token
                result.Logs.Add(logCounter++, new LogModel
                {
                    LogType = LogModel.LogTypes.Information,
                    NotesA = $"Requesting bearer token from {config.IdentityProviderUrl}",
                    CreateDate = DateTime.UtcNow
                });

                var tokenRequestBody = $"grant_type=client_credentials&client_id={config.ClientId}&client_secret={config.ClientSecret}&scope={config.Scope}";
                var tokenHeaders = new List<string> { "Content-Type: application/x-www-form-urlencoded" };

                var tokenHttpResult = _httpClient.Post(config.IdentityProviderUrl, tokenHeaders, tokenRequestBody, 5);

                if (tokenHttpResult.HttpCode >= 200 && tokenHttpResult.HttpCode < 300)
                {
                    var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(tokenHttpResult.Body);
                    _cachedToken = tokenResponse.AccessToken;
                    _tokenExpiration = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn - 60); // Refresh 60 seconds before expiry

                    result.Logs.Add(logCounter++, new LogModel
                    {
                        LogType = LogModel.LogTypes.Information,
                        NotesA = "Bearer token obtained successfully",
                        CreateDate = DateTime.UtcNow
                    });

                    return (_cachedToken, logCounter);
                }
                else
                {
                    result.Logs.Add(logCounter++, new LogModel
                    {
                        LogType = LogModel.LogTypes.Error,
                        NotesA = $"Failed to obtain token: {tokenHttpResult.HttpCode} - {tokenHttpResult.Body}",
                        CreateDate = DateTime.UtcNow
                    });
                    return (null, logCounter);
                }
            }
            catch (Exception ex)
            {
                result.Logs.Add(logCounter++, new LogModel
                {
                    LogType = LogModel.LogTypes.Error,
                    NotesA = $"Exception obtaining token: {ex.Message}",
                    CreateDate = DateTime.UtcNow
                });
                return (null, logCounter);
            }
        }
    }

    // Configuration model
    public class TypeAOptions
    {
        public string IdentityProviderUrl { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Scope { get; set; }
    }

    // Token response model
    public class TokenResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
    }
}

