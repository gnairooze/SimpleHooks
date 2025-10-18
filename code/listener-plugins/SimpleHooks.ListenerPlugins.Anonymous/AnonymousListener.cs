using SimpleTools.SimpleHooks.ListenerInterfaces;
using SimpleTools.SimpleHooks.HttpClient.Interface;
using SimpleTools.SimpleHooks.Log.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleTools.SimpleHooks.ListenerPlugins.Anonymous
{
    public class AnonymousListener : IListener
    {
        // IListener properties - set by ListenerPluginManager
        public string Url { get; set; }
        public int Timeout { get; set; }
        public List<string> Headers { get; set; }

        private readonly IHttpClient _httpClient;

        public AnonymousListener()
        {
            // Initialize with default HTTP client
            _httpClient = new SimpleTools.SimpleHooks.HttpClient.Simple.SimpleClient();
        }

        public async Task<ListenerResult> ExecuteAsync(string eventData, string typeOptions)
        {
            var result = new ListenerResult();
            int logCounter = 0;
            var log = new LogModel
            {
                LogType = LogModel.LogTypes.Information,
                NotesA = $"Anonymous plugin executing call to {Url}. eventData: {eventData}",
                NotesB = string.Empty,
                Correlation = Guid.NewGuid(),
                CreateDate = DateTime.UtcNow
            };

            HttpResult httpResult = null;

            try
            {
                // Log start
                result.Logs.Add(logCounter++, log);

                // Make HTTP call using properties
                httpResult = _httpClient.Post(Url, Headers, eventData, Timeout);

                // Check result
                if (httpResult.HttpCode >= 200 && httpResult.HttpCode < 300)
                {
                    result.Succeeded = true;
                    result.Message = $"HTTP call succeeded with status code {httpResult.HttpCode}";

                    log.NotesA = $"Success: {httpResult.HttpCode} - {httpResult.Body}";
                    log.CreateDate = DateTime.UtcNow;
                    result.Logs.Add(logCounter++, log);
                }
                else
                {
                    result.Succeeded = false;
                    result.Message = $"HTTP call failed with status code {httpResult.HttpCode}";

                    log.LogType = LogModel.LogTypes.Error;
                    log.NotesB = $"Failed: {httpResult}";
                    log.CreateDate = DateTime.UtcNow;
                    result.Logs.Add(logCounter++, log);
                }
            }
            catch (Exception ex)
            {
                result.Succeeded = false;
                result.Message = $"Exception during execution: {ex.Message}";

                log.LogType = LogModel.LogTypes.Error;
                log.NotesA = (httpResult != null)? httpResult.ToString() : "exception caught with null httpResult";
                log.NotesB = ex.ToString();
                log.CreateDate = DateTime.UtcNow;
                result.Logs.Add(logCounter++, log);
            }

            return await Task.FromResult(result);
        }
    }
}

