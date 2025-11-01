using SimpleTools.SimpleHooks.ListenerInterfaces;
using SimpleTools.SimpleHooks.Log.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SimpleTools.SimpleHooks.Business
{
    /// <summary>
    /// Manages the lifecycle of listener plugins including loading, caching, and initialization.
    /// Responsible for dynamically loading plugin assemblies and creating configured instances.
    /// </summary>
    public class ListenerPluginManager : LogBase
    {
        private readonly ILog _logger;
        private readonly Dictionary<string, Type> _pluginTypeCache;

        public ListenerPluginManager(ILog logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _pluginTypeCache = new Dictionary<string, Type>();
        }

        /// <summary>
        /// Creates and initializes a listener plugin instance.
        /// </summary>
        /// <param name="path">Path to the plugin DLL (relative or absolute)</param>
        /// <param name="url">Target URL for the listener</param>
        /// <param name="timeout">Timeout in minutes</param>
        /// <param name="headers">HTTP headers to include</param>
        /// <param name="typeOptionsValue">Plugin-specific configuration value</param>
        /// <returns>Initialized plugin instance</returns>
        public IListener CreatePluginInstance(string path, string url, int timeout, List<string> headers)
        {
            var log = GetLogModelMethodStart(MethodBase.GetCurrentMethod()?.Name,
                $"Path: {path}, URL: {url}, Timeout: {timeout}", string.Empty);
            _logger.Add(log);

            try
            {
                // Validate input parameters
                if (string.IsNullOrWhiteSpace(path))
                {
                    throw new ArgumentException("Plugin path cannot be null or empty", nameof(path));
                }

                // Resolve full path (handle relative paths)
                string fullPath = Path.IsPathRooted(path)
                    ? path
                    : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);

                // Check if file exists
                if (!File.Exists(fullPath))
                {
                    throw new FileNotFoundException($"Plugin DLL not found at path: {fullPath}", fullPath);
                }

                // Load plugin type (use cache if available)
                Type pluginType = GetPluginType(fullPath);

                // Create instance of the plugin
                IListener pluginInstance = Activator.CreateInstance(pluginType) as IListener;

                if (pluginInstance == null)
                {
                    throw new InvalidOperationException(
                        $"Failed to create instance of plugin type {pluginType.FullName}. " +
                        $"Type does not implement IListener interface.");
                }

                // Set properties directly
                pluginInstance.Url = url;
                pluginInstance.Timeout = timeout;
                pluginInstance.Headers = headers ?? new List<string>();

                log = GetLogModelMethodEnd(log);
                _logger.Add(log);

                return pluginInstance;
            }
            catch (Exception ex)
            {
                log = GetLogModelException(log, ex);
                _logger.Add(log);
                throw; // Re-throw to let caller handle the error
            }
        }

        /// <summary>
        /// Gets the plugin type from cache or loads it from the assembly.
        /// </summary>
        /// <param name="fullPath">Full path to the plugin DLL</param>
        /// <returns>Type implementing IListener interface</returns>
        private Type GetPluginType(string fullPath)
        {
            // Check cache first
            if (_pluginTypeCache.TryGetValue(fullPath, out Type cachedType))
            {
                return cachedType;
            }

            // Load assembly from path
            Assembly pluginAssembly = Assembly.LoadFrom(fullPath);

            // Find type that implements IListener interface
            Type pluginType = pluginAssembly.GetTypes()
                .FirstOrDefault(t => typeof(IListener).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

            if (pluginType == null)
            {
                throw new InvalidOperationException(
                    $"No type implementing IListener interface found in assembly: {fullPath}");
            }

            // Cache the type for future use
            _pluginTypeCache[fullPath] = pluginType;

            return pluginType;
        }
    }
}

