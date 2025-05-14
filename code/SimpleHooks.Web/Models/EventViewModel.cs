using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SimpleHooks.Web.Models
{
    public class EventViewModel
    {
        public long EventDefinitionId { get; set; }
        public string ReferenceName { get; set; }
        public string ReferenceValue { get; set; }
        
        [JsonExtensionData]
        public Dictionary<string, JsonElement> EventData { get; set; }
    }
}
