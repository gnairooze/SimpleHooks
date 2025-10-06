using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleTools.SimpleHooks.Assist.Models
{
    public class TriggerEventRequest
    {
        public long EventDefinitionId { get; set; }
        public string ReferenceName { get; set; } = string.Empty;
        public string ReferenceValue { get; set; } = string.Empty;
        public object? EventData { get; set; }
    }
}
