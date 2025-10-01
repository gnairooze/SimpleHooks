using System;
using System.Collections.Generic;
using System.Text;
using static SimpleTools.SimpleHooks.Models.Instance.Enums;

namespace SimpleTools.SimpleHooks.Models.Instance
{
    public class ListenerInstance : ModelBase
    {
        public long EventInstanceId { get; set; }
        public long ListenerDefinitionId { get; set; }
        public ListenerInstanceStatus Status { get; set; }
        public int RemainingTrialCount { get; set; }
        public DateTime NextRun { get; set; }
        public byte[] TimeStamp { get; set; }
        public Definition.ListenerDefinition Definition {get;set;}
        public override string ToString()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }
    }
}
