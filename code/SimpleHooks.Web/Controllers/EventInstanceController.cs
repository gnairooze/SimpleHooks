using Microsoft.AspNetCore.Mvc;
using System;

namespace SimpleHooks.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventInstanceController : ControllerBase
    {
        private readonly Business.InstanceManager _manager;

        public EventInstanceController(
            Log.Interface.ILog logger,
            Interfaces.IConnectionRepository connectionRepo,
            Interfaces.IDataRepository<global::Models.Instance.EventInstance> eventInstanceRepo,
            Interfaces.IDataRepository<global::Models.Instance.ListenerInstance> listenerInstanceRepo,
            HttpClient.Interface.IHttpClient httpClient,
            Interfaces.IDataRepository<global::Models.Definition.EventDefinition> eventDefRepo,
            Interfaces.IDataRepository<global::Models.Definition.ListenerDefinition> listenerDefRepo,
            Interfaces.IDataRepository<global::Models.Definition.EventDefinitionListenerDefinition> eventDefListenerDefRepo,
            Interfaces.IDataRepository<global::Models.Definition.AppOption> appOptionRepo) => _manager = new Business.InstanceManager(
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

        [Route("get-status")]
        [HttpGet]
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
