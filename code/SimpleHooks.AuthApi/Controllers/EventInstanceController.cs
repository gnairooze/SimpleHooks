using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleTools.SimpleHooks.AuthApi.Helper;
using SimpleTools.SimpleHooks.Business;
using SimpleTools.SimpleHooks.Interfaces;
using SimpleTools.SimpleHooks.Log.Interface;
using SimpleTools.SimpleHooks.Models.Instance;
using System;

namespace SimpleTools.SimpleHooks.AuthApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EventInstanceController : ControllerBase
    {
        private readonly Business.InstanceManager _manager;

        public EventInstanceController(
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
            Interfaces.IDataRepository<SimpleTools.SimpleHooks.Models.Definition.AppOption> appOptionRepo) => _manager = new SimpleTools.SimpleHooks.Business.InstanceManager(
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

        [Route("get-status")]
        [HttpGet]
        [Authorize(Policy = AuthPolicies.REQUIRE_SIMPLEHOOKS_API_GET_EVENT_INSTANCE_STATUS)]
        public IActionResult GetStatus(Guid businessId)
        {
            var result = _manager.ReadEventInstanceStatusByBusinessId(businessId);

            if (result.BusinessId == Guid.Empty)
            {
                return NotFound($"no event instance found for business id = {businessId}");
            }
            return Ok(result);
        }
    }
}
