using System;
using System.Collections.Generic;
using System.Text;
using static Models.Instance.Enums;

namespace Models.Instance
{
    public class EventInstanceStatusBrief
    {
        public long Id { get; set; }
        public Guid BusinessId { get; set; }
        public EventInstanceStatus Status { get; set; }
        
        public override string ToString()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }
    }
}
