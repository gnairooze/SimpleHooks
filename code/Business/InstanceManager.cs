using SimpleTools.SimpleHooks.HttpClient.Interface;
using SimpleTools.SimpleHooks.Interfaces;
using SimpleTools.SimpleHooks.Log.Interface;
using SimpleTools.SimpleHooks.ListenerInterfaces;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using SimpleTools.SimpleHooks.Models.Instance;

namespace SimpleTools.SimpleHooks.Business
{
    public class InstanceManager:LogBase
    {
        private readonly ILog _logger;

        private readonly IConnectionRepository _connectionRepo;
        private readonly IDataRepository<Models.Instance.EventInstance> _eventInstanceRepo;
        private readonly IDataRepository<Models.Instance.ListenerInstance> _listenerInstanceRepo;
        private readonly IHttpClient _httpClient;
        
        private readonly DefinitionManager _definitionManager;
        private readonly Guid _logCorrelation = Guid.NewGuid();

        private static int _groupId = 1; // default value
        private int _maxGroups;

        public DefinitionManager DefinitionMgr => _definitionManager;

        #region constructors
        public InstanceManager(
            ILog logger,
            IConnectionRepository connectionRepo,
            IDataRepository<Models.Instance.EventInstance> eventInstanceRepo,
            IDataRepository<Models.Instance.ListenerInstance> listenerInstanceRepo,
            IHttpClient httpClient,
            IDataRepository<Models.Definition.EventDefinition> eventDefRepo,
            IDataRepository<Models.Definition.ListenerDefinition> listenerDefRepo,
            IDataRepository<Models.Definition.EventDefinitionListenerDefinition> eventDefListenerDefRepo,
            IDataRepository<Models.Definition.ListenerType> listenerTypeRepo,
            ListenerPluginManager listenerPluginManager,
            IDataRepository<Models.Definition.AppOption> appOptionRepo
            )
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));

            this._connectionRepo = connectionRepo ?? throw new ArgumentNullException(nameof(connectionRepo));

            this._eventInstanceRepo = eventInstanceRepo ?? throw new ArgumentNullException(nameof(eventInstanceRepo));
            this._listenerInstanceRepo = listenerInstanceRepo ?? throw new ArgumentNullException(nameof(listenerInstanceRepo));

            this._httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

            this._definitionManager = new DefinitionManager(logger, eventDefRepo, listenerDefRepo, eventDefListenerDefRepo, appOptionRepo, listenerTypeRepo, connectionRepo, listenerPluginManager);

            _definitionManager.DefitionsLoaded += OnDefinitionsLoaded;

            var succeeded = this._definitionManager.LoadDefinitions();
            if (!succeeded)
            {
                throw new InvalidOperationException("definitions failed to load");
            }
        }

        private void OnDefinitionsLoaded(object sender, EventArgs e)
        {
            SetMaxGroups();
        }
        #endregion

        private void SetMaxGroups()
        {
            var maxGroupsOption = _definitionManager.AppOptions.SingleOrDefault(o =>
                o.Name == Constants.AppOptionNameMaxGroups
                && o.Category == Constants.AppOptionCategoryGetNotProcessed);

            if (maxGroupsOption != null && !string.IsNullOrEmpty(maxGroupsOption.Value))
            {
                _maxGroups = int.Parse(maxGroupsOption.Value);
            }
            else
            {

                _maxGroups = 1; // default value
            }
        }

        /// <summary>
        /// Add event instance
        /// </summary>
        /// <param name="eventInstance">fill the event instance properties only. listener instances will be added automatically</param>
        /// <returns></returns>
        public Models.Instance.EventInstance Add(Models.Instance.EventInstance eventInstance)
        {
            //initialize log and add first log
            var log = this.GetLogModelMethodStart(MethodBase.GetCurrentMethod()?.Name, eventInstance.ReferenceName, eventInstance.ReferenceValue);
            log.Correlation = _logCorrelation;

            this._logger.Add(log);

            eventInstance.GroupId = SetGroupId();

            Models.Instance.EventInstance resultInstance = null;

            object trans = null;
            object conn = this._connectionRepo.CreateConnection();

            try
            {
                FetchListenersForEventInstance(eventInstance);

                this._connectionRepo.OpenConnection(conn);
                trans = this._connectionRepo.BeginTransaction(conn);

                eventInstance = this._eventInstanceRepo.Create(eventInstance, conn, trans);
                List<Models.Instance.ListenerInstance> listenerInstances = new List<Models.Instance.ListenerInstance>();
                foreach (var item in eventInstance.ListenerInstances)
                {
                    item.EventInstanceId = eventInstance.Id;
                    var listenerInstance = this._listenerInstanceRepo.Create(item, conn, trans);
                    listenerInstances.Add(listenerInstance);
                }

                eventInstance.ListenerInstances.Clear();
                eventInstance.ListenerInstances.AddRange(listenerInstances);

                this._connectionRepo.CommitTransaction(trans);

                resultInstance = eventInstance;
            }
            catch (Exception ex)
            {
                this._connectionRepo.RollbackTransaction(trans);

                log = GetLogModelException(log, ex);
                log.Correlation = _logCorrelation;
                this._logger.Add(log);
            }
            finally
            {
                this._connectionRepo.CloseConnection(conn);
            }

            this._connectionRepo.DisposeConnection(conn);

            //add end log
            log.LogType = LogModel.LogTypes.Debug;
            log.Correlation = this._logCorrelation;
            this._logger.Add(this.GetLogModelMethodEnd(log, "eventInstance.Id", eventInstance.Id.ToString()));

            return resultInstance;
        }

        private int SetGroupId()
        {
            if (_groupId >= _maxGroups)
            {
                _groupId = 1;
            }
            else
            {
                Interlocked.Increment(ref _groupId);
            }

            return _groupId;
        }

        private void FetchListenersForEventInstance(Models.Instance.EventInstance eventInstance)
        {
            eventInstance.ListenerInstances.Clear();
            var relations = this._definitionManager.EventDefinitionListenerDefinitionRelations.Where(r => r.EventDefinitionId == eventInstance.EventDefinitionId
                && r.Active
                );
            foreach (var relation in relations)
            {
                var listenerInstance = new Models.Instance.ListenerInstance()
                {
                    Active = true,
                    CreateBy = InstanceConstants.UserSystemEventManager,
                    CreateDate = DateTime.UtcNow,
                    ListenerDefinitionId = relation.ListenerDefinitionId,
                    ModifyBy = InstanceConstants.UserSystemEventManager,
                    ModifyDate = DateTime.UtcNow,
                    NextRun = DateTime.UtcNow,
                    RemainingTrialCount = this._definitionManager.ListenerDefinitions.Single(l => l.Id == relation.ListenerDefinitionId).TrialCount,
                    Status = Models.Instance.Enums.ListenerInstanceStatus.InQueue
                };

                eventInstance.ListenerInstances.Add(listenerInstance);
            }
        }

        public List<Models.Instance.EventInstance> GetEventInstancesToProcess(DateTime runDate, int groupId = 1)
        {
            //initialize log and add first log
            var log = this.GetLogModelMethodStart(MethodBase.GetCurrentMethod()?.Name, string.Empty, string.Empty);
            log.Correlation = this._logCorrelation;
            log.NotesA = $"runDate {runDate:yyyy-MM-dd HH:mm:ss}";
            this._logger.Add(log);

            List<Models.Instance.EventInstance> results = null;

            var readOperation = new Dictionary<string, string>
            {
                {
                    Models.Instance.Enums.EventInstanceReadOperations.ReadNotProcessed.ToString(),
                    runDate.ToString(CultureInfo.InvariantCulture)
                },
                {
                    Constants.OperationGroupId, groupId.ToString()
                }
            };

            object conn = null;

            try
            {
                conn = this._connectionRepo.CreateConnection();
                this._connectionRepo.OpenConnection(conn);

                results = this._eventInstanceRepo.Read(readOperation, conn);
            }
            catch (Exception ex)
            {
                log = GetLogModelException(log, ex);
                log.Correlation = this._logCorrelation;
                this._logger.Add(log);
            }
            finally
            {
                this._connectionRepo.CloseConnection(conn);
                this._connectionRepo.DisposeConnection(conn);
            }

            if (results != null)
            {

                foreach (var eventInstance in results)
                {
                    EnrichListenerDefinition(eventInstance);
                    EnrichEventDefinition(eventInstance);
                }
            }

            int resultsCount = results?.Count ?? 0;
            log.Step = "events count";
            log.NotesA += $"{Environment.NewLine} | {resultsCount} events to be processed";
            log.LogType = LogModel.LogTypes.Information;
            log.Correlation = this._logCorrelation;
            this._logger.Add(log);

            this._logger.Add(this.GetLogModelMethodEnd(log));

            return results;
        }

        private void EnrichListenerDefinition(Models.Instance.EventInstance eventInstance)
        {
            foreach (var listenerInstance in eventInstance.ListenerInstances)
            {
                listenerInstance.Definition = this._definitionManager.ListenerDefinitions.Single(d => d.Id == listenerInstance.ListenerDefinitionId);
            }
        }

        private void EnrichEventDefinition(Models.Instance.EventInstance eventInstance)
        {
            eventInstance.Definition = this._definitionManager.EventDefinitions.Single(d => d.Id == eventInstance.EventDefinitionId);
        }

        public void Process(List<Models.Instance.EventInstance> eventInstances)
        {
            //initialize log and add first log
            var log = this.GetLogModelMethodStart(MethodBase.GetCurrentMethod()?.Name,string.Empty, string.Empty);
            log.Correlation = this._logCorrelation;
            this._logger.Add(log);

            foreach (var eventInstance in eventInstances)
            {
                try
                {
                    Process(eventInstance);
                }
                catch (Exception ex)
                {
                    log = GetLogModelException(log, ex);
                    log.Correlation = this._logCorrelation;
                    this._logger.Add(log);
                }

            }

            //add end log
            this._logger.Add(this.GetLogModelMethodEnd(log));
        }

        private void Process(Models.Instance.EventInstance eventInstance)
        {
            //initialize log and add first log
            var log = this.GetLogModelMethodStart(MethodBase.GetCurrentMethod()?.Name, "eventInstance.Id", eventInstance.Id.ToString());
            log.ReferenceName = "eventInstance.Id";
            log.ReferenceValue = eventInstance.Id.ToString();
            log.Correlation = this._logCorrelation;
            this._logger.Add(log);

            //todo: updating the event instance and listener instance should be in the same transaction
            eventInstance.ModifyBy = InstanceConstants.UserSystemProcessor;
            eventInstance.ModifyDate = DateTime.UtcNow;
            eventInstance.Status = Models.Instance.Enums.EventInstanceStatus.Processing;

            object conn = this._connectionRepo.CreateConnection();
            try
            {
                this._connectionRepo.OpenConnection(conn);

                this._eventInstanceRepo.Edit(eventInstance, conn, null);
                log.Counter++;
                log.Step = "event instance status updated";
                log.NotesA = eventInstance.ToString();
                log.LogType = LogModel.LogTypes.Debug;
                this._logger.Add(log);

            }
            catch(Exception ex)
            {
                log = GetLogModelException(log, ex);
                this._logger.Add(log);
            }
            finally
            {
                this._connectionRepo.CloseConnection(conn);
            }

            string enhancedEventData = InjectSimpleHookMetaDataInEventDate(eventInstance);

            foreach (var item in eventInstance.ListenerInstances)
            {
                ExecuteListener(item, enhancedEventData);
            }

            RefreshListeners(eventInstance);
            SetEventInstanceStatus(eventInstance);

            try
            {
                this._connectionRepo.OpenConnection(conn);
                this._eventInstanceRepo.Edit(eventInstance, conn, null);
                log.Counter++;
                log.Step = "event instance status updated";
                log.NotesA = eventInstance.ToString();
                log.LogType = LogModel.LogTypes.Debug;
                this._logger.Add(log);
            }
            catch (Exception ex)
            {
                log = GetLogModelException(log, ex);
                this._logger.Add(log);
            }
            finally
            {
                this._connectionRepo.CloseConnection(conn);
            }

            this._connectionRepo.DisposeConnection(conn);

            this._logger.Add(this.GetLogModelMethodEnd(log));
        }

        private string InjectSimpleHookMetaDataInEventDate(Models.Instance.EventInstance eventInstance)
        {
            var log = this.GetLogModelMethodStart(MethodBase.GetCurrentMethod()?.Name, string.Empty, string.Empty);
            log.Correlation = this._logCorrelation;
            this._logger.Add(log);

            try
            {
                var jsonData = JObject.Parse(eventInstance.EventData);
                jsonData.Add(new JProperty("simpleHooksMetadata", new JObject(
                    new JProperty("eventDefinitionId", eventInstance.EventDefinitionId),
                    new JProperty("eventDefinitionName", eventInstance.Definition.Name),
                    new JProperty("eventBusinessId", eventInstance.BusinessId),
                    new JProperty("eventCreateDate", eventInstance.CreateDate),
                    new JProperty("eventReferenceName", eventInstance.ReferenceName),
                    new JProperty("eventReferenceValue", eventInstance.ReferenceValue)
                    )));

                this._logger.Add(this.GetLogModelMethodEnd(log));

                return jsonData.ToString();
            }
            catch (Exception ex)
            {
                log = this.GetLogModelException(log, ex);
                this._logger.Add(log);

                return eventInstance.EventData;
            }
        }

        private void RefreshListeners(Models.Instance.EventInstance eventInstance)
        {
            var log = this.GetLogModelMethodStart(MethodBase.GetCurrentMethod()?.Name, "eventInstance.Id", eventInstance.Id.ToString());
            log.Correlation = this._logCorrelation;
            this._logger.Add(log);

            var readOperation = new Dictionary<string, string>
            {
                { Models.Instance.Enums.ListenerInstanceReadOperations.ReadByEventInstanceId.ToString(), eventInstance.Id.ToString() }
            };

            object conn = this._connectionRepo.CreateConnection();
            List<Models.Instance.ListenerInstance> listeners = null;
            try
            {
                this._connectionRepo.OpenConnection(conn);
                listeners = this._listenerInstanceRepo.Read(readOperation, conn);
            }
            catch (Exception ex)
            {
                log = GetLogModelException(log, ex);
                this._logger.Add(log);
            }
            finally
            {
                this._connectionRepo.CloseConnection(conn);
            }

            this._connectionRepo.DisposeConnection(conn);

            eventInstance.ListenerInstances.Clear();
            if (listeners != null) eventInstance.ListenerInstances.AddRange(listeners);

            this._logger.Add(this.GetLogModelMethodEnd(log));
        }

        private void SetEventInstanceStatus(Models.Instance.EventInstance eventInstance)
        {
            #region set default values
            eventInstance.ModifyBy = InstanceConstants.UserSystemProcessor;
            eventInstance.ModifyDate = DateTime.UtcNow;
            #endregion

            #region if no listeners exist
            bool eventHasListeners = eventInstance.ListenerInstances.Count > 0;

            if (!eventHasListeners)
            {
                eventInstance.Status = Models.Instance.Enums.EventInstanceStatus.Succeeded;
                return;
            }
            #endregion

            #region remaining status
            bool listenerActive = eventInstance.ListenerInstances.Exists(l =>
                l.Status == Models.Instance.Enums.ListenerInstanceStatus.InQueue ||
                l.Status == Models.Instance.Enums.ListenerInstanceStatus.Processing ||
                l.Status == Models.Instance.Enums.ListenerInstanceStatus.WaitingForRetrial);

            if (listenerActive)
            {
                eventInstance.Status = Models.Instance.Enums.EventInstanceStatus.Processing;
                return;
            }

            bool listenerFailed = eventInstance.ListenerInstances.Exists(l => l.Status == Models.Instance.Enums.ListenerInstanceStatus.Failed);
            if (listenerFailed)
            {
                eventInstance.Status = Models.Instance.Enums.EventInstanceStatus.Failed;
                return;
            }

            bool listenerAborted = eventInstance.ListenerInstances.Exists(l => l.Status == Models.Instance.Enums.ListenerInstanceStatus.Aborted);
            if (listenerAborted)
            {
                eventInstance.Status = Models.Instance.Enums.EventInstanceStatus.Aborted;
                return;
            }

            bool listenerHold = eventInstance.ListenerInstances.Exists(l => l.Status == Models.Instance.Enums.ListenerInstanceStatus.Hold);
            if (listenerHold)
            {
                eventInstance.Status = Models.Instance.Enums.EventInstanceStatus.Hold;
                return;
            }

            eventInstance.Status = Models.Instance.Enums.EventInstanceStatus.Succeeded;
            #endregion
        }

        private void ExecuteListener(Models.Instance.ListenerInstance listenerInstance, string eventData)
        {
            //initialize log and add first log
            var log = this.GetLogModelMethodStart(MethodBase.GetCurrentMethod()?.Name, "listenerInstance.Id", listenerInstance.Id.ToString());
            log.Correlation = this._logCorrelation;
            this._logger.Add(log);

            listenerInstance.ModifyBy = InstanceConstants.UserSystemProcessor;
            listenerInstance.RemainingTrialCount--;
            listenerInstance.Status = Models.Instance.Enums.ListenerInstanceStatus.Processing;

            object conn = this._connectionRepo.CreateConnection();

            var succeeded = UpdateListenerInstanceStatus(listenerInstance, log, conn);

            if (!succeeded)
            {
                this._connectionRepo.DisposeConnection(conn);
                return;
            }

            #region handle plugin execution result
            var parameters = new Dictionary<string, string>
            {
                { "ListenerInstance", listenerInstance.ToString() },
                { "eventData", eventData }
            };

            ListenerInterfaces.ListenerResult pluginResult = null;

            try
            {
                // Get plugin instance from listener definition
                var plugin = listenerInstance.Definition.ListenerPlugin;

                if (plugin == null)
                {
                    throw new InvalidOperationException($"No plugin instance found for ListenerDefinition {listenerInstance.Definition.Id}");
                }

                // Get type options value from environment variable
                string typeOptionsValue = string.Empty;
                if (!string.IsNullOrWhiteSpace(listenerInstance.Definition.TypeOptions))
                {
                    typeOptionsValue = Environment.GetEnvironmentVariable(listenerInstance.Definition.TypeOptions) ?? string.Empty;
                }

                // Execute plugin
                pluginResult = plugin.ExecuteAsync(listenerInstance.Id, eventData, typeOptionsValue).GetAwaiter().GetResult();

                parameters.Add("result", pluginResult.Message);

                // Log plugin execution logs
                foreach (var pluginLog in pluginResult.Logs.Values)
                {
                    pluginLog.Correlation = log.Correlation;
                    pluginLog.ReferenceName = "listenerInstance.Id";
                    pluginLog.ReferenceValue = listenerInstance.Id.ToString();
                    this._logger.Add(pluginLog);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

                if (pluginResult == null)
                {
                    pluginResult = new ListenerInterfaces.ListenerResult()
                    {
                        Succeeded = false,
                        Message = "null result. created in catch Exception block"
                    };

                    parameters.Add("result", pluginResult.Message);
                }

                log.LogType = LogModel.LogTypes.Error;
                log.Counter++;
                log.Step = "execute listener - plugin.ExecuteAsync";
                log.NotesA = Newtonsoft.Json.JsonConvert.SerializeObject(parameters);
                log.NotesB = Newtonsoft.Json.JsonConvert.SerializeObject(e);

                this._logger.Add(log);
            }

            //succeeded
            if (pluginResult != null && pluginResult.Succeeded)
            {
                listenerInstance.Status = Models.Instance.Enums.ListenerInstanceStatus.Succeeded;

                UpdateListenerInstanceStatus(listenerInstance, log, conn);

                log.Counter++;
                log.Step = "At the end of execute listener";
                log.LogType = LogModel.LogTypes.Information;
                log.NotesA = Newtonsoft.Json.JsonConvert.SerializeObject(parameters);
                log.NotesB = string.Empty;
            }
            //failed with no remaining retrials
            else if(listenerInstance.RemainingTrialCount <= 0)
            {
                listenerInstance.Status = Models.Instance.Enums.ListenerInstanceStatus.Failed;
                UpdateListenerInstanceStatus(listenerInstance, log, conn);
                log.LogType = LogModel.LogTypes.Error;
                log.Counter++;
                log.Step = "At the end of execute listener";
                log.NotesB = Newtonsoft.Json.JsonConvert.SerializeObject(parameters);
            }
            //failed with remaining retrials
            else
            {
                listenerInstance.Status = Models.Instance.Enums.ListenerInstanceStatus.WaitingForRetrial;
                listenerInstance.NextRun = listenerInstance.ModifyDate.Add(TimeSpan.FromMinutes(listenerInstance.Definition.RetrialDelay));
                UpdateListenerInstanceStatus(listenerInstance, log, conn);

                log.LogType = LogModel.LogTypes.Error;
                log.Counter++;
                log.Step = "At the end of execute listener";
                log.NotesB = Newtonsoft.Json.JsonConvert.SerializeObject(parameters);
            }
            #endregion

            this._logger.Add(log);

            this._connectionRepo.DisposeConnection(conn);
        }

        private bool UpdateListenerInstanceStatus(Models.Instance.ListenerInstance listenerInstance, LogModel log, object conn)
        {
            bool succeeded = true;
            listenerInstance.ModifyDate = DateTime.UtcNow;

            try
            {
                this._connectionRepo.OpenConnection(conn);
                this._listenerInstanceRepo.Edit(listenerInstance, conn, null);
                log.Counter++;
                log.Step = "listener instance status updated";
                log.NotesA = listenerInstance.ToString();
                log.LogType = LogModel.LogTypes.Debug;
                this._logger.Add(log);
            }
            catch (Exception ex)
            {
                log = GetLogModelException(log, ex);
                log = this._logger.Add(log);

                Console.WriteLine($"Error: {ex.Message} logged with Id {log.Id}");

                succeeded = false;
            }
            finally
            {
                this._connectionRepo.CloseConnection(conn);
            }

            return succeeded;
        }

        public EventInstanceStatusBrief ReadEventInstanceStatusByBusinessId (Guid businessId)
        {
            object conn = null;
            var instanceBrief = new EventInstanceStatusBrief();

            //initialize log and add first log
            var log = this.GetLogModelMethodStart(MethodBase.GetCurrentMethod()?.Name, string.Empty, string.Empty);
            log.Correlation = this._logCorrelation;
            log.NotesA = $"businessId {businessId}";
            this._logger.Add(log);

            try
            {
                conn = this._connectionRepo.CreateConnection();
                this._connectionRepo.OpenConnection(conn);

                instanceBrief = ((Interfaces.IDataRepositoryEventInstanceStatus)_eventInstanceRepo).ReadByBusinessId(businessId, conn);
            }
            catch (Exception ex)
            {
                log = GetLogModelException(log, ex);
                log.Correlation = this._logCorrelation;
                this._logger.Add(log);
            }
            finally
            {
                this._connectionRepo.CloseConnection(conn);
                this._connectionRepo.DisposeConnection(conn);
            }

            //add end log
            this._logger.Add(this.GetLogModelMethodEnd(log));

            return instanceBrief;
        }
    }
}
