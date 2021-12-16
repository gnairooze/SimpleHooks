using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Models.Instance;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SimpleHooks.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TriggerEventController : ControllerBase
    {
        private readonly Business.InstanceManager _Manager;
        private readonly Helper.ConfigurationHelper _Config;
        public TriggerEventController(IConfiguration config)
        {
            _Config = new Helper.ConfigurationHelper(config);

            _Manager = new(
                new Log.SQL.Logger()
                {
                    MinLogType = (Log.Interface.LogModel.LogTypes)Enum.Parse(typeof(Log.Interface.LogModel.LogTypes), _Config.Logger_MinLogLevel, true),
                    ConnectionString = _Config.ConnectionString_Log,
                    FunctionName = _Config.Logger_Function
                },
                new Repo.SQL.SqlConnectionRepo() { ConnectionString = _Config.ConnectionString_SimpleHooks },
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
        public void Post([FromBody] Models.EventViewModel value)
        {
            _Manager.Add(new EventInstance() {
                Active = true,
                BusinessId = Guid.NewGuid(),
                CreateBy = "system.trigger",
                CreateDate = DateTime.UtcNow,
                EventData = value.EventData,
                EventDefinitionId = value.EventDefinitionId,
                ModifyBy = "system.trigger",
                ModifyDate = DateTime.UtcNow,
                Notes = string.Empty,
                ReferenceName = value.ReferenceName,
                ReferenceValue = value.ReferenceValue,
                Status = Enums.EventInstanceStatus.InQueue
            });
        }
    }
}
