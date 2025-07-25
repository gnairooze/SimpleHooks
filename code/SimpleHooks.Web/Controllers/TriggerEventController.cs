﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Text.Json;

namespace SimpleHooks.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TriggerEventController : ControllerBase
    {
        private readonly Business.InstanceManager _manager;

        public TriggerEventController(
            Log.Interface.ILog logger,
            Interfaces.IConnectionRepository connectionRepo,
            Interfaces.IDataRepository<global::Models.Instance.EventInstance> eventInstanceRepo,
            Interfaces.IDataRepository<global::Models.Instance.ListenerInstance> listenerInstanceRepo,
            HttpClient.Interface.IHttpClient httpClient,
            Interfaces.IDataRepository<global::Models.Definition.EventDefinition> eventDefRepo,
            Interfaces.IDataRepository<global::Models.Definition.ListenerDefinition> listenerDefRepo,
            Interfaces.IDataRepository<global::Models.Definition.EventDefinitionListenerDefinition> eventDefListenerDefRepo,
            Interfaces.IDataRepository<global::Models.Definition.AppOption> appOptionRepo)
        {
            _manager = new Business.InstanceManager(
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
        public OkObjectResult Post([FromBody] Models.EventViewModel value)
        {
            // Extract the EventData node from the request
            var eventDataJson = value.EventData.TryGetValue("EventData", out var eventDataNode) 
                ? eventDataNode.GetRawText() 
                : "{}";

            var result = _manager.Add(new global::Models.Instance.EventInstance() {
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
                Status = global::Models.Instance.Enums.EventInstanceStatus.InQueue
            });

            return Ok(result);
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
