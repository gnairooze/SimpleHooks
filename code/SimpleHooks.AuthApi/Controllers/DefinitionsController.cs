using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleTools.SimpleHooks.AuthApi.Helper;

namespace SimpleTools.SimpleHooks.AuthApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
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
                appOptionRepo
            );
        }
        
        [Route("load-definitions")]
        [HttpPost]
        [Authorize(Policy = AuthPolicies.REQUIRE_SIMPLEHOOKS_API_LOADDEFINITIONS)]
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
