namespace SimpleTools.SimpleHooks.Models.Definition
{
    public class AppOption: ModelBase
    {
        public string Category { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }

        public override string ToString()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }
    }
}
