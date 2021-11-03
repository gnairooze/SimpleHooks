using Log.Interface;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Business
{
    public class DefinitionManager:LogBase
    {
        private readonly ILog _Logger;

        private readonly Interfaces.IRepository<Models.Definition.AppOption> _AppOptionRepo;
        private readonly Interfaces.IRepository<Models.Definition.EventDefinition> _EventDefRepo;
        private readonly Interfaces.IRepository<Models.Definition.ListenerDefinition> _ListenerDefRepo;
        private readonly Interfaces.IRepository<Models.Definition.EventDefinitionListenerDefinition> _EventDefListenerDefRepo;

        public List<Models.Definition.AppOption> AppOptions { get; }

        public List<Models.Definition.EventDefinition> EventDefinitions { get; }

        public List<Models.Definition.ListenerDefinition> ListenerDefinitions { get; }
        
        public List<Models.Definition.EventDefinitionListenerDefinition> EventDefinitionListenerDefinitionRelations { get; }

        public DefinitionManager(ILog logger, Interfaces.IRepository<Models.Definition.EventDefinition> eventDefRepo, Interfaces.IRepository<Models.Definition.ListenerDefinition> listenerDefRepo, Interfaces.IRepository<Models.Definition.EventDefinitionListenerDefinition> eventDefListenerDefRepo, Interfaces.IRepository<Models.Definition.AppOption> appOptionRepo)
        {
            this._Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._AppOptionRepo = appOptionRepo ?? throw new ArgumentNullException(nameof(appOptionRepo));
            this._EventDefRepo = eventDefRepo ?? throw new ArgumentNullException(nameof(eventDefRepo));
            this._ListenerDefRepo = listenerDefRepo ?? throw new ArgumentNullException(nameof(listenerDefRepo));
            this._EventDefListenerDefRepo = eventDefListenerDefRepo ?? throw new ArgumentNullException(nameof(eventDefListenerDefRepo));

            this.AppOptions = new List<Models.Definition.AppOption>();
            this.EventDefinitions = new List<Models.Definition.EventDefinition>();
            this.ListenerDefinitions = new List<Models.Definition.ListenerDefinition>();
            this.EventDefinitionListenerDefinitionRelations = new List<Models.Definition.EventDefinitionListenerDefinition>();
        }

        public void LoadDefinitions()
        {
            //intialize log and add first log
            var log = this.GetLogModelMethodStart(MethodBase.GetCurrentMethod().Name);
            this._Logger.Add(log);

            //set definitions
            this.AppOptions.Clear();
            this.AppOptions.AddRange(this._AppOptionRepo.Read(null, null));
            this.EventDefinitions.Clear();
            this.EventDefinitions.AddRange(this._EventDefRepo.Read(null, null));
            this.ListenerDefinitions.Clear();
            this.ListenerDefinitions.AddRange(this._ListenerDefRepo.Read(null, null));
            this.EventDefinitionListenerDefinitionRelations.Clear();
            this.EventDefinitionListenerDefinitionRelations.AddRange(this._EventDefListenerDefRepo.Read(null, null));

            //add end log
            this._Logger.Add(this.GetLogModelMethodEnd(log));
        }
    }
}
