using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Text.Json;
using Models.Instance;

namespace SimpleHooks.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TriggerEventController : ControllerBase
    {
        private readonly Business.InstanceManager _manager;

        public TriggerEventController(IConfiguration config)
        {
            var config1 = new Helper.ConfigurationHelper(config);

            _manager = new(
                new Log.SQL.Logger()
                {
                    MinLogType = (Log.Interface.LogModel.LogTypes)Enum.Parse(typeof(Log.Interface.LogModel.LogTypes), config1.LoggerMinLogLevel, true),
                    ConnectionString = config1.ConnectionStringLog,
                    FunctionName = config1.LoggerFunction
                },
                new Repo.SQL.SqlConnectionRepo() { ConnectionString = config1.ConnectionStringSimpleHooks },
                new Repo.SQL.EventInstanceDataRepo(),
                new Repo.SQL.ListenerInstanceDataRepo(),
                new HttpClient.Simple.SimpleClient(),
                new Repo.SQL.EventDefinitionDataRepo(),
                new Repo.SQL.ListenerDefinitionDataRepo(),
                new Repo.SQL.EventIistenerDefinitionDataRepo(),
                new Repo.SQL.AppOptionDataRepo());
        }
        
        // POST api/<TriggerEventController>
        [HttpPost]
        public OkObjectResult Post([FromBody] Models.EventViewModel value)
        {
            var result = _manager.Add(new EventInstance() {
                Active = true,
                BusinessId = Guid.NewGuid(),
                CreateBy = "system.trigger",
                CreateDate = DateTime.UtcNow,
                EventData = value.EventData != null ? JsonSerializer.Serialize(value.EventData) : "{}",
                EventDefinitionId = value.EventDefinitionId,
                ModifyBy = "system.trigger",
                ModifyDate = DateTime.UtcNow,
                Notes = string.Empty,
                ReferenceName = value.ReferenceName,
                ReferenceValue = value.ReferenceValue,
                Status = Enums.EventInstanceStatus.InQueue
            });

            return Ok(result);
        }
    }
}
