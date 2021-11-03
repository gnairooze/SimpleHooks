using HttpClient.Interface;
using Log.Interface;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Business
{
    public class InstanceManager:LogBase
    {
        private readonly ILog _Logger;

        private readonly Interfaces.IRepository<Models.Instance.EventInstance> _EventInstanceRepo;
        private readonly Interfaces.IRepository<Models.Instance.ListenerInstance> _ListenerInstanceRepo;
        private readonly IHttpClient _HttpClient;

        public InstanceManager(ILog logger, Interfaces.IRepository<Models.Instance.EventInstance> eventInstanceRepo, Interfaces.IRepository<Models.Instance.ListenerInstance> listenerInstanceRepo, HttpClient.Interface.IHttpClient httpClient)
        {
            this._Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            this._EventInstanceRepo = eventInstanceRepo ?? throw new ArgumentNullException(nameof(eventInstanceRepo));
            this._ListenerInstanceRepo = listenerInstanceRepo ?? throw new ArgumentNullException(nameof(listenerInstanceRepo));
            this._HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }
        public Models.Instance.EventInstance Add(Models.Instance.EventInstance eventInstance)
        {
            //intialize log and add first log
            var log = this.GetLogModelMethodStart(MethodBase.GetCurrentMethod().Name);
            this._Logger.Add(log);

            Models.Instance.EventInstance resultInstance = null;
            //todo: fetch listener definitions
            //set listenerinstance remaining trial count from definition and set next run to utc now
            object trans = null;

            try
            {
                this._EventInstanceRepo.OpenConnection();
                trans = this._EventInstanceRepo.BeginTransaction();


                eventInstance = this._EventInstanceRepo.Create(eventInstance, trans);
                List<Models.Instance.ListenerInstance> listenerInstances = new List<Models.Instance.ListenerInstance>();
                foreach (var item in eventInstance.ListenerInstances)
                {
                    var listenerInstance = this._ListenerInstanceRepo.Create(item, trans);
                    listenerInstances.Add(listenerInstance);
                }

                eventInstance.ListenerInstances.Clear();
                eventInstance.ListenerInstances.AddRange(listenerInstances);

                this._EventInstanceRepo.CommitTransaction(trans);

                resultInstance = eventInstance;
            }
            catch (Exception ex)
            {
                this._EventInstanceRepo.RollbackTransaction(trans);

                log = GetLogModelException(log, ex);
                this._Logger.Add(log);
            }
            finally
            {
                this._EventInstanceRepo.CloseConnection();
            }

            //add end log
            this._Logger.Add(this.GetLogModelMethodEnd(log));

            return resultInstance;
        }

        public List<Models.Instance.EventInstance> GetEventInstancesToProcess(DateTime runDate)
        {
            var readOperation = new Dictionary<string, string>
            {
                { Models.Instance.Enums.EventInstanceReadOperations.ReadNotProcessed.ToString(), runDate.ToString() }
            };

            return this._EventInstanceRepo.Read(readOperation, null);
        }

        public void Process(List<Models.Instance.EventInstance> eventInstances)
        {
            foreach (var eventInstance in eventInstances)
            {
                Process(eventInstance);
            }
        }

        private void Process(Models.Instance.EventInstance eventInstance)
        {
            //todo: updating the event instance and listener instance should be in the same transaction
            eventInstance.ModifyBy = InstanceConstants.USER_SYSTEM_PROCESSOR;
            eventInstance.ModifyDate = DateTime.UtcNow;
            eventInstance.Status = Models.Instance.Enums.EventInstanceStatus.Processing;
            this._EventInstanceRepo.Edit(eventInstance, null);

            foreach (var item in eventInstance.ListenerInstances)
            {
                ExecuteListener(item, eventInstance.EventDate);
            }

            RefereshListeners(eventInstance);
            SetEventInstanceStatus(eventInstance);
            this._EventInstanceRepo.Edit(eventInstance, null);
        }

        private void RefereshListeners(Models.Instance.EventInstance eventInstance)
        {
            var readOperation = new Dictionary<string, string>
            {
                { Models.Instance.Enums.ListenerInstanceReadOperations.ReadByEventInstanceId.ToString(), eventInstance.Id.ToString() }
            };

            var listeners = this._ListenerInstanceRepo.Read(readOperation, null);

            eventInstance.ListenerInstances.Clear();
            eventInstance.ListenerInstances.AddRange(listeners);
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

        private void ExecuteListener(Models.Instance.ListenerInstance listenerInstance, JObject eventData)
        {
            listenerInstance.ModifyBy = InstanceConstants.USER_SYSTEM_PROCESSOR;
            listenerInstance.ModifyDate = DateTime.UtcNow;
            listenerInstance.RemainingTrialCount--;
            listenerInstance.Status = Models.Instance.Enums.ListenerInstanceStatus.Processing;
            this._ListenerInstanceRepo.Edit(listenerInstance, null);

            var result = this._HttpClient.Post(listenerInstance.Definition.URL, listenerInstance.Definition.Headers, eventData, listenerInstance.Definition.Timeout);

            #region handle http client result
            //succeeded
            if (result.HttpCode == InstanceConstants.HTTP_CODE_SUCCEEDED)
            {
                listenerInstance.Status = Models.Instance.Enums.ListenerInstanceStatus.Succeeded;
                listenerInstance.ModifyDate = DateTime.UtcNow;
                this._ListenerInstanceRepo.Edit(listenerInstance, null);

                return;
            }

            //failed with no remaining retrials
            if(listenerInstance.RemainingTrialCount <= 0)
            {
                listenerInstance.ModifyDate = DateTime.UtcNow;
                listenerInstance.Status = Models.Instance.Enums.ListenerInstanceStatus.Failed;
                this._ListenerInstanceRepo.Edit(listenerInstance, null);
                return;
            }

            //failed with remaining retrials
            listenerInstance.Status = Models.Instance.Enums.ListenerInstanceStatus.WaitingForRetrial;
            listenerInstance.ModifyDate = DateTime.UtcNow;
            listenerInstance.NextRun = listenerInstance.ModifyDate.Add(TimeSpan.FromMinutes(listenerInstance.Definition.RetrialDelay));
            this._ListenerInstanceRepo.Edit(listenerInstance, null);
            #endregion
        }
    }
}
