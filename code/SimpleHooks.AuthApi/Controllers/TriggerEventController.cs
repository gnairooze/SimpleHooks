using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleTools.SimpleHooks.AuthApi.Helper;

namespace SimpleTools.SimpleHooks.AuthApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TriggerEventController : ControllerBase
    {
        private readonly Business.InstanceManager _manager;

        public TriggerEventController(
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

        // POST api/<TriggerEventController>
        [HttpPost]
        [Authorize(Policy = AuthPolicies.REQUIRE_SIMPLEHOOKS_API_TRIGGEREVENT)]
        public OkObjectResult Post([FromBody] Models.EventViewModel value)
        {
            // Extract the EventData node from the request
            var eventDataJson = value.EventData.TryGetValue("EventData", out var eventDataNode)
                ? eventDataNode.GetRawText()
                : "{}";

            var result = _manager.Add(new SimpleTools.SimpleHooks.Models.Instance.EventInstance() {
                Active = true,
                BusinessId = Guid.NewGuid(),
                CreateBy = "system.trigger",
                CreateDate = DateTime.UtcNow,
                EventData = eventDataJson,
                EventDefinitionId = value.EventDefinitionId,
                ModifyBy = "system.trigger",
                ModifyDate = DateTime.UtcNow,
                Notes = string.Empty,
                ReferenceName = value.ReferenceName,
                ReferenceValue = value.ReferenceValue,
                Status = SimpleTools.SimpleHooks.Models.Instance.Enums.EventInstanceStatus.InQueue
            });

            // write json manually
            JsonObject resultJson = new()
            {
                ["eventDefinitionId"] = result.EventDefinitionId,
                ["businessId"] = result.BusinessId,
                ["status"] = (int)result.Status,
                ["referenceName"] = result.ReferenceName,
                ["referenceValue"] = result.ReferenceValue,
                ["createDate"] = result.CreateDate,
                ["eventData"] = JsonNode.Parse(result.EventData)
            };

            return Ok(resultJson);
        }
    }
}
