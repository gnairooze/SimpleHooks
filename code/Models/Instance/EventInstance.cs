using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using static Models.Instance.Enums;

namespace Models.Instance
{
    public class EventInstance:ModelBase
    {
        public EventInstance()
        {
            this.ListenerInstances = new List<ListenerInstance>();
        }
        public long EventDefinitionId { get; set; }
        public JObject EventData { get; set; }
        public string ReferenceName { get; set; }
        public string ReferenceValue { get; set; }
        public EventInstanceStatus Status { get; set; }
        public byte[] TimeStamp { get; set; }
        public List<ListenerInstance> ListenerInstances { get; }
        public override string ToString()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }
    }
}
