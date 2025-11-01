using SimpleTools.SimpleHooks.Log.Interface;
using System.Collections.Generic;

namespace SimpleTools.SimpleHooks.ListenerInterfaces
{
    /// <summary>
    /// Result of a listener plugin execution.
    /// Contains success status, message, and detailed execution logs.
    /// </summary>
    public class ListenerResult
    {
        /// <summary>
        /// Indicates whether the listener execution succeeded.
        /// </summary>
        public bool Succeeded { get; set; }

        /// <summary>
        /// Human-readable message describing the execution result.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Detailed logs of the execution process.
        /// Key is the log sequence number, value is the log entry.
        /// </summary>
        public SortedList<int, LogModel> Logs { get; set; } = new SortedList<int, LogModel>();
    }
}

