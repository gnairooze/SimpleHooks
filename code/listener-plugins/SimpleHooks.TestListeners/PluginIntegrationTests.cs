using SimpleTools.SimpleHooks.ListenerInterfaces;
using SimpleTools.SimpleHooks.HttpClient.Interface;
using SimpleTools.SimpleHooks.TestListeners.Mocks;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using SimpleTools.SimpleHooks.ListenerPlugins.TypeA;

namespace SimpleTools.SimpleHooks.TestListeners
{
    public class PluginIntegrationTests
    {
        [Fact]
        public async Task EndToEnd_AnonymousPlugin_ProcessesEvent()
        {
            // Arrange
            var mockHttpClient = new MockHttpClient
            {
                MockResult = new HttpResult
                {
                    HttpCode = 200,
                    Body = "Event processed successfully",
                    Headers = new List<string>()
                }
            };

            var plugin = new AnonymousListenerTestable(mockHttpClient)
            {
                Url = "https://webhook.example.com/events",
                Timeout = 10,
                Headers = new List<string>
                {
                    "Content-Type: application/json",
                    "X-Event-Source: SimpleHooks"
                },
                TypeOptionsValue = ""
            };

            var eventData = JsonSerializer.Serialize(new
            {
                eventId = "evt_123",
                eventType = "user.created",
                timestamp = DateTime.UtcNow,
                data = new { userId = 456, username = "testuser" }
            });

            // Act
            var result = await plugin.ExecuteAsync(eventData, "");

            // Assert
            Assert.True(result.Succeeded);
            Assert.Contains("succeeded", result.Message, StringComparison.OrdinalIgnoreCase);
            Assert.NotEmpty(result.Logs);
            Assert.Equal("https://webhook.example.com/events", mockHttpClient.LastUrl);
            Assert.Equal(eventData, mockHttpClient.LastBody);
        }

        [Fact]
        public async Task EndToEnd_TypeAPlugin_ProcessesEventWithAuth()
        {
            // Arrange
            var authOptions = JsonSerializer.Serialize(new
            {
                identityProviderUrl = "https://auth.example.com/oauth/token",
                clientId = "test-client-id",
                clientSecret = "test-client-secret",
                scope = "webhooks.write"
            });

            var tokenResponse = JsonSerializer.Serialize(new
            {
                access_token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
                expires_in = 3600,
                token_type = "Bearer"
            });

            var mockHttpClient = new MockHttpClientWithSequence(new[]
            {
                new HttpResult { HttpCode = 200, Body = tokenResponse, Headers = new List<string>() },
                new HttpResult { HttpCode = 201, Body = "Event created", Headers = new List<string>() }
            });

            var plugin = new TypeAListenerTestable(mockHttpClient)
            {
                Url = "https://api.example.com/webhooks/events",
                Timeout = 10,
                Headers = new List<string> { "Content-Type: application/json" },
                TypeOptionsValue = authOptions
            };

            var eventData = JsonSerializer.Serialize(new
            {
                eventId = "evt_456",
                eventType = "order.completed",
                timestamp = DateTime.UtcNow,
                data = new { orderId = 789, total = 99.99 }
            });

            // Act
            var result = await plugin.ExecuteAsync(eventData, authOptions);

            // Assert
            Assert.True(result.Succeeded);
            Assert.Contains("succeeded", result.Message, StringComparison.OrdinalIgnoreCase);
            Assert.NotEmpty(result.Logs);

            // Verify token was obtained and used
            var webhookHeaders = mockHttpClient.GetCallHeaders(1);
            Assert.NotNull(webhookHeaders);
            Assert.Contains(webhookHeaders, h => h.Contains("Authorization: Bearer"));
        }

        [Fact]
        public void Configuration_EnvironmentVariables_LoadedCorrectly()
        {
            // Arrange
            var envVarName = "TEST_LISTENER_CONFIG";
            var configValue = JsonSerializer.Serialize(new
            {
                identityProviderUrl = "https://test-auth.example.com/token",
                clientId = "env-test-client",
                clientSecret = "env-test-secret",
                scope = "test.scope"
            });

            Environment.SetEnvironmentVariable(envVarName, configValue);

            try
            {
                // Act
                var retrievedValue = Environment.GetEnvironmentVariable(envVarName);

                // Assert
                Assert.NotNull(retrievedValue);
                Assert.Equal(configValue, retrievedValue);

                var parsedConfig = JsonSerializer.Deserialize<TypeAOptions>(retrievedValue);
                Assert.NotNull(parsedConfig);
                Assert.Equal("https://test-auth.example.com/token", parsedConfig!.IdentityProviderUrl);
                Assert.Equal("env-test-client", parsedConfig.ClientId);
            }
            finally
            {
                // Cleanup
                Environment.SetEnvironmentVariable(envVarName, null);
            }
        }

        [Fact]
        public async Task ErrorHandling_PluginException_ReturnsFailureResult()
        {
            // Arrange
            var mockHttpClient = new MockHttpClient
            {
                ShouldThrow = true,
                ExceptionToThrow = new InvalidOperationException("Simulated plugin failure")
            };

            var plugin = new AnonymousListenerTestable(mockHttpClient)
            {
                Url = "https://api.example.com/webhook",
                Timeout = 5,
                Headers = new List<string>(),
                TypeOptionsValue = ""
            };

            // Act
            var result = await plugin.ExecuteAsync("{\"test\":\"data\"}", "");

            // Assert
            Assert.False(result.Succeeded);
            Assert.Contains("Exception", result.Message);
            Assert.NotEmpty(result.Logs);

            var lastLog = result.Logs[result.Logs.Count - 1];
            Assert.Contains("Simulated plugin failure", lastLog.NotesA);
        }

        [Fact]
        public async Task ErrorHandling_NetworkFailure_ReturnsFailureResult()
        {
            // Arrange
            var mockHttpClient = new MockHttpClient
            {
                ShouldThrow = true,
                ExceptionToThrow = new HttpRequestException("Network unreachable")
            };

            var plugin = new AnonymousListenerTestable(mockHttpClient)
            {
                Url = "https://unreachable.example.com/webhook",
                Timeout = 5,
                Headers = new List<string>(),
                TypeOptionsValue = ""
            };

            // Act
            var result = await plugin.ExecuteAsync("{\"test\":\"data\"}", "");

            // Assert
            Assert.False(result.Succeeded);
            Assert.Contains("Exception", result.Message);
            Assert.NotEmpty(result.Logs);
        }

        [Fact]
        public async Task MultipleExecutions_SamePlugin_MaintainsState()
        {
            // Arrange
            var authOptions = JsonSerializer.Serialize(new
            {
                identityProviderUrl = "https://auth.example.com/token",
                clientId = "test-client",
                clientSecret = "test-secret",
                scope = "api.access"
            });

            var tokenResponse = JsonSerializer.Serialize(new
            {
                access_token = "reusable-token",
                expires_in = 7200
            });

            var mockHttpClient = new MockHttpClientWithSequence(new[]
            {
                new HttpResult { HttpCode = 200, Body = tokenResponse, Headers = new List<string>() }, // Token request
                new HttpResult { HttpCode = 200, Body = "Success 1", Headers = new List<string>() }, // First execution
                new HttpResult { HttpCode = 200, Body = "Success 2", Headers = new List<string>() }, // Second execution (cached token)
                new HttpResult { HttpCode = 200, Body = "Success 3", Headers = new List<string>() }  // Third execution (cached token)
            });

            var plugin = new TypeAListenerTestable(mockHttpClient)
            {
                Url = "https://api.example.com/webhook",
                Timeout = 5,
                Headers = new List<string>(),
                TypeOptionsValue = authOptions
            };

            // Act
            var result1 = await plugin.ExecuteAsync("{\"event\":1}", authOptions);
            var result2 = await plugin.ExecuteAsync("{\"event\":2}", authOptions);
            var result3 = await plugin.ExecuteAsync("{\"event\":3}", authOptions);

            // Assert
            Assert.True(result1.Succeeded);
            Assert.True(result2.Succeeded);
            Assert.True(result3.Succeeded);

            // Verify token was only requested once (reused for subsequent calls)
            Assert.Equal(4, mockHttpClient.CallCount); // 1 token + 3 webhook calls
        }

        [Fact]
        public async Task Retry_After401_RefreshesTokenAndSucceeds()
        {
            // Arrange
            var authOptions = JsonSerializer.Serialize(new
            {
                identityProviderUrl = "https://auth.example.com/token",
                clientId = "test-client",
                clientSecret = "test-secret",
                scope = "api.access"
            });

            var initialToken = JsonSerializer.Serialize(new
            {
                access_token = "expired-token",
                expires_in = 3600
            });

            var newToken = JsonSerializer.Serialize(new
            {
                access_token = "fresh-token",
                expires_in = 3600
            });

            var mockHttpClient = new MockHttpClientWithSequence(new[]
            {
                new HttpResult { HttpCode = 200, Body = initialToken, Headers = new List<string>() }, // Initial token
                new HttpResult { HttpCode = 401, Body = "Token expired", Headers = new List<string>() }, // First attempt fails
                new HttpResult { HttpCode = 200, Body = newToken, Headers = new List<string>() }, // Token refresh
                new HttpResult { HttpCode = 200, Body = "Success", Headers = new List<string>() } // Retry succeeds
            });

            var plugin = new TypeAListenerTestable(mockHttpClient)
            {
                Url = "https://api.example.com/webhook",
                Timeout = 5,
                Headers = new List<string>(),
                TypeOptionsValue = authOptions
            };

            // Act
            var result = await plugin.ExecuteAsync("{\"event\":\"test\"}", authOptions);

            // Assert
            Assert.True(result.Succeeded);
            Assert.Contains("token refresh", result.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(4, mockHttpClient.CallCount);
        }

        [Fact]
        public async Task ConcurrentExecutions_DifferentPluginInstances_WorkIndependently()
        {
            // Arrange
            var mockHttpClient1 = new MockHttpClient
            {
                MockResult = new HttpResult { HttpCode = 200, Body = "Success 1", Headers = new List<string>() }
            };

            var mockHttpClient2 = new MockHttpClient
            {
                MockResult = new HttpResult { HttpCode = 200, Body = "Success 2", Headers = new List<string>() }
            };

            var plugin1 = new AnonymousListenerTestable(mockHttpClient1)
            {
                Url = "https://webhook1.example.com/events",
                Timeout = 5,
                Headers = new List<string>(),
                TypeOptionsValue = ""
            };

            var plugin2 = new AnonymousListenerTestable(mockHttpClient2)
            {
                Url = "https://webhook2.example.com/events",
                Timeout = 5,
                Headers = new List<string>(),
                TypeOptionsValue = ""
            };

            // Act
            var task1 = plugin1.ExecuteAsync("{\"source\":\"plugin1\"}", "");
            var task2 = plugin2.ExecuteAsync("{\"source\":\"plugin2\"}", "");

            await Task.WhenAll(task1, task2);

            // Assert
            Assert.True(task1.Result.Succeeded);
            Assert.True(task2.Result.Succeeded);
            Assert.Equal("https://webhook1.example.com/events", mockHttpClient1.LastUrl);
            Assert.Equal("https://webhook2.example.com/events", mockHttpClient2.LastUrl);
        }
    }
}

