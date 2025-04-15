namespace Models.Definition
{
    public class EventDefinitionListenerDefinition:ModelBase
    {
        public long EventDefinitionId { get; set; } 
        public long ListenerDefinitionId { get; set; }
        public override string ToString()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }
    }
}
