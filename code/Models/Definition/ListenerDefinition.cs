using System.Collections.Generic;

namespace Models.Definition
{
    public class ListenerDefinition:ModelBase
    {
        public string Name { get; set; }
        public string Url { get; set; }

        public List<string> Headers { get; } = new List<string>();

        /// <summary>
        /// timeout in minutes
        /// </summary>
        public int Timeout { get; set; }
        public int TrialCount { get; set; }
        /// <summary>
        /// delay in minutes
        /// </summary>
        public int RetrialDelay { get; set; }
        public override string ToString()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }
    }
}
