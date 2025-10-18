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

            try
            {
                // Log start
                result.Logs.Add(logCounter++, new LogModel
                {
                    LogType = LogModel.LogTypes.Information,
                    NotesA = $"Anonymous plugin executing call to {Url}",
                    CreateDate = DateTime.UtcNow
                });

                // Make HTTP call using properties
                var httpResult = _httpClient.Post(Url, Headers, eventData, Timeout);

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

            return await Task.FromResult(result);
        }
    }
}

