using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SimpleHooks.Web.Models
{
    public class EventViewModel
    {
        [JsonConstructor]
        public EventViewModel(long eventDefinitionId, string referenceName, string referenceValue)
        {
            EventDefinitionId = eventDefinitionId;
            ReferenceName = referenceName;
            ReferenceValue = referenceValue;
        }

        public long EventDefinitionId { get; }
        public string ReferenceName { get; }
        public string ReferenceValue { get; }

        [JsonExtensionData]
        public Dictionary<string, JsonElement> EventData { get; set; } = new();
    }
}
