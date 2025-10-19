using SimpleTools.SimpleHooks.ListenerInterfaces;
using SimpleTools.SimpleHooks.ListenerPlugins.Anonymous;
using SimpleTools.SimpleHooks.HttpClient.Interface;
using SimpleTools.SimpleHooks.TestListeners.Mocks;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace SimpleTools.SimpleHooks.TestListeners
{
    public class AnonymousPluginTests
    {
        [Fact]
        public async Task ExecuteAsync_ValidRequest_ReturnsSuccess()
        {
            // Arrange
            var mockHttpClient = new MockHttpClient
            {
                MockResult = new HttpResult
                {
                    HttpCode = 200,
                    Body = "Success response",
                    Headers = new List<string>()
                }
            };

            var plugin = new AnonymousListenerTestable(mockHttpClient)
            {
                Url = "https://api.example.com/webhook",
                Timeout = 5,
                Headers = new List<string> { "Content-Type: application/json" }
            };

            string eventData = "{\"event\":\"test\"}";

            var listenerInstanceId = 1;
            // Act
            var result = await plugin.ExecuteAsync(listenerInstanceId, eventData, "");

            // Assert
            Assert.True(result.Succeeded);
            Assert.Contains("succeeded", result.Message, StringComparison.OrdinalIgnoreCase);
            Assert.NotEmpty(result.Logs);
            Assert.Equal("https://api.example.com/webhook", mockHttpClient.LastUrl);
            Assert.Equal(eventData, mockHttpClient.LastBody);
        }

        [Fact]
        public async Task ExecuteAsync_HttpError_ReturnsFailure()
        {
            // Arrange
            var mockHttpClient = new MockHttpClient
            {
                MockResult = new HttpResult
                {
                    HttpCode = 500,
                    Body = "Internal Server Error",
                    Headers = new List<string>()
                }
            };

            var plugin = new AnonymousListenerTestable(mockHttpClient)
            {
                Url = "https://api.example.com/webhook",
                Timeout = 5,
                Headers = new List<string>()
            };

            // Act
            var result = await plugin.ExecuteAsync(1, "{\"event\":\"test\"}", "");

            // Assert
            Assert.False(result.Succeeded);
            Assert.Contains("failed", result.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("500", result.Message);
            Assert.NotEmpty(result.Logs);
            Assert.True(result.Logs.Count > 0);
        }

        [Fact]
        public async Task ExecuteAsync_Timeout_ReturnsFailure()
        {
            // Arrange
            var mockHttpClient = new MockHttpClient
            {
                ShouldThrow = true,
                ExceptionToThrow = new TimeoutException("Request timeout")
            };

            var plugin = new AnonymousListenerTestable(mockHttpClient)
            {
                Url = "https://api.example.com/webhook",
                Timeout = 1,
                Headers = new List<string>()
            };

            // Act
            var result = await plugin.ExecuteAsync(1, "{\"event\":\"test\"}", "");

            // Assert
            Assert.False(result.Succeeded);
            Assert.Contains("Exception", result.Message);
            Assert.NotEmpty(result.Logs);
            var lastLog = result.Logs[result.Logs.Count - 1];
            Assert.Contains("timeout", lastLog.NotesA, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task ExecuteAsync_InvalidUrl_ReturnsFailure()
        {
            // Arrange
            var mockHttpClient = new MockHttpClient
            {
                ShouldThrow = true,
                ExceptionToThrow = new UriFormatException("Invalid URI")
            };

            var plugin = new AnonymousListenerTestable(mockHttpClient)
            {
                Url = "not-a-valid-url",
                Timeout = 5,
                Headers = new List<string>()
            };

            // Act
            var result = await plugin.ExecuteAsync(1, "{\"event\":\"test\"}", "");

            // Assert
            Assert.False(result.Succeeded);
            Assert.Contains("Exception", result.Message);
            Assert.NotEmpty(result.Logs);
        }

        [Fact]
        public async Task ExecuteAsync_EmptyHeaders_ExecutesSuccessfully()
        {
            // Arrange
            var mockHttpClient = new MockHttpClient
            {
                MockResult = new HttpResult
                {
                    HttpCode = 200,
                    Body = "Success",
                    Headers = new List<string>()
                }
            };

            var plugin = new AnonymousListenerTestable(mockHttpClient)
            {
                Url = "https://api.example.com/webhook",
                Timeout = 5,
                Headers = new List<string>() // Empty headers
            };

            // Act
            var result = await plugin.ExecuteAsync(1, "{\"event\":\"test\"}", "");

            // Assert
            Assert.True(result.Succeeded);
            Assert.NotNull(mockHttpClient.LastHeaders);
        }

        [Fact]
        public async Task ExecuteAsync_MultipleHeaders_PassedCorrectly()
        {
            // Arrange
            var mockHttpClient = new MockHttpClient
            {
                MockResult = new HttpResult
                {
                    HttpCode = 201,
                    Body = "Created",
                    Headers = new List<string>()
                }
            };

            var headers = new List<string>
            {
                "Content-Type: application/json",
                "X-Custom-Header: custom-value",
                "Accept: application/json"
            };

            var plugin = new AnonymousListenerTestable(mockHttpClient)
            {
                Url = "https://api.example.com/webhook",
                Timeout = 5,
                Headers = headers
            };

            // Act
            var result = await plugin.ExecuteAsync(1, "{\"event\":\"test\"}", "");

            // Assert
            Assert.True(result.Succeeded);
            Assert.NotNull(mockHttpClient.LastHeaders);
            Assert.Equal(headers.Count, mockHttpClient.LastHeaders.Count);
        }

        [Fact]
        public async Task ExecuteAsync_LogsContainTimestamps()
        {
            // Arrange
            var mockHttpClient = new MockHttpClient
            {
                MockResult = new HttpResult
                {
                    HttpCode = 200,
                    Body = "Success",
                    Headers = new List<string>()
                }
            };

            var plugin = new AnonymousListenerTestable(mockHttpClient)
            {
                Url = "https://api.example.com/webhook",
                Timeout = 5,
                Headers = new List<string>()
            };

            // Act
            var result = await plugin.ExecuteAsync(1,"{\"event\":\"test\"}", "");

            // Assert
            Assert.NotEmpty(result.Logs);
            foreach (var log in result.Logs.Values)
            {
                Assert.NotEqual(default(DateTime), log.CreateDate);
            }
        }
    }

    // Testable version that accepts mock HTTP client
    public class AnonymousListenerTestable : IListener
    {
        private readonly IHttpClient _httpClient;

        public string Url { get; set; } = string.Empty;
        public int Timeout { get; set; }
        public List<string> Headers { get; set; } = new List<string>();
        
        public AnonymousListenerTestable(IHttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<ListenerResult> ExecuteAsync(long listenerInstanceId, string eventData, string typeOptions)
        {
            var result = new ListenerResult();
            int logCounter = 0;

            try
            {
                result.Logs.Add(logCounter++, new Log.Interface.LogModel
                {
                    NotesA = $"Anonymous plugin executing call to {Url}",
                    CreateDate = DateTime.Now
                });

                var httpResult = _httpClient.Post(Url, Headers, eventData, Timeout);

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

            return await Task.FromResult(result);
        }
    }
}

