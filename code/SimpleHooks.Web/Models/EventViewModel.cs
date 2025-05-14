using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SimpleHooks.Web.Models
{
    public class EventViewModel(
        long eventDefinitionId,
        string referenceName,
        string referenceValue,
        Dictionary<string, JsonElement> eventData)
    {
        public long EventDefinitionId { get; } = eventDefinitionId;
        public string ReferenceName { get; } = referenceName;
        public string ReferenceValue { get; } = referenceValue;

        [JsonExtensionData]
        public Dictionary<string, JsonElement> EventData { get; } = eventData;
    }
}
