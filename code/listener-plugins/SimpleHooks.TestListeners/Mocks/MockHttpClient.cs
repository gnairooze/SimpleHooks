using SimpleTools.SimpleHooks.HttpClient.Interface;
using System;
using System.Collections.Generic;

namespace SimpleTools.SimpleHooks.TestListeners.Mocks
{
    public class MockHttpClient : IHttpClient
    {
        public HttpResult? MockResult { get; set; }
        public bool ShouldThrow { get; set; } = false;
        public Exception? ExceptionToThrow { get; set; }
        public string? LastUrl { get; private set; }
        public List<string>? LastHeaders { get; private set; }
        public string? LastBody { get; private set; }
        public int LastTimeout { get; private set; }

        public HttpResult Post(string url, List<string> headers, string body, int timeout)
        {
            LastUrl = url;
            LastHeaders = headers;
            LastBody = body;
            LastTimeout = timeout;

            if (ShouldThrow)
                throw ExceptionToThrow ?? new HttpRequestException("Mock exception");

            return MockResult ?? new HttpResult
            {
                HttpCode = 200,
                Body = "Mock response",
                Headers = new List<string>()
            };
        }
    }
}

