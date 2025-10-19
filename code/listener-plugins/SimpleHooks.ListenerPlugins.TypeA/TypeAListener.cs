using SimpleTools.SimpleHooks.HttpClient.Interface;
using SimpleTools.SimpleHooks.ListenerInterfaces;
using SimpleTools.SimpleHooks.Log.Interface;
using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace SimpleTools.SimpleHooks.ListenerPlugins.TypeA
{
    public class TypeAListener : IListener
    {
        // IListener properties - set by ListenerPluginManager
        public string Url { get; set; }
        public int Timeout { get; set; }
        public List<string> Headers { get; set; }

        private readonly IHttpClient _httpClient;
        private string _cachedToken;
        private DateTime _tokenExpiration;

        public TypeAListener()
        {
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

            var log = Log.Interface.Utility.FillBasicProps(null); //fill Machine, Owner, Location
            log.CodeReference = $"{this.GetType().FullName}|{System.Reflection.MethodBase.GetCurrentMethod()?.Name}";
            log.Correlation = Guid.NewGuid();
            log.ReferenceName = "listenerInstance.Id";
            log.ReferenceValue = listenerInstanceId.ToString();
            log = Log.Interface.Utility.SetArgumentsToNotesA(log, parameters); //fill NotesA
            
            HttpResult httpResult = null;

            try
            {
                // Log start
                log = Log.Interface.Utility.SetMethodStart(log); //fill LogType, LogStep, Counter, CreateDate
                result.Logs.Add(logCounter++, log);

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

                    log = Log.Interface.Utility.SetError(log, ex); // fill LogType, Step, NotesB, Counter, CreateDate
                    result.Logs.Add(logCounter++, log);
                    return result;
                }

                if (authConfig == null || string.IsNullOrWhiteSpace(authConfig.IdentityProviderUrl))
                {
                    result.Succeeded = false;
                    result.Message = "TypeOptions is missing or invalid";

                    log = Log.Interface.Utility.SetError(log,
                        "TypeOptions is missing or invalid"); //fill LogType, Step, NotesB, Counter, CreateDate
                    result.Logs.Add(logCounter++, log);
                    return result;
                }

                // Get bearer token (cached or fresh)
                var tokenResult = await GetBearerToken(listenerInstanceId, authConfig, result, logCounter);
                string token = tokenResult.token;
                logCounter = tokenResult.logCounter;

                if (string.IsNullOrWhiteSpace(token))
                {
                    result.Succeeded = false;
                    result.Message = "Failed to obtain bearer token";

                    log = Log.Interface.Utility.SetError(log,
                        "Failed to obtain bearer token"); //fill LogType, Step, NotesB, Counter, CreateDate
                    result.Logs.Add(logCounter++, log);
                    return result;
                }

                // Add Authorization header
                var headersWithAuth = new List<string>(Headers ?? [])
                {
                    $"Authorization: Bearer {token}"
                };

                // Make HTTP call
                httpResult = _httpClient.Post(Url, headersWithAuth, eventData, Timeout);

                parameters.Add("httpResult", httpResult.ToString());
                log = Log.Interface.Utility.SetArgumentsToNotesA(log, parameters);

                // Check result
                if (httpResult.HttpCode >= 200 && httpResult.HttpCode < 300)
                {
                    var message = $"HTTP call succeeded with status code {httpResult.HttpCode}";
                    result.Succeeded = true;
                    result.Message = message;

                    log = Log.Interface.Utility.SetInformationMessage(log, message);
                    result.Logs.Add(logCounter++, log);
                }
                else if (httpResult.HttpCode == 401)
                {
                    // Token might be expired, try refreshing
                    log = Log.Interface.Utility.SetError(log, "Received 401, attempting token refresh");
                    result.Logs.Add(logCounter++, log);

                    _cachedToken = null; // Invalidate cache
                    tokenResult = await GetBearerToken(listenerInstanceId, authConfig, result, logCounter);
                    token = tokenResult.token;
                    logCounter = tokenResult.logCounter;

                    // Retry with new token
                    headersWithAuth = new List<string>(Headers ?? [])
                    {
                        $"Authorization: Bearer {token}"
                    };

                    httpResult = _httpClient.Post(Url, headersWithAuth, eventData, Timeout);

                    if (httpResult.HttpCode >= 200 && httpResult.HttpCode < 300)
                    {
                        var message = $"HTTP call succeeded after token refresh: {httpResult.HttpCode}";
                        result.Succeeded = true;
                        result.Message = message;

                        log = Log.Interface.Utility.SetInformationMessage(log, message);
                        result.Logs.Add(logCounter++, log);
                    }
                    else
                    {
                        var message = $"HTTP call failed after token refresh: {httpResult.HttpCode}";
                        result.Succeeded = false;
                        result.Message = message;

                        log = Log.Interface.Utility.SetError(log, message);
                        result.Logs.Add(logCounter++, log);
                    }

                    log = Log.Interface.Utility.SetInformationMessage(log, $"Retry result: {httpResult.HttpCode}");
                    result.Logs.Add(logCounter++, log);
                }
                else
                {
                    var message = $"HTTP call failed with status code {httpResult.HttpCode}";
                    result.Succeeded = false;
                    result.Message = message;

                    log = Log.Interface.Utility.SetError(log, message);
                    result.Logs.Add(logCounter++, log);
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
                log = Log.Interface.Utility.SetArgumentsToNotesA(log, parameters);

                log = Log.Interface.Utility.SetError(log, ex);
                result.Logs.Add(logCounter++, log);
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
                log = Log.Interface.Utility.SetArgumentsToNotesA(log, parameters);

                log = Log.Interface.Utility.SetError(log, ex);
                result.Logs.Add(logCounter++, log);
            }

            log = Log.Interface.Utility.SetMethodEnd(log, startTime);

            return result;
        }

        private async Task<(string token, int logCounter)> GetBearerToken(long listenerInstanceId, TypeAOptions config, ListenerResult result, int logCounter)
        {
            var startTime = DateTime.UtcNow;

            //create basic log model
            var log = Log.Interface.Utility.FillBasicProps(null); //fill Machine, Owner, Location
            log.CodeReference = $"{this.GetType().FullName}|{System.Reflection.MethodBase.GetCurrentMethod()?.Name}";
            log.Correlation = Guid.NewGuid();
            log.ReferenceName = "listenerInstance.Id";
            log.ReferenceValue = listenerInstanceId.ToString();

            var parameters = new Dictionary<string, string>();

            log = Log.Interface.Utility.SetMethodStart(log);
            result.Logs.Add(logCounter++, log);

            // Check cache
            if (!string.IsNullOrWhiteSpace(_cachedToken) && DateTime.UtcNow < _tokenExpiration)
            {
                log = Log.Interface.Utility.SetInformationMessage(log, "Using cached bearer token");
                result.Logs.Add(logCounter++, log);
                return (_cachedToken, logCounter);
            }

            HttpResult tokenHttpResult = null;

            try
            {
                // Request new token
                log = Log.Interface.Utility.SetInformationMessage(log,
                    $"Requesting bearer token from {config.IdentityProviderUrl}");
                result.Logs.Add(logCounter++, log);

                var tokenRequestBody =
                    $"grant_type=client_credentials&client_id={config.ClientId}&client_secret={config.ClientSecret}&scope={config.Scope}";
                var tokenHeaders = new List<string> { "Content-Type: application/x-www-form-urlencoded" };

                tokenHttpResult = _httpClient.Post(config.IdentityProviderUrl, tokenHeaders, tokenRequestBody, 5);

                parameters.Add("tokenHttpResult", tokenHttpResult.ToString());
                log = Log.Interface.Utility.SetArgumentsToNotesA(log, parameters); //fill NotesA

                if (tokenHttpResult.HttpCode >= 200 && tokenHttpResult.HttpCode < 300)
                {
                    var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(tokenHttpResult.Body);
                    _cachedToken = tokenResponse.AccessToken;
                    _tokenExpiration =
                        DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn - 60); // Refresh 60 seconds before expiry

                    log = Log.Interface.Utility.SetInformationMessage(log, "Bearer token obtained successfully");
                    result.Logs.Add(logCounter++, log);

                    return (_cachedToken, logCounter);
                }
                else
                {
                    log = Log.Interface.Utility.SetError(log,
                        $"Failed to obtain token: {tokenHttpResult.HttpCode} - {tokenHttpResult.Body}");
                    result.Logs.Add(logCounter++, log);
                    return (null, logCounter);
                }
            }
            catch (HttpRequestException ex)
            {
                tokenHttpResult = tokenHttpResult ?? new HttpResult()
                {
                    Body = "null httpResult filled in  catch HttpRequestException",
                    HttpCode = 0
                };
                parameters.Add("httpResult", tokenHttpResult.ToString());
                log = Log.Interface.Utility.SetArgumentsToNotesA(log, parameters);
                log = Log.Interface.Utility.SetError(log, ex);
                result.Logs.Add(logCounter++, log);
                return (null, logCounter);
            }
            catch (Exception ex)
            {
                tokenHttpResult = tokenHttpResult ?? new HttpResult()
                {
                    Body = "null httpResult filled in  catch Exception",
                    HttpCode = 0
                };
                parameters.Add("httpResult", tokenHttpResult.ToString());
                log = Log.Interface.Utility.SetArgumentsToNotesA(log, parameters);
                log = Log.Interface.Utility.SetError(log, ex);
                result.Logs.Add(logCounter++, log);
                return (null, logCounter);
            }

            log = Log.Interface.Utility.SetMethodEnd(log, startTime);
            result.Logs.Add(logCounter++, log);
        }
    }
}

