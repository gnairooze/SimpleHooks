using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleHooks.Web.Models
{
    public class EventViewModel
    {
        public long EventDefinitionId { get; set; }
        public string EventData { get; set; }
        public string ReferenceName { get; set; }
        public string ReferenceValue { get; set; }
    }
}
