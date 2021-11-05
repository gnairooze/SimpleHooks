using System;
using System.Collections.Generic;
using System.Text;

namespace Models.Definition
{
    public class EventDefinitionListenerDefinition:ModelBase
    {
        public long EventDefinitiontId { get; set; } 
        public long ListenerDefinitionId { get; set; }
        public override string ToString()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }
    }
}
