using SimpleTools.SimpleHooks.ListenerInterfaces;
using SimpleTools.SimpleHooks.ListenerPlugins.TypeA;
using SimpleTools.SimpleHooks.HttpClient.Interface;
using SimpleTools.SimpleHooks.TestListeners.Mocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace SimpleTools.SimpleHooks.TestListeners
{
    public class TypeAPluginTests
    {
        private const string ValidAuthOptions = @"{
            ""identityProviderUrl"": ""https://auth.example.com/token"",
            ""clientId"": ""test-client"",
            ""clientSecret"": ""test-secret"",
            ""scope"": ""api.read api.write""
        }";

        [Fact]
        public async Task ExecuteAsync_ValidAuth_GetsTokenAndExecutes()
        {
            // Arrange
            var tokenResponse = JsonSerializer.Serialize(new
            {
                access_token = "test-bearer-token",
                expires_in = 3600
            });

            var mockHttpClient = new MockHttpClientWithSequence(new[]
            {
                new HttpResult { HttpCode = 200, Body = tokenResponse, Headers = new List<string>() }, // Token request
                new HttpResult { HttpCode = 200, Body = "Webhook success", Headers = new List<string>() } // Webhook call
            });

            var plugin = new TypeAListenerTestable(mockHttpClient)
            {
                Url = "https://api.example.com/webhook",
                Timeout = 5,
                Headers = new List<string> { "Content-Type: application/json" }
            };

            // Act
            var result = await plugin.ExecuteAsync(1, "{\"event\":\"test\"}", ValidAuthOptions);

            // Assert
            Assert.True(result.Succeeded);
            Assert.Contains("succeeded", result.Message, StringComparison.OrdinalIgnoreCase);
            Assert.NotEmpty(result.Logs);

            // Verify Authorization header was added
            var webhookHeaders = mockHttpClient.GetCallHeaders(1);
            Assert.NotNull(webhookHeaders);
            Assert.Contains(webhookHeaders, h => h.StartsWith("Authorization: Bearer"));
        }

        [Fact]
        public async Task ExecuteAsync_InvalidCredentials_ReturnsFailure()
        {
            // Arrange
            var mockHttpClient = new MockHttpClientWithSequence(new[]
            {
                new HttpResult { HttpCode = 401, Body = "Unauthorized", Headers = new List<string>() } // Token request fails
            });

            var plugin = new TypeAListenerTestable(mockHttpClient)
            {
                Url = "https://api.example.com/webhook",
                Timeout = 5,
                Headers = new List<string>()
            };

            // Act
            var result = await plugin.ExecuteAsync(1, "{\"event\":\"test\"}", ValidAuthOptions);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Contains("Failed to obtain bearer token", result.Message);
            Assert.NotEmpty(result.Logs);
        }

        [Fact]
        public async Task ExecuteAsync_TokenExpired_RefreshesToken()
        {
            // Arrange
            var firstToken = JsonSerializer.Serialize(new
            {
                access_token = "first-token",
                expires_in = -1 // Already expired
            });

            var newToken = JsonSerializer.Serialize(new
            {
                access_token = "refreshed-token",
                expires_in = 3600
            });

            var mockHttpClient = new MockHttpClientWithSequence(new[]
            {
                new HttpResult { HttpCode = 200, Body = firstToken, Headers = new List<string>() }, // Initial token
                new HttpResult { HttpCode = 401, Body = "Unauthorized", Headers = new List<string>() }, // First call fails
                new HttpResult { HttpCode = 200, Body = newToken, Headers = new List<string>() }, // Token refresh
                new HttpResult { HttpCode = 200, Body = "Success", Headers = new List<string>() } // Retry succeeds
            });

            var plugin = new TypeAListenerTestable(mockHttpClient)
            {
                Url = "https://api.example.com/webhook",
                Timeout = 5,
                Headers = new List<string>()
            };

            // Act
            var result = await plugin.ExecuteAsync(1, "{\"event\":\"test\"}", ValidAuthOptions);

            // Assert
            Assert.True(result.Succeeded);
            Assert.Contains("token refresh", result.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(4, mockHttpClient.CallCount);
        }

        [Fact]
        public async Task ExecuteAsync_MalformedOptions_ReturnsFailure()
        {
            // Arrange
            var mockHttpClient = new MockHttpClientWithSequence(new HttpResult[0]);

            var plugin = new TypeAListenerTestable(mockHttpClient)
            {
                Url = "https://api.example.com/webhook",
                Timeout = 5,
                Headers = new List<string>()
            };

            // Act
            var result = await plugin.ExecuteAsync(1, "{\"event\":\"test\"}", "invalid-json{{{");

            // Assert
            Assert.False(result.Succeeded);
            Assert.NotEmpty(result.Logs);
        }

        [Fact]
        public async Task ExecuteAsync_MissingRequiredOptions_ReturnsFailure()
        {
            // Arrange
            var incompleteOptions = @"{
                ""identityProviderUrl"": ""https://auth.example.com/token""
            }"; // Missing clientId and clientSecret

            var mockHttpClient = new MockHttpClientWithSequence(new HttpResult[0]);

            var plugin = new TypeAListenerTestable(mockHttpClient)
            {
                Url = "https://api.example.com/webhook",
                Timeout = 5,
                Headers = new List<string>()
            };

            // Act
            var result = await plugin.ExecuteAsync(1, "{\"event\":\"test\"}", incompleteOptions);

            // Assert
            Assert.False(result.Succeeded);
            Assert.NotEmpty(result.Logs);
        }

        [Fact]
        public async Task ExecuteAsync_IdentityProviderDown_ReturnsFailure()
        {
            // Arrange
            var mockHttpClient = new MockHttpClientWithSequence(new[]
            {
                new HttpResult { HttpCode = 503, Body = "Service Unavailable", Headers = new List<string>() }
            });

            var plugin = new TypeAListenerTestable(mockHttpClient)
            {
                Url = "https://api.example.com/webhook",
                Timeout = 5,
                Headers = new List<string>()
            };

            // Act
            var result = await plugin.ExecuteAsync(1, "{\"event\":\"test\"}", ValidAuthOptions);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Contains("Failed to obtain bearer token", result.Message);
        }

        [Fact]
        public async Task ExecuteAsync_EmptyTypeOptionsValue_ReturnsFailure()
        {
            // Arrange
            var mockHttpClient = new MockHttpClientWithSequence(new HttpResult[0]);

            var plugin = new TypeAListenerTestable(mockHttpClient)
            {
                Url = "https://api.example.com/webhook",
                Timeout = 5,
                Headers = new List<string>()
            };

            // Act
            var result = await plugin.ExecuteAsync(1, "{\"event\":\"test\"}", "");

            // Assert
            Assert.False(result.Succeeded);
            Assert.Contains("Invalid or missing authentication configuration", result.Message);
        }

        [Fact]
        public async Task ExecuteAsync_TokenCaching_ReusesCachedToken()
        {
            // Arrange
            var tokenResponse = JsonSerializer.Serialize(new
            {
                access_token = "cached-token",
                expires_in = 3600
            });

            var mockHttpClient = new MockHttpClientWithSequence(new[]
            {
                new HttpResult { HttpCode = 200, Body = tokenResponse, Headers = new List<string>() }, // Token request
                new HttpResult { HttpCode = 200, Body = "Success 1", Headers = new List<string>() }, // First webhook call
                new HttpResult { HttpCode = 200, Body = "Success 2", Headers = new List<string>() } // Second webhook call (should reuse token)
            });

            var plugin = new TypeAListenerTestable(mockHttpClient)
            {
                Url = "https://api.example.com/webhook",
                Timeout = 5,
                Headers = new List<string>()
            };

            // Act
            var result1 = await plugin.ExecuteAsync(1, "{\"event\":\"test1\"}", ValidAuthOptions);
            var result2 = await plugin.ExecuteAsync(2, "{\"event\":\"test2\"}", ValidAuthOptions);

            // Assert
            Assert.True(result1.Succeeded);
            Assert.True(result2.Succeeded);
            Assert.Equal(3, mockHttpClient.CallCount); // 1 token request + 2 webhook calls (token reused)
        }
    }

    // Testable version that accepts mock HTTP client
    public class TypeAListenerTestable : IListener
    {
        private readonly IHttpClient _httpClient;
        private string? _cachedToken;
        private DateTime _tokenExpiration;

        public string Url { get; set; } = string.Empty;
        public int Timeout { get; set; }
        public List<string> Headers { get; set; } = new List<string>();

        public TypeAListenerTestable(IHttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<ListenerResult> ExecuteAsync(long listenerInstanceId, string eventData, string typeOptionsValue)
        {
            var result = new ListenerResult();
            int logCounter = 0;

            try
            {
                result.Logs.Add(logCounter++, new Log.Interface.LogModel
                {
                    NotesA = $"TypeA plugin executing call to {Url}",
                    CreateDate = DateTime.Now
                });

                TypeAOptions? authConfig;
                try
                {
                    authConfig = JsonSerializer.Deserialize<TypeAOptions>(typeOptionsValue);
                }
                catch (JsonException)
                {
                    result.Succeeded = false;
                    result.Message = "Invalid JSON in authentication configuration";
                    result.Logs.Add(logCounter++, new Log.Interface.LogModel
                    {
                        NotesA = "Failed to parse TypeOptionsValue as JSON",
                        CreateDate = DateTime.Now
                    });
                    return result;
                }

                if (authConfig == null || string.IsNullOrWhiteSpace(authConfig.IdentityProviderUrl))
                {
                    result.Succeeded = false;
                    result.Message = "Invalid or missing authentication configuration";
                    result.Logs.Add(logCounter++, new Log.Interface.LogModel
                    {
                        NotesA = "TypeOptionsValue is missing or invalid",
                        CreateDate = DateTime.Now
                    });
                    return result;
                }

                string? token = GetBearerToken(authConfig, result, ref logCounter);

                if (string.IsNullOrWhiteSpace(token))
                {
                    result.Succeeded = false;
                    result.Message = "Failed to obtain bearer token";
                    return result;
                }

                var headersWithAuth = new List<string>(Headers ?? new List<string>());
                headersWithAuth.Add($"Authorization: Bearer {token}");

                var httpResult = _httpClient.Post(Url, headersWithAuth, eventData, Timeout);

                if (httpResult.HttpCode >= 200 && httpResult.HttpCode < 300)
                {
                    result.Succeeded = true;
                    result.Message = $"HTTP call succeeded with status code {httpResult.HttpCode}";

                    result.Logs.Add(logCounter++, new Log.Interface.LogModel
                    {
                        NotesA = $"Success: {httpResult.HttpCode} - {httpResult.Body}",
                        CreateDate = DateTime.Now
                    });
                }
                else if (httpResult.HttpCode == 401)
                {
                    result.Logs.Add(logCounter++, new Log.Interface.LogModel
                    {
                        NotesA = "Received 401, attempting token refresh",
                        CreateDate = DateTime.Now
                    });

                    _cachedToken = null;
                    token = GetBearerToken(authConfig, result, ref logCounter);

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

                    result.Logs.Add(logCounter++, new Log.Interface.LogModel
                    {
                        NotesA = $"Retry result: {httpResult.HttpCode}",
                        CreateDate = DateTime.Now
                    });
                }
                else
                {
                    result.Succeeded = false;
                    result.Message = $"HTTP call failed with status code {httpResult.HttpCode}";

                    result.Logs.Add(logCounter++, new Log.Interface.LogModel
                    {
                        NotesA = $"Failed: {httpResult.HttpCode} - {httpResult.Body}",
                        CreateDate = DateTime.Now
                    });
                }
            }
            catch (Exception ex)
            {
                result.Succeeded = false;
                result.Message = $"Exception during execution: {ex.Message}";

                result.Logs.Add(logCounter++, new Log.Interface.LogModel
                {
                    NotesA = $"Exception: {ex.Message}\n{ex.StackTrace}",
                    CreateDate = DateTime.Now
                });
            }

            return result;
        }

        private string? GetBearerToken(TypeAOptions config, ListenerResult result, ref int logCounter)
        {
            if (!string.IsNullOrWhiteSpace(_cachedToken) && DateTime.Now < _tokenExpiration)
            {
                result.Logs.Add(logCounter++, new Log.Interface.LogModel
                {
                    NotesA = "Using cached bearer token",
                    CreateDate = DateTime.Now
                });
                return _cachedToken;
            }

            try
            {
                result.Logs.Add(logCounter++, new Log.Interface.LogModel
                {
                    NotesA = $"Requesting bearer token from {config.IdentityProviderUrl}",
                    CreateDate = DateTime.Now
                });

                var tokenRequestBody = $"grant_type=client_credentials&client_id={config.ClientId}&client_secret={config.ClientSecret}&scope={config.Scope}";
                var tokenHeaders = new List<string> { "Content-Type: application/x-www-form-urlencoded" };

                var tokenResult = _httpClient.Post(config.IdentityProviderUrl, tokenHeaders, tokenRequestBody, 5);

                if (tokenResult.HttpCode >= 200 && tokenResult.HttpCode < 300)
                {
                    var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(tokenResult.Body);
                    if (tokenResponse != null)
                    {
                        _cachedToken = tokenResponse.AccessToken;
                        _tokenExpiration = DateTime.Now.AddSeconds(tokenResponse.ExpiresIn - 60);

                        result.Logs.Add(logCounter++, new Log.Interface.LogModel
                        {
                            NotesA = "Bearer token obtained successfully",
                            CreateDate = DateTime.Now
                        });

                        return _cachedToken;
                    }
                }

                result.Logs.Add(logCounter++, new Log.Interface.LogModel
                {
                    NotesA = $"Failed to obtain token: {tokenResult.HttpCode} - {tokenResult.Body}",
                    CreateDate = DateTime.Now
                });
                return null;
            }
            catch (Exception ex)
            {
                result.Logs.Add(logCounter++, new Log.Interface.LogModel
                {
                    NotesA = $"Exception obtaining token: {ex.Message}",
                    CreateDate = DateTime.Now
                });
                return null;
            }
        }
    }

    // Mock HTTP client that returns different results for sequential calls
    public class MockHttpClientWithSequence : IHttpClient
    {
        private readonly List<HttpResult> _results;
        private int _callIndex = 0;
        private readonly List<CallRecord> _callRecords = new List<CallRecord>();

        public int CallCount => _callIndex;

        public MockHttpClientWithSequence(HttpResult[] results)
        {
            _results = results.ToList();
        }

        public HttpResult Post(string url, List<string> headers, string body, int timeout)
        {
            _callRecords.Add(new CallRecord
            {
                Url = url,
                Headers = new List<string>(headers),
                Body = body,
                Timeout = timeout
            });

            if (_callIndex >= _results.Count)
            {
                return new HttpResult
                {
                    HttpCode = 500,
                    Body = "No more mock results configured",
                    Headers = new List<string>()
                };
            }

            return _results[_callIndex++];
        }

        public List<string>? GetCallHeaders(int callIndex)
        {
            if (callIndex < 0 || callIndex >= _callRecords.Count)
                return null;
            return _callRecords[callIndex].Headers;
        }

        private class CallRecord
        {
            public string Url { get; set; } = string.Empty;
            public List<string> Headers { get; set; } = new List<string>();
            public string Body { get; set; } = string.Empty;
            public int Timeout { get; set; }
        }
    }
}

