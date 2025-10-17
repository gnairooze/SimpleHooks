namespace SimpleTools.SimpleHooks.Models.Definition
{
    /// <summary>
    /// Represents a type of listener plugin with its configuration.
    /// Contains the plugin identity, location (DLL path), and metadata.
    /// </summary>
    public class ListenerType : ModelBase
    {
        /// <summary>
        /// Plugin type name (e.g., "Anonymous", "TypeA")
        /// Used for identification and environment variable naming conventions.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Plugin DLL path (e.g., "listener-plugins/TypeA/TypeAListener.dll")
        /// Can be relative (resolved from application base directory) or absolute.
        /// </summary>
        public string Path { get; set; }

        public override string ToString()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }
    }
}

