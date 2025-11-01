using SimpleTools.SimpleHooks.HttpClient.Interface;
using SimpleTools.SimpleHooks.ListenerInterfaces;
using SimpleTools.SimpleHooks.Log.Interface;
using System;
using System.Collections.Generic;
using System.Net.Http;
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

        public async Task<ListenerResult> ExecuteAsync(long listenerInstanceId, string eventData, string typeOptions)
        {
            var startTime = DateTime.UtcNow;

            var result = new ListenerResult();
            int logCounter = 0;

            //create basic log model
            var parameters = new Dictionary<string, string>
            {
                { "listenerInstanceId", listenerInstanceId.ToString() },
                { "eventData", eventData },
                { "typeOptions", typeOptions }
            };

            var methodName = Log.Interface.Utility.GetRealMethodFromAsync(System.Reflection.MethodBase.GetCurrentMethod())?.Name;

            var log = Log.Interface.Utility.FillBasicProps(null); //fill Machine, Owner, Location
            log.Operation = methodName;
            log.CodeReference = $"{this.GetType().FullName}|{methodName}";
            log.Correlation = Guid.NewGuid();
            log.ReferenceName = "listenerInstance.Id";
            log.ReferenceValue = listenerInstanceId.ToString();
            log.NotesA = System.Text.Json.JsonSerializer.Serialize(parameters);

            HttpResult httpResult = null;

            try
            {
                // Log start
                log = Log.Interface.Utility.SetMethodStart(log); //fill LogType, LogStep, Counter, CreateDate
                result.Logs.Add(logCounter++, Log.Interface.Utility.Clone(log));

                // Make HTTP call using properties
                httpResult = _httpClient.Post(Url, Headers, eventData, Timeout);

                parameters.Add("httpResult", httpResult.ToString());
                log.NotesA = System.Text.Json.JsonSerializer.Serialize(parameters);

                // Check result
                if (httpResult.HttpCode >= 200 && httpResult.HttpCode < 300)
                {
                    var message = $"HTTP call succeeded with status code {httpResult.HttpCode}";
                    result.Succeeded = true;
                    result.Message = message;

                    log = Log.Interface.Utility.SetInformationMessage(log, message);
                    result.Logs.Add(logCounter++, Log.Interface.Utility.Clone(log));
                }
                else
                {
                    var message = $"HTTP call failed with status code {httpResult.HttpCode}";
                    result.Succeeded = false;
                    result.Message = message;

                    log = Log.Interface.Utility.SetError(log, message);
                    result.Logs.Add(logCounter++, Log.Interface.Utility.Clone(log));
                }
            }
            catch (HttpRequestException ex)
            {
                result.Succeeded = false;
                result.Message = $"Exception during execution: {ex.Message}";

                httpResult = httpResult ?? new HttpResult()
                {
                    Body = "null httpResult filled in  catch HttpRequestException",
                    HttpCode = 0
                };

                parameters.Add("httpResult", httpResult.ToString());
                log.NotesA = System.Text.Json.JsonSerializer.Serialize(parameters);

                log = Log.Interface.Utility.SetError(log, ex);
                result.Logs.Add(logCounter++, Log.Interface.Utility.Clone(log));
            }
            catch (Exception ex)
            {
                result.Succeeded = false;
                result.Message = $"Exception during execution: {ex.Message}";

                httpResult = httpResult ?? new HttpResult()
                {
                    Body = "null httpResult filled in  catch Exception",
                    HttpCode = 0
                };

                parameters.Add("httpResult", httpResult.ToString());
                log.NotesA = System.Text.Json.JsonSerializer.Serialize(parameters);

                log = Log.Interface.Utility.SetError(log, ex);
                result.Logs.Add(logCounter++, Log.Interface.Utility.Clone(log));
            }

            log = Log.Interface.Utility.SetMethodEnd(log, startTime);
            result.Logs.Add(logCounter++, Log.Interface.Utility.Clone(log));

            return await Task.FromResult(result);
        }
    }
}

