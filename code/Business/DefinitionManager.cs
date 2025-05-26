using Interfaces;
using Log.Interface;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Business
{
    public class DefinitionManager:LogBase
    {
        private readonly ILog _logger;

        private readonly IConnectionRepository _connectionRepo;
        private readonly IDataRepository<Models.Definition.AppOption> _appOptionRepo;
        private readonly IDataRepository<Models.Definition.EventDefinition> _eventDefRepo;
        private readonly IDataRepository<Models.Definition.ListenerDefinition> _listenerDefRepo;
        private readonly IDataRepository<Models.Definition.EventDefinitionListenerDefinition> _eventDefListenerDefRepo;

        public List<Models.Definition.AppOption> AppOptions { get; }

        public List<Models.Definition.EventDefinition> EventDefinitions { get; }

        public List<Models.Definition.ListenerDefinition> ListenerDefinitions { get; }
        
        public List<Models.Definition.EventDefinitionListenerDefinition> EventDefinitionListenerDefinitionRelations { get; }

        public EventHandler DefitionsLoaded;

        public DefinitionManager(
            ILog logger, 
            IDataRepository<Models.Definition.EventDefinition> eventDefRepo, 
            IDataRepository<Models.Definition.ListenerDefinition> listenerDefRepo, 
            IDataRepository<Models.Definition.EventDefinitionListenerDefinition> eventDefListenerDefRepo, 
            IDataRepository<Models.Definition.AppOption> appOptionRepo,
            IConnectionRepository connectionRepo)
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._appOptionRepo = appOptionRepo ?? throw new ArgumentNullException(nameof(appOptionRepo));
            this._eventDefRepo = eventDefRepo ?? throw new ArgumentNullException(nameof(eventDefRepo));
            this._listenerDefRepo = listenerDefRepo ?? throw new ArgumentNullException(nameof(listenerDefRepo));
            this._eventDefListenerDefRepo = eventDefListenerDefRepo ?? throw new ArgumentNullException(nameof(eventDefListenerDefRepo));
            this._connectionRepo = connectionRepo ?? throw new ArgumentNullException(nameof(connectionRepo));

            this.AppOptions = new List<Models.Definition.AppOption>();
            this.EventDefinitions = new List<Models.Definition.EventDefinition>();
            this.ListenerDefinitions = new List<Models.Definition.ListenerDefinition>();
            this.EventDefinitionListenerDefinitionRelations = new List<Models.Definition.EventDefinitionListenerDefinition>();
        }

        public bool LoadDefinitions()
        {
            bool succeeded = true;

            //initialize log and add first log
            var log = this.GetLogModelMethodStart(MethodBase.GetCurrentMethod()?.Name, string.Empty, string.Empty);
            this._logger.Add(log);

            var conn = this._connectionRepo.CreateConnection();

            try
            {
                this._connectionRepo.OpenConnection(conn);

                //set definitions
                this.AppOptions.Clear();
                this.AppOptions.AddRange(this._appOptionRepo.Read(null, conn, null));
                this.EventDefinitions.Clear();
                this.EventDefinitions.AddRange(this._eventDefRepo.Read(null, conn, null));
                this.ListenerDefinitions.Clear();
                this.ListenerDefinitions.AddRange(this._listenerDefRepo.Read(null, conn, null));
                this.EventDefinitionListenerDefinitionRelations.Clear();
                this.EventDefinitionListenerDefinitionRelations.AddRange(this._eventDefListenerDefRepo.Read(null, conn, null));

                //trigger event for definitions loaded
                this.DefitionsLoaded?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                log = GetLogModelException(log, ex);
                this._logger.Add(log);
                succeeded = false;
            }
            finally
            {
                this._connectionRepo.CloseConnection(conn);
            }

            this._connectionRepo.DisposeConnection(conn);

            //add end log
            this._logger.Add(this.GetLogModelMethodEnd(log));

            return succeeded;
        }
    }
}
