using Interfaces;
using Log.Interface;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Business
{
    public class DefinitionManager:LogBase
    {
        private readonly ILog _Logger;

        private readonly IConnectionRepository _ConnectionRepo;
        private readonly IDataRepository<Models.Definition.AppOption> _AppOptionRepo;
        private readonly IDataRepository<Models.Definition.EventDefinition> _EventDefRepo;
        private readonly IDataRepository<Models.Definition.ListenerDefinition> _ListenerDefRepo;
        private readonly IDataRepository<Models.Definition.EventDefinitionListenerDefinition> _EventDefListenerDefRepo;

        public List<Models.Definition.AppOption> AppOptions { get; }

        public List<Models.Definition.EventDefinition> EventDefinitions { get; }

        public List<Models.Definition.ListenerDefinition> ListenerDefinitions { get; }
        
        public List<Models.Definition.EventDefinitionListenerDefinition> EventDefinitionListenerDefinitionRelations { get; }

        public DefinitionManager(
            ILog logger, 
            IDataRepository<Models.Definition.EventDefinition> eventDefRepo, 
            IDataRepository<Models.Definition.ListenerDefinition> listenerDefRepo, 
            IDataRepository<Models.Definition.EventDefinitionListenerDefinition> eventDefListenerDefRepo, 
            IDataRepository<Models.Definition.AppOption> appOptionRepo,
            IConnectionRepository connectionRepo)
        {
            this._Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._AppOptionRepo = appOptionRepo ?? throw new ArgumentNullException(nameof(appOptionRepo));
            this._EventDefRepo = eventDefRepo ?? throw new ArgumentNullException(nameof(eventDefRepo));
            this._ListenerDefRepo = listenerDefRepo ?? throw new ArgumentNullException(nameof(listenerDefRepo));
            this._EventDefListenerDefRepo = eventDefListenerDefRepo ?? throw new ArgumentNullException(nameof(eventDefListenerDefRepo));
            this._ConnectionRepo = connectionRepo ?? throw new ArgumentNullException(nameof(connectionRepo));

            this.AppOptions = new List<Models.Definition.AppOption>();
            this.EventDefinitions = new List<Models.Definition.EventDefinition>();
            this.ListenerDefinitions = new List<Models.Definition.ListenerDefinition>();
            this.EventDefinitionListenerDefinitionRelations = new List<Models.Definition.EventDefinitionListenerDefinition>();
        }

        public bool LoadDefinitions()
        {
            bool succeeded = true;

            //intialize log and add first log
            var log = this.GetLogModelMethodStart(MethodBase.GetCurrentMethod().Name, string.Empty, string.Empty);
            this._Logger.Add(log);

            var conn = this._ConnectionRepo.CreateConnection();

            try
            {
                this._ConnectionRepo.OpenConnection(conn);

                //set definitions
                this.AppOptions.Clear();
                this.AppOptions.AddRange(this._AppOptionRepo.Read(null, conn, null));
                this.EventDefinitions.Clear();
                this.EventDefinitions.AddRange(this._EventDefRepo.Read(null, conn, null));
                this.ListenerDefinitions.Clear();
                this.ListenerDefinitions.AddRange(this._ListenerDefRepo.Read(null, conn, null));
                this.EventDefinitionListenerDefinitionRelations.Clear();
                this.EventDefinitionListenerDefinitionRelations.AddRange(this._EventDefListenerDefRepo.Read(null, conn, null));
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

            this._ConnectionRepo.DisposeConnection(conn);

            //add end log
            this._Logger.Add(this.GetLogModelMethodEnd(log));

            return succeeded;
        }
    }
}
