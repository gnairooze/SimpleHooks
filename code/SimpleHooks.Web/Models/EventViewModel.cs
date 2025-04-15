namespace SimpleHooks.Web.Models
{
    public class EventViewModel(long eventDefinitionId, string eventData, string referenceName, string referenceValue)
    {
        public long EventDefinitionId { get; } = eventDefinitionId;
        public string EventData { get; } = eventData;
        public string ReferenceName { get; } = referenceName;
        public string ReferenceValue { get; } = referenceValue;
    }
}
