using System.Collections.Generic;
using SimpleTools.SimpleHooks.ListenerInterfaces;

namespace SimpleTools.SimpleHooks.Models.Definition
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

        /// <summary>
        /// Foreign key to ListenerType.Id
        /// Identifies which plugin type to use for this listener (default: 1 for Anonymous)
        /// </summary>
        public int TypeId { get; set; } = 1;

        /// <summary>
        /// Environment variable name for plugin-specific authentication/configuration options.
        /// The value from this environment variable is passed to the plugin during execution.
        /// Example: "SimpleHooks_Listener_TypeA_CONFIG"
        /// </summary>
        public string TypeOptions { get; set; } = string.Empty;

        /// <summary>
        /// Listener plugin instance that will be used to execute the listener.
        /// This is set by DefinitionManager during startup via ListenerPluginManager.
        /// Not persisted to database - runtime only.
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public IListener ListenerPlugin { get; set; }

        public override string ToString()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }
    }
}
