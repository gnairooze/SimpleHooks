namespace SimpleTools.SimpleHooks.Models.Definition
{
    public class EventDefinition: ModelBase
    {
        public string Name { get; set; }
        public override string ToString()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }
    }
}
