using System;
using System.Collections.Generic;
using System.Text;

namespace Models.Definition
{
    public class ListenerDefinition:ModelBase
    {
        protected List<string> _Headers = new List<string>();
        public string Name { get; set; }
        public string URL { get; set; }
        public List<String> Headers 
        {
            get
            {
                return this._Headers;
            }
        }
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
