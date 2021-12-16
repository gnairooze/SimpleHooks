using HttpClient.Interface;
using Interfaces;
using Log.Interface;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Business
{
    public class InstanceManager:LogBase
    {
        private readonly ILog _Logger;

        private readonly IConnectionRepository _ConnectionRepo;
        private readonly IDataRepository<Models.Instance.EventInstance> _EventInstanceRepo;
        private readonly IDataRepository<Models.Instance.ListenerInstance> _ListenerInstanceRepo;
        private readonly IHttpClient _HttpClient;
        private readonly DefinitionManager _DefinitionManager;

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
            IDataRepository<Models.Definition.AppOption> appOptionRepo
            )
        {
            this._Logger = logger ?? throw new ArgumentNullException(nameof(logger));

            this._ConnectionRepo = connectionRepo ?? throw new ArgumentNullException(nameof(connectionRepo));

            this._EventInstanceRepo = eventInstanceRepo ?? throw new ArgumentNullException(nameof(eventInstanceRepo));
            this._ListenerInstanceRepo = listenerInstanceRepo ?? throw new ArgumentNullException(nameof(listenerInstanceRepo));
            
            this._HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

            this._DefinitionManager = new DefinitionManager(logger, eventDefRepo, listenerDefRepo, eventDefListenerDefRepo, appOptionRepo, connectionRepo);
            
            var succeeded = this._DefinitionManager.LoadDefinitions();
            if(!succeeded)
            {
                throw new InvalidOperationException("definitions failed to load");
            }
        }
        #endregion

        /// <summary>
        /// Add event instance
        /// </summary>
        /// <param name="eventInstance">fill the event instance properties only. listener instances will be added automatically</param>
        /// <returns></returns>
        public Models.Instance.EventInstance Add(Models.Instance.EventInstance eventInstance)
        {
            //intialize log and add first log
            var log = this.GetLogModelMethodStart(MethodBase.GetCurrentMethod().Name, "eventInstance.Id", eventInstance.Id.ToString());
            this._Logger.Add(log);

            Models.Instance.EventInstance resultInstance = null;
            
            object trans = null;
            object conn = this._ConnectionRepo.CreateConnection();

            try
            {
                FetchListenersForEventInstance(eventInstance);

                this._ConnectionRepo.OpenConnection(conn);
                trans = this._ConnectionRepo.BeginTransaction(conn);

                eventInstance = this._EventInstanceRepo.Create(eventInstance, conn, trans);
                List<Models.Instance.ListenerInstance> listenerInstances = new List<Models.Instance.ListenerInstance>();
                foreach (var item in eventInstance.ListenerInstances)
                {
                    item.EventInstanceId = eventInstance.Id;
                    var listenerInstance = this._ListenerInstanceRepo.Create(item, conn, trans);
                    listenerInstances.Add(listenerInstance);
                }

                eventInstance.ListenerInstances.Clear();
                eventInstance.ListenerInstances.AddRange(listenerInstances);

                this._ConnectionRepo.CommitTransaction(trans);

                resultInstance = eventInstance;
            }
            catch (Exception ex)
            {
                this._ConnectionRepo.RollbackTransaction(trans);

                log = GetLogModelException(log, ex);
                this._Logger.Add(log);
            }
            finally
            {
                this._ConnectionRepo.CloseConnection(conn);
            }

            this._ConnectionRepo.DisposeConnection(conn);

            //add end log
            log.LogType = LogModel.LogTypes.Debug;
            this._Logger.Add(this.GetLogModelMethodEnd(log));

            return resultInstance;
        }

        private void FetchListenersForEventInstance(Models.Instance.EventInstance eventInstance)
        {
            eventInstance.ListenerInstances.Clear();
            var relations = this._DefinitionManager.EventDefinitionListenerDefinitionRelations.Where(r => r.EventDefinitiontId == eventInstance.EventDefinitionId);
            foreach (var relation in relations)
            {
                var listenerInstance = new Models.Instance.ListenerInstance()
                {
                    Active = true,
                    CreateBy = InstanceConstants.USER_SYSTEM_EVENT_MANAGER,
                    CreateDate = DateTime.UtcNow,
                    ListenerDefinitionId = relation.ListenerDefinitionId,
                    ModifyBy = InstanceConstants.USER_SYSTEM_EVENT_MANAGER,
                    ModifyDate = DateTime.UtcNow,
                    NextRun = DateTime.UtcNow,
                    RemainingTrialCount = this._DefinitionManager.ListenerDefinitions.Single(l => l.Id == relation.ListenerDefinitionId).TrialCount,
                    Status = Models.Instance.Enums.ListenerInstanceStatus.InQueue
                };

                eventInstance.ListenerInstances.Add(listenerInstance);
            }
        }

        public List<Models.Instance.EventInstance> GetEventInstancesToProcess(DateTime runDate)
        {
            //intialize log and add first log
            var log = this.GetLogModelMethodStart(MethodBase.GetCurrentMethod().Name, string.Empty, string.Empty);
            log.NotesA = $"runDate {runDate:yyyy-MM-dd HH:mm:ss}";
            this._Logger.Add(log);

            List<Models.Instance.EventInstance> results = null;

            var readOperation = new Dictionary<string, string>
            {
                { Models.Instance.Enums.EventInstanceReadOperations.ReadNotProcessed.ToString(), runDate.ToString() }
            };

            object conn = null;

            try
            {
                conn = this._ConnectionRepo.CreateConnection();
                this._ConnectionRepo.OpenConnection(conn);

                results = this._EventInstanceRepo.Read(readOperation, conn, null);
            }
            catch (Exception ex)
            {
                log = GetLogModelException(log, ex);
                this._Logger.Add(log);
            }
            finally
            {
                this._ConnectionRepo.CloseConnection(conn);
                this._ConnectionRepo.DisposeConnection(conn);
            }

            foreach (var eventInstance in results)
            {
                EnrichListenerDefinition(eventInstance);
                EnrichEventDefinition(eventInstance);
            }

            log.Step = "events count";
            log.NotesA += $"{Environment.NewLine} | {results.Count} events to be processed";
            log.LogType = LogModel.LogTypes.Information;
            this._Logger.Add(log);

            this._Logger.Add(this.GetLogModelMethodEnd(log));

            return results;
        }

        private void EnrichListenerDefinition(Models.Instance.EventInstance eventInstance)
        {
            foreach (var listenerInstnace in eventInstance.ListenerInstances)
            {
                listenerInstnace.Definition = this._DefinitionManager.ListenerDefinitions.Single(d => d.Id == listenerInstnace.ListenerDefinitionId);
            }
        }

        private void EnrichEventDefinition(Models.Instance.EventInstance eventInstance)
        {
            eventInstance.Definition = this._DefinitionManager.EventDefinitions.Single(d => d.Id == eventInstance.EventDefinitionId);
        }

        public void Process(List<Models.Instance.EventInstance> eventInstances)
        {
            //intialize log and add first log
            var log = this.GetLogModelMethodStart(MethodBase.GetCurrentMethod().Name,string.Empty, string.Empty);
            this._Logger.Add(log);

            foreach (var eventInstance in eventInstances)
            {
                Process(eventInstance);
            }

            //add end log
            this._Logger.Add(this.GetLogModelMethodEnd(log));
        }

        private void Process(Models.Instance.EventInstance eventInstance)
        {
            //intialize log and add first log
            var log = this.GetLogModelMethodStart(MethodBase.GetCurrentMethod().Name, "eventInstance.Id", eventInstance.Id.ToString());
            log.ReferenceName = "eventInstance.Id";
            log.ReferenceValue = eventInstance.Id.ToString();
            this._Logger.Add(log);

            //todo: updating the event instance and listener instance should be in the same transaction
            eventInstance.ModifyBy = InstanceConstants.USER_SYSTEM_PROCESSOR;
            eventInstance.ModifyDate = DateTime.UtcNow;
            eventInstance.Status = Models.Instance.Enums.EventInstanceStatus.Processing;

            object conn = this._ConnectionRepo.CreateConnection();
            try
            {
                this._ConnectionRepo.OpenConnection(conn);

                this._EventInstanceRepo.Edit(eventInstance, conn, null);
                log.Counter++;
                log.Step = "event instance status updated";
                log.NotesA = eventInstance.ToString();
                log.LogType = LogModel.LogTypes.Debug;
                this._Logger.Add(log);

            }
            catch(Exception ex)
            {
                log = GetLogModelException(log, ex);
                this._Logger.Add(log);
            }
            finally
            {
                this._ConnectionRepo.CloseConnection(conn);
            }

            string enhancedEventData = InjectSimpleHookMetaDataInEventDate(eventInstance);

            foreach (var item in eventInstance.ListenerInstances)
            {
                ExecuteListener(item, enhancedEventData);
            }

            RefereshListeners(eventInstance);
            SetEventInstanceStatus(eventInstance);

            try
            {
                this._ConnectionRepo.OpenConnection(conn);
                this._EventInstanceRepo.Edit(eventInstance, conn, null);
                log.Counter++;
                log.Step = "event instance status updated";
                log.NotesA = eventInstance.ToString();
                log.LogType = LogModel.LogTypes.Debug;
                this._Logger.Add(log);
            }
            catch (Exception ex)
            {
                log = GetLogModelException(log, ex);
                this._Logger.Add(log);
            }
            finally
            {
                this._ConnectionRepo.CloseConnection(conn);
            }

            this._ConnectionRepo.DisposeConnection(conn);

            this._Logger.Add(this.GetLogModelMethodEnd(log));
        }

        private string InjectSimpleHookMetaDataInEventDate(Models.Instance.EventInstance eventInstance)
        {
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

                return jsonData.ToString();
            }
            catch
            {
                //todo:log exception
                return eventInstance.EventData;
            }
        }

        private void RefereshListeners(Models.Instance.EventInstance eventInstance)
        {
            var log = this.GetLogModelMethodStart(MethodBase.GetCurrentMethod().Name, "eventInstance.Id", eventInstance.Id.ToString());
            this._Logger.Add(log);

            var readOperation = new Dictionary<string, string>
            {
                { Models.Instance.Enums.ListenerInstanceReadOperations.ReadByEventInstanceId.ToString(), eventInstance.Id.ToString() }
            };

            object conn = this._ConnectionRepo.CreateConnection();
            List<Models.Instance.ListenerInstance> listeners = null;
            try
            {
                this._ConnectionRepo.OpenConnection(conn);
                listeners = this._ListenerInstanceRepo.Read(readOperation, conn, null);
            }
            catch (Exception ex)
            {
                log = GetLogModelException(log, ex);
                this._Logger.Add(log);
            }
            finally
            {
                this._ConnectionRepo.CloseConnection(conn);
            }

            this._ConnectionRepo.DisposeConnection(conn);

            eventInstance.ListenerInstances.Clear();
            eventInstance.ListenerInstances.AddRange(listeners);

            this._Logger.Add(this.GetLogModelMethodEnd(log));
        }

        private void SetEventInstanceStatus(Models.Instance.EventInstance eventInstance)
        {
            #region set default values
            eventInstance.ModifyBy = InstanceConstants.USER_SYSTEM_PROCESSOR;
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
            //intialize log and add first log
            var log = this.GetLogModelMethodStart(MethodBase.GetCurrentMethod().Name, "listenerInstance.Id", listenerInstance.Id.ToString());
            this._Logger.Add(log);

            listenerInstance.ModifyBy = InstanceConstants.USER_SYSTEM_PROCESSOR;
            listenerInstance.RemainingTrialCount--;
            listenerInstance.Status = Models.Instance.Enums.ListenerInstanceStatus.Processing;

            object conn = this._ConnectionRepo.CreateConnection();

            var succeeded = UpdateListenerInstanceStatus(listenerInstance, log, conn);

            if (!succeeded)
            {
                this._ConnectionRepo.DisposeConnection(conn);
                return;
            }
            
            var result = this._HttpClient.Post(listenerInstance.Definition.URL, listenerInstance.Definition.Headers, eventData, listenerInstance.Definition.Timeout);

            #region handle http client result
            var parameters = new Dictionary<string, string>();
            parameters.Add("ListenerInstance", listenerInstance.ToString());
            parameters.Add("eventData", eventData);
            parameters.Add("result", result.ToString());

            //succeeded
            if (result.HttpCode == InstanceConstants.HTTP_CODE_SUCCEEDED)
            {
                listenerInstance.Status = Models.Instance.Enums.ListenerInstanceStatus.Succeeded;

                UpdateListenerInstanceStatus(listenerInstance, log, conn);

                log.Counter++;
                log.Step = "At the end of execute listener";
                log.LogType = LogModel.LogTypes.Information;
                log.NotesA = Newtonsoft.Json.JsonConvert.SerializeObject(parameters);
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

            this._Logger.Add(log);

            this._ConnectionRepo.DisposeConnection(conn);
        }
    
        private bool UpdateListenerInstanceStatus(Models.Instance.ListenerInstance listenerInstance, LogModel log, object conn)
        {
            bool succeeded = true;
            listenerInstance.ModifyDate = DateTime.UtcNow;

            try
            {
                this._ConnectionRepo.OpenConnection(conn);
                this._ListenerInstanceRepo.Edit(listenerInstance, conn, null);
                log.Counter++;
                log.Step = "listener instance status updated";
                log.NotesA = listenerInstance.ToString();
                log.LogType = LogModel.LogTypes.Debug;
                this._Logger.Add(log);
            }
            catch (Exception ex)
            {
                log = GetLogModelException(log, ex);
                this._Logger.Add(log);
                succeeded = false;
            }
            finally
            {
                this._ConnectionRepo.CloseConnection(conn);
            }

            return succeeded;
        }
    }
}
