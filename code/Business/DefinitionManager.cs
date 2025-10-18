using SimpleTools.SimpleHooks.Interfaces;
using SimpleTools.SimpleHooks.Log.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SimpleTools.SimpleHooks.Business
{
    public class DefinitionManager:LogBase
    {
        private readonly ILog _logger;

        private readonly IConnectionRepository _connectionRepo;
        private readonly IDataRepository<Models.Definition.AppOption> _appOptionRepo;
        private readonly IDataRepository<Models.Definition.EventDefinition> _eventDefRepo;
        private readonly IDataRepository<Models.Definition.ListenerDefinition> _listenerDefRepo;
        private readonly IDataRepository<Models.Definition.ListenerType> _listenerTypeRepo;
        private readonly IDataRepository<Models.Definition.EventDefinitionListenerDefinition> _eventDefListenerDefRepo;
        private readonly ListenerPluginManager _listenerPluginManager;

        public List<Models.Definition.AppOption> AppOptions { get; }

        public List<Models.Definition.EventDefinition> EventDefinitions { get; }

        public List<Models.Definition.ListenerDefinition> ListenerDefinitions { get; }

        public List<Models.Definition.ListenerType> ListenerTypes { get; }

        public List<Models.Definition.EventDefinitionListenerDefinition> EventDefinitionListenerDefinitionRelations { get; }

        public EventHandler DefitionsLoaded;

        public DefinitionManager(
            ILog logger,
            IDataRepository<Models.Definition.EventDefinition> eventDefRepo,
            IDataRepository<Models.Definition.ListenerDefinition> listenerDefRepo,
            IDataRepository<Models.Definition.EventDefinitionListenerDefinition> eventDefListenerDefRepo,
            IDataRepository<Models.Definition.AppOption> appOptionRepo,
            IDataRepository<Models.Definition.ListenerType> listenerTypeRepo,
            IConnectionRepository connectionRepo,
            ListenerPluginManager listenerPluginManager)
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._appOptionRepo = appOptionRepo ?? throw new ArgumentNullException(nameof(appOptionRepo));
            this._eventDefRepo = eventDefRepo ?? throw new ArgumentNullException(nameof(eventDefRepo));
            this._listenerDefRepo = listenerDefRepo ?? throw new ArgumentNullException(nameof(listenerDefRepo));
            this._listenerTypeRepo = listenerTypeRepo ?? throw new ArgumentNullException(nameof(listenerTypeRepo));
            this._eventDefListenerDefRepo = eventDefListenerDefRepo ?? throw new ArgumentNullException(nameof(eventDefListenerDefRepo));
            this._connectionRepo = connectionRepo ?? throw new ArgumentNullException(nameof(connectionRepo));
            this._listenerPluginManager = listenerPluginManager ?? throw new ArgumentNullException(nameof(listenerPluginManager));

            this.AppOptions = new List<Models.Definition.AppOption>();
            this.EventDefinitions = new List<Models.Definition.EventDefinition>();
            this.ListenerDefinitions = new List<Models.Definition.ListenerDefinition>();
            this.ListenerTypes = new List<Models.Definition.ListenerType>();
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
                this.AppOptions.AddRange(this._appOptionRepo.Read(null, conn));
                this.EventDefinitions.Clear();
                this.EventDefinitions.AddRange(this._eventDefRepo.Read(null, conn));

                // Load ListenerTypes FIRST before ListenerDefinitions
                this.ListenerTypes.Clear();
                this.ListenerTypes.AddRange(this._listenerTypeRepo.Read(null, conn));

                // Load ListenerDefinitions AFTER ListenerTypes
                this.ListenerDefinitions.Clear();
                this.ListenerDefinitions.AddRange(this._listenerDefRepo.Read(null, conn));

                this.EventDefinitionListenerDefinitionRelations.Clear();
                this.EventDefinitionListenerDefinitionRelations.AddRange(this._eventDefListenerDefRepo.Read(null, conn));

                // Initialize listener plugins for each ListenerDefinition
                this.SetListenerPlugins();

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

        /// <summary>
        /// Initializes listener plugins for all listener definitions.
        /// Called after ListenerTypes and ListenerDefinitions are loaded.
        /// </summary>
        /// <param name="conn">Database connection (not used but kept for consistency)</param>
        private void SetListenerPlugins()
        {
            var log = this.GetLogModelMethodStart(MethodBase.GetCurrentMethod()?.Name, string.Empty, string.Empty);
            this._logger.Add(log);

            try
            {
                // Loop over each ListenerDefinition
                foreach (var listenerDef in this.ListenerDefinitions)
                {
                    try
                    {
                        // Find the related ListenerType
                        var listenerType = this.ListenerTypes.FirstOrDefault(lt => lt.Id == listenerDef.TypeId);

                        if (listenerType == null)
                        {
                            // Log error if ListenerType not found
                            var errorLog = new Log.Interface.LogModel
                            {
                                LogType = Log.Interface.LogModel.LogTypes.Error,
                                CreateDate = DateTime.UtcNow,
                                Operation = MethodBase.GetCurrentMethod()?.Name,
                                Step = "ListenerType Not Found",
                                ReferenceName = "ListenerDefinition.Id",
                                ReferenceValue = listenerDef.Id.ToString(),
                                NotesA = $"ListenerType with Id {listenerDef.TypeId} not found for ListenerDefinition {listenerDef.Id}"
                            };
                            this._logger.Add(errorLog);
                            continue;
                        }

                        // Create plugin instance
                        listenerDef.ListenerPlugin = _listenerPluginManager.CreatePluginInstance(
                            path: listenerType.Path,
                            url: listenerDef.Url,
                            timeout: listenerDef.Timeout,
                            headers: listenerDef.Headers
                        );
                    }
                    catch (Exception ex)
                    {
                        // Log error for this specific listener but continue with others
                        var errorLog = new Log.Interface.LogModel
                        {
                            LogType = Log.Interface.LogModel.LogTypes.Error,
                            CreateDate = DateTime.UtcNow,
                            Operation = MethodBase.GetCurrentMethod()?.Name,
                            Step = "Plugin Creation Failed",
                            ReferenceName = "ListenerDefinition.Id",
                            ReferenceValue = listenerDef.Id.ToString(),
                            NotesA = $"Failed to create plugin for ListenerDefinition {listenerDef.Id}",
                            NotesB = Newtonsoft.Json.JsonConvert.SerializeObject(ex)
                        };
                        this._logger.Add(errorLog);
                    }
                }

                log = this.GetLogModelMethodEnd(log);
                this._logger.Add(log);
            }
            catch (Exception ex)
            {
                log = GetLogModelException(log, ex);
                this._logger.Add(log);
                throw;
            }
        }
    }
}
