using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleTools.SimpleHooks.ListenerInterfaces
{
    /// <summary>
    /// Interface for listener plugin implementations.
    /// Plugins are loaded dynamically and executed when events are triggered.
    /// </summary>
    public interface IListener
    {
        /// <summary>
        /// The target URL for the listener webhook call.
        /// Set by ListenerPluginManager during initialization.
        /// </summary>
        string Url { get; set; }

        /// <summary>
        /// Timeout duration in minutes for the HTTP request.
        /// Set by ListenerPluginManager during initialization.
        /// </summary>
        int Timeout { get; set; }

        /// <summary>
        /// List of HTTP headers to include in the request.
        /// Set by ListenerPluginManager during initialization.
        /// </summary>
        List<string> Headers { get; set; }

        /// <summary>
        /// Execute the listener with the provided event data.
        /// </summary>
        /// <param name="eventData">The event data to send to the listener endpoint</param>
        /// <param name="typeOptions">Plugin-specific configuration options (typically JSON)</param>
        /// <returns>Result of the listener execution including success status, message, and logs</returns>
        Task<ListenerResult> ExecuteAsync(string eventData, string typeOptions);
    }
}

