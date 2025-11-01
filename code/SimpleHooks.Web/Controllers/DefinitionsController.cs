using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleTools.SimpleHooks.Business;
using SimpleTools.SimpleHooks.Interfaces;
using System;

namespace SimpleTools.SimpleHooks.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DefinitionsController : ControllerBase
    {
        private readonly Business.InstanceManager _manager;

        public DefinitionsController(
            Log.Interface.ILog logger,
            Interfaces.IConnectionRepository connectionRepo,
            Interfaces.IDataRepository<SimpleTools.SimpleHooks.Models.Instance.EventInstance> eventInstanceRepo,
            Interfaces.IDataRepository<SimpleTools.SimpleHooks.Models.Instance.ListenerInstance> listenerInstanceRepo,
            HttpClient.Interface.IHttpClient httpClient,
            Interfaces.IDataRepository<SimpleTools.SimpleHooks.Models.Definition.EventDefinition> eventDefRepo,
            Interfaces.IDataRepository<SimpleTools.SimpleHooks.Models.Definition.ListenerDefinition> listenerDefRepo,
            Interfaces.IDataRepository<SimpleTools.SimpleHooks.Models.Definition.EventDefinitionListenerDefinition> eventDefListenerDefRepo,
            IDataRepository<SimpleTools.SimpleHooks.Models.Definition.ListenerType> listenerTypeRepo,
            ListenerPluginManager listenerPluginManager,
            Interfaces.IDataRepository<SimpleTools.SimpleHooks.Models.Definition.AppOption> appOptionRepo)
        {
            _manager = new SimpleTools.SimpleHooks.Business.InstanceManager(
                logger,
                connectionRepo,
                eventInstanceRepo,
                listenerInstanceRepo,
                httpClient,
                eventDefRepo,
                listenerDefRepo,
                eventDefListenerDefRepo,
                listenerTypeRepo,
                listenerPluginManager,
                appOptionRepo
            );
        }

        [Route("load-definitions")]
        [HttpPost]
        public OkObjectResult LoadDefinitions()
        {
            bool succeeded = _manager.DefinitionMgr.LoadDefinitions();

            if (!succeeded)
            {
                throw new InvalidOperationException("could not load definitions");
            }

            return Ok(_manager.DefinitionMgr.AppOptions);
        }
    }
}
