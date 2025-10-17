# Pluggable Listener Implementation Plan

## Overview

This document outlines the detailed implementation plan for adding pluggable listener executors to SimpleHooks. The feature will allow multiple authentication methods using a plugin-based approach, starting with bearer token authentication.

## Current State Analysis

### Existing Implementation

- Listener execution logic is currently in `Business/InstanceManager.cs` method `ExecuteListener`
- Uses `HttpClient.Simple` for HTTP calls
- Direct HTTP calls with headers from `ListenerDefinition.Headers`
- Fixed timeout and retry logic
- Status tracking through `ListenerInstanceStatus` enum

### Current Models

- `ListenerDefinition`: Contains URL, Headers, Timeout, TrialCount, RetrialDelay
- `ListenerInstance`: Runtime instance with status tracking
- `HttpResult`: Response structure with Body, Headers, HttpCode
- `LogModel`: Logging structure

## Plugin Initialization Flow

This section describes the complete flow of how listener plugins are initialized during server startup:

### 1. Server Startup
   - DI container registers all services including:
     - `ListenerPluginManager` (singleton)
     - `ListenerTypeDataRepo` (repository for ListenerType)
     - `DefinitionManager` (with all dependencies)

### 2. Load Definitions Process
   - `DefinitionManager.LoadDefinitions()` is called
   - **Step 1**: Load `ListenerTypes` from database → stored in `DefinitionManager.ListenerTypes` list
   - **Step 2**: Load `ListenerDefinitions` from database → stored in `DefinitionManager.ListenerDefinitions` list
   - **Step 3**: Call `SetListenerPlugins()` method

### 3. SetListenerPlugins Method
   For each `ListenerDefinition`:
   - Find matching `ListenerType` by `TypeId`
   - Get plugin DLL path from `ListenerType.Path`
   - Read environment variable value from `ListenerDefinition.TypeOptions` property
   - Call `ListenerPluginManager.CreatePluginInstance()`:
     - **path**: DLL path from `ListenerType.Path`
     - **url**: `ListenerDefinition.Url`
     - **timeout**: `ListenerDefinition.Timeout`
     - **headers**: `ListenerDefinition.Headers`
     - **typeOptionsValue**: Value from environment variable
   - Store returned plugin instance in `ListenerDefinition.ListenerPlugin`

### 4. CreatePluginInstance Method
   - Resolve full path (handle relative paths)
   - Check plugin type cache (performance optimization)
   - If not cached:
     - Load assembly from DLL path using `Assembly.LoadFrom()`
     - Find type implementing `IListener` interface
     - Cache the type for future use
   - Create plugin instance using `Activator.CreateInstance()`
   - Initialize plugin via reflection (set properties or call Initialize method)
   - Return plugin instance

### 5. Plugin Ready for Execution
   - All `ListenerDefinition` objects now have `ListenerPlugin` property populated
   - When `InstanceManager.ExecuteListener()` is called:
     - Get plugin from `ListenerDefinition.ListenerPlugin`
     - Call `plugin.ExecuteAsync(eventData, typeOptionsValue)`
     - Process returned `ListenerResult`

**Key Benefits:**
- Plugins loaded once at startup (not per execution)
- Plugin type cache improves performance
- Flexible plugin initialization (properties or Initialize method)
- Environment-based configuration keeps secrets out of database
- Each listener definition gets its own plugin instance

### Architecture Diagram

```
Server Startup
      |
      v
+---------------------+
|  DI Registration    |
|---------------------|
| - ListenerPlugin    |
|   Manager           |
| - ListenerType      |
|   DataRepo          |
| - DefinitionManager |
+---------------------+
      |
      v
+----------------------------------+
| DefinitionManager.LoadDefinitions()
+----------------------------------+
      |
      |---> Load ListenerTypes[]
      |     (from database)
      |
      |---> Load ListenerDefinitions[]
      |     (from database)
      |
      v
+----------------------------------+
| SetListenerPlugins()             |
+----------------------------------+
      |
      | For each ListenerDefinition:
      |
      v
+----------------------------------+
| 1. Find ListenerType by TypeId   |
| 2. Get path from ListenerType    |
| 3. Read env var from TypeOptions |
+----------------------------------+
      |
      v
+----------------------------------+
| ListenerPluginManager            |
| .CreatePluginInstance()          |
|----------------------------------|
| - Resolve path                   |
| - Load assembly (with cache)     |
| - Find IListener implementation  |
| - Create instance                |
| - Initialize properties          |
+----------------------------------+
      |
      v
+----------------------------------+
| ListenerDefinition.ListenerPlugin|
| = plugin instance                |
+----------------------------------+
      |
      v
+----------------------------------+
| Ready for Execution              |
|----------------------------------|
| InstanceManager.ExecuteListener()|
| → plugin.ExecuteAsync()          |
+----------------------------------+
```

## Implementation Tasks

### Phase 1: Core Infrastructure

#### Task 1.1: Create Listener Plugin Interface Project

**Project**: `SimpleHooks.ListenerInterfaces`
**Namespace**: `SimpleTools.SimpleHooks.ListenerInterfaces`

**Files to Create:**

- `IListener.cs` - Main plugin interface
- `ListenerResult.cs` - Plugin execution result model
- `SimpleHooks.ListenerInterfaces.csproj` - Project file

**Interface Definition:**

```csharp
public interface IListener
{
    // Properties to be set during plugin initialization
    string Url { get; set; }
    int Timeout { get; set; }
    List<string> Headers { get; set; }
    string TypeOptionsValue { get; set; }
    
    // Method to execute the listener
    Task<ListenerResult> ExecuteAsync(
        string eventData,
        string typeOptions
    );
}

public class ListenerResult
{
    public bool Succeeded { get; set; }
    public string Message { get; set; }
    public SortedList<int, LogModel> Logs { get; set; } = new SortedList<int, LogModel>();
}
```

**Property Descriptions:**

- `Url`: The target URL for the listener webhook call
- `Timeout`: Timeout duration in minutes for the HTTP request
- `Headers`: List of HTTP headers to include in the request
- `TypeOptionsValue`: Plugin-specific configuration value read from environment variable

These properties are set by `ListenerPluginManager.InitializePlugin()` after the plugin instance is created, allowing the plugin to have access to all necessary configuration during execution.

**Dependencies:**

- Reference to `Log.Interface` for `LogModel`

#### Task 1.2: Extend ListenerDefinition Model

**File**: `code/Models/Definition/ListenerDefinition.cs`

**New Properties to Add:**

- `int TypeId { get; set; }` - Foreign key to ListenerType.Id (default: 1 for Anonymous)
- `string TypeOptions { get; set; }` - Environment variable name for auth options
- `IListener ListenerPlugin { get; set; }` - Listener plugin instance that will be used to execute the listener

**Type_Options Property Usage:**

The `TypeOptions` property in ListenerDefinition stores the name of an environment variable that contains plugin-specific configuration. During listener execution:

1. **Environment Variable Reading**: The system reads the environment variable named in `TypeOptions`
2. **Configuration Parsing**: The environment variable value (typically JSON) is passed to the plugin and parsed in the plugin according to the plugin's implementation.
3. **Plugin Initialization**: Each plugin receives its specific configuration through the `options` parameter in `ExecuteAsync`

**Example Usage:**

```csharp
// ListenerDefinition record
var listenerDef = new ListenerDefinition
{
    TypeId = 2, // TypeA plugin
    TypeOptions = "OAUTH_CLIENT_CREDENTIALS", // Environment variable name
    ListenerPlugin = _listenerPluginManager.CreatePluginInstance(pass Url as string, TypeId as id parameter, timeoutInMinutes as int, headers as List<string>) // in the CreatePluginInstance method, the plugin will be created and initialized with the configuration
    Url = "https://api.example.com/v1/data",
    Headers = new List<string> { "Content-Type: application/json" },
    Timeout = 60,
    TrialCount = 3,
    RetrialDelay = 1,
    CreateBy = "admin",
    CreateDate = DateTime.Now,
    ModifyBy = "admin",
    ModifyDate = DateTime.Now,
    Notes = "TypeA plugin configuration",
    // ... other properties
};

// Environment variable content
Environment.SetEnvironmentVariable("OAUTH_CLIENT_CREDENTIALS", 
    "{\"clientId\":\"abc123\",\"clientSecret\":\"secret\",\"tokenUrl\":\"https://auth.example.com/token\"}");

// During execution, plugin receives the event data as string parameter and type options as string parameter
await plugin.ExecuteAsync(eventData, typeOptions);
```

in the plugin, the type options will be parsed and used to authenticate the request. the plugin will be responsible for the authentication logic.

the benefits of passing type options as string parameter to the method instead of persisting it directly in the listener definition is that the type options may contain sensitive information such as client id and client secret. So it is better to use it in the method execution only instead of storing it in the listener definition. So the listener definition object in the memory will not contain sensitive information for long time.

**New Model to Create:**

**File**: `code/Models/Definition/ListenerType.cs`

- `int Id { get; set; }` - Primary key
- `string Name { get; set; }` - Plugin type name (e.g., "Anonymous", "TypeA")
- `string Path { get; set; }` - Plugin DLL path (e.g., "listener-plugins/TypeA/TypeAListener.dll")
- `string CreateBy { get; set; }` - Created by user
- `DateTime CreateDate { get; set; }` - Creation date
- `string ModifyBy { get; set; }` - Modified by user
- `DateTime ModifyDate { get; set; }` - Modification date
- `string Notes { get; set; }` - Additional notes

**ListenerType Integration with Server Startup:**

The ListenerType model serves as the configuration source for plugin initialization during server startup. Each ListenerType record contains:

1. **Plugin Identity**: `Name` field identifies the plugin type (used for environment variable naming)
2. **Plugin Location**: `Path` field specifies the DLL location for dynamic loading

**Environment Variable Naming Convention:**

- Plugin configuration is read from environment variables named: `SimpleHooks_Listener_{ListenerType.Name}_CONFIG`. the full environment variable name is saved in ListenerDefinition.TypeOptions property.
- Example: For ListenerType with Name="TypeA", the system reads `SimpleHooks_Listener_TYPEA_CONFIG` environment variable
- This allows each plugin type to have independent configuration without conflicts

**Server Startup Process:**

1. `DefinitionManager.LoadDefinitions()` is called during server startup
2. First, load `ListenerTypes` list from database via repository
3. Then, load `ListenerDefinitions` list from database via repository
4. Call `SetListenerPlugins()` method which:
   - Loops over each `ListenerDefinition`
   - Finds the related `ListenerType` by `TypeId`
   - Gets the plugin DLL path from `ListenerType.Path`
   - Reads the environment variable specified in `ListenerDefinition.TypeOptions` to get `typeOptionsValue`
   - Calls `ListenerPluginManager.CreatePluginInstance()` passing:
     - `path`: Plugin DLL path from ListenerType
     - `url`: ListenerDefinition.Url
     - `timeout`: ListenerDefinition.Timeout
     - `headers`: ListenerDefinition.Headers
     - `typeOptionsValue`: Value from environment variable
   - Stores the created plugin instance in `ListenerDefinition.ListenerPlugin` property

#### Task 1.3: Update Database Schema

**Files**:

- `code/SQL/operation-db/` - Add migration scripts
- `code/Repo.SQL/ListenerTypeDataRepo.cs` - Create new repository class for ListenerType
- Update repository classes in `code/Repo.SQL/`

**Repository Implementation:**

Create `ListenerTypeDataRepo.cs` following the same pattern as `ListenerDefinitionDataRepo.cs`, implementing `IDataRepository<Models.Definition.ListenerType>` interface with CRUD operations.

**Schema Changes:**

1. **Create ListenerType table:**

```sql
USE [SimpleHooks]
GO

CREATE TABLE [dbo].[ListenerType](
	[Id] [int] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Path] [nvarchar](1000) NOT NULL,
	[CreateBy] [nvarchar](50) NOT NULL,
	[CreateDate] [datetime2](7) NOT NULL,
	[ModifyBy] [nvarchar](50) NOT NULL,
	[ModifyDate] [datetime2](7) NOT NULL,
	[Notes] [nvarchar](1000) NOT NULL,
 CONSTRAINT [PK_ListenerType] PRIMARY KEY CLUSTERED
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_ListenerType_Name] ON [dbo].[ListenerType]
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

ALTER TABLE [dbo].[ListenerType] ADD  CONSTRAINT [DF_ListenerType_CreateDate]  DEFAULT (getdate()) FOR [CreateDate]
GO
ALTER TABLE [dbo].[ListenerType] ADD  CONSTRAINT [DF_ListenerType_ModifyDate]  DEFAULT (getdate()) FOR [ModifyDate]
GO
ALTER TABLE [dbo].[ListenerType] ADD  CONSTRAINT [DF_ListenerType_Notes]  DEFAULT ('') FOR [Notes]
GO
```

2. **Populate ListenerType with initial data:**

```sql
INSERT ListenerType(Id, [Name], [Path], CreateBy, ModifyBy, Notes) 
VALUES (1, 'Anonymous', 'listener-plugins/Anonymous/Anonymous.dll', 'system.admin', 'system.admin', '');

INSERT ListenerType(Id, [Name], [Path], CreateBy, ModifyBy, Notes) 
VALUES (2, 'TypeA', 'listener-plugins/TypeA/TypeAListener.dll', 'system.admin', 'system.admin', 'include Bearer token in header');
```

3. **Alter ListenerDefinition table:**

```sql
ALTER TABLE ListenerDefinition 
ADD Type_Id INT NOT NULL DEFAULT (1),
    Type_Options NVARCHAR(200) NOT NULL DEFAULT ('');

ALTER TABLE ListenerDefinition
ADD CONSTRAINT FK_ListenerDefinition_ListenerType
FOREIGN KEY (Type_Id) REFERENCES ListenerType(Id);
```

#### Task 1.4: Update DefinitionManager

**File**: `code/Business/DefinitionManager.cs`

**Summary:**

The `DefinitionManager` class is updated to handle the initialization of listener plugins during the definition loading process. Key changes include:
- Adding `ListenerType` repository and list management
- Injecting `ListenerPluginManager` dependency
- Loading `ListenerTypes` before `ListenerDefinitions`
- Creating plugin instances for each listener definition after data is loaded
- Storing plugin instances in `ListenerDefinition.ListenerPlugin` property for later execution

**Changes Required:**

1. **Add ListenerPluginManager Dependency:**
   - Inject `ListenerPluginManager` in constructor
   - Store as private readonly field `_listenerPluginManager`

2. **Add ListenerType Repository:**
   - Add repository: `IDataRepository<Models.Definition.ListenerType> _listenerTypeRepo`
   - Add property: `public List<Models.Definition.ListenerType> ListenerTypes { get; }`
   - Initialize empty list in constructor

3. **Modify LoadDefinitions Method:**
   - Load `ListenerTypes` list BEFORE loading `ListenerDefinitions`
   - Load `ListenerDefinitions` list as currently done
   - Call new `SetListenerPlugins()` method AFTER `ListenerDefinitions` are loaded

4. **Add SetListenerPlugins Method:**

```csharp
private void SetListenerPlugins(IDbConnection conn)
{
    // Loop over each ListenerDefinition
    foreach (var listenerDef in this.ListenerDefinitions)
    {
        // Find the related ListenerType
        var listenerType = this.ListenerTypes.FirstOrDefault(lt => lt.Id == listenerDef.TypeId);
        
        if (listenerType == null)
        {
            // Log error if ListenerType not found
            var log = GetLogModelError($"ListenerType with Id {listenerDef.TypeId} not found for ListenerDefinition {listenerDef.Id}");
            this._logger.Add(log);
            continue;
        }
        
        // Get the TypeOptions value from environment variable
        string typeOptionsValue = string.Empty;
        if (!string.IsNullOrWhiteSpace(listenerDef.TypeOptions))
        {
            typeOptionsValue = Environment.GetEnvironmentVariable(listenerDef.TypeOptions) ?? string.Empty;
        }
        
        // Create plugin instance
        listenerDef.ListenerPlugin = _listenerPluginManager.CreatePluginInstance(
            path: listenerType.Path,
            url: listenerDef.Url,
            timeout: listenerDef.Timeout,
            headers: listenerDef.Headers,
            typeOptionsValue: typeOptionsValue
        );
    }
}
```

**Updated LoadDefinitions Method Flow:**

```csharp
public bool LoadDefinitions()
{
    bool succeeded = true;
    var log = this.GetLogModelMethodStart(MethodBase.GetCurrentMethod()?.Name, string.Empty, string.Empty);
    this._logger.Add(log);
    var conn = this._connectionRepo.CreateConnection();

    try
    {
        this._connectionRepo.OpenConnection(conn);

        // Load AppOptions
        this.AppOptions.Clear();
        this.AppOptions.AddRange(this._appOptionRepo.Read(null, conn));
        
        // Load EventDefinitions
        this.EventDefinitions.Clear();
        this.EventDefinitions.AddRange(this._eventDefRepo.Read(null, conn));
        
        // Load ListenerTypes FIRST
        this.ListenerTypes.Clear();
        this.ListenerTypes.AddRange(this._listenerTypeRepo.Read(null, conn));
        
        // Load ListenerDefinitions AFTER ListenerTypes
        this.ListenerDefinitions.Clear();
        this.ListenerDefinitions.AddRange(this._listenerDefRepo.Read(null, conn));
        
        // Load relations
        this.EventDefinitionListenerDefinitionRelations.Clear();
        this.EventDefinitionListenerDefinitionRelations.AddRange(this._eventDefListenerDefRepo.Read(null, conn));
        
        // Set listener plugins for each ListenerDefinition
        this.SetListenerPlugins(conn);

        // Trigger event for definitions loaded
        this.DefitionsLoaded?.Invoke(this, EventArgs.Empty);
    }
    catch (Exception ex)
    {
        log = GetLogModelException(log, ex);
        this._logger.Add(log);
        succeeded = false;
    }
    finally
    {
        this._connectionRepo.CloseConnection(conn);
    }

    this._connectionRepo.DisposeConnection(conn);
    this._logger.Add(this.GetLogModelMethodEnd(log));
    return succeeded;
}
```

**Constructor Update:**

```csharp
public DefinitionManager(
    ILog logger,
    IDataRepository<Models.Definition.EventDefinition> eventDefRepo,
    IDataRepository<Models.Definition.ListenerDefinition> listenerDefRepo,
    IDataRepository<Models.Definition.EventDefinitionListenerDefinition> eventDefListenerDefRepo,
    IDataRepository<Models.Definition.AppOption> appOptionRepo,
    IDataRepository<Models.Definition.ListenerType> listenerTypeRepo,
    IConnectionRepository connectionRepo,
    ListenerPluginManager listenerPluginManager)
{
    this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
    this._appOptionRepo = appOptionRepo ?? throw new ArgumentNullException(nameof(appOptionRepo));
    this._eventDefRepo = eventDefRepo ?? throw new ArgumentNullException(nameof(eventDefRepo));
    this._listenerDefRepo = listenerDefRepo ?? throw new ArgumentNullException(nameof(listenerDefRepo));
    this._listenerTypeRepo = listenerTypeRepo ?? throw new ArgumentNullException(nameof(listenerTypeRepo));
    this._eventDefListenerDefRepo = eventDefListenerDefRepo ?? throw new ArgumentNullException(nameof(eventDefListenerDefRepo));
    this._connectionRepo = connectionRepo ?? throw new ArgumentNullException(nameof(connectionRepo));
    this._listenerPluginManager = listenerPluginManager ?? throw new ArgumentNullException(nameof(listenerPluginManager));

    this.AppOptions = new List<Models.Definition.AppOption>();
    this.EventDefinitions = new List<Models.Definition.EventDefinition>();
    this.ListenerTypes = new List<Models.Definition.ListenerType>();
    this.ListenerDefinitions = new List<Models.Definition.ListenerDefinition>();
    this.EventDefinitionListenerDefinitionRelations = new List<Models.Definition.EventDefinitionListenerDefinition>();
}
```

#### Task 1.5: Listener Plugin Manager

**File**: `code/Business/ListenerPluginManager.cs`

**Namespace**: `SimpleTools.SimpleHooks.Business`

**Responsibilities:**

- Handle plugin lifecycle and error handling
- Dynamically load plugin DLLs using reflection
- Create plugin instances with `Activator.CreateInstance()`
- Initialize plugin properties directly (no reflection needed for properties)
- Cache plugin types for performance
- Validate plugin implementations

**Project References Required:**

Add the following project reference to `code/Business/Business.csproj`:
```xml
<ProjectReference Include="..\SimpleHooks.ListenerInterfaces\SimpleHooks.ListenerInterfaces.csproj" />
```

**Dependencies:**

- `SimpleTools.SimpleHooks.ListenerInterfaces` - For `IListener` interface
- `SimpleTools.SimpleHooks.Log.Interface` - For logging (already referenced)
- `System.Reflection` - For dynamic assembly loading (built-in)

**Class Structure:**

```csharp
using SimpleTools.SimpleHooks.ListenerInterfaces;
using SimpleTools.SimpleHooks.Log.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SimpleTools.SimpleHooks.Business
{
    public class ListenerPluginManager : LogBase
    {
        private readonly ILog _logger;
        private readonly Dictionary<string, Type> _pluginTypeCache;

        public ListenerPluginManager(ILog logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _pluginTypeCache = new Dictionary<string, Type>();
        }

        public IListener CreatePluginInstance(string path, string url, int timeout, List<string> headers, string typeOptionsValue)
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
                pluginInstance.Headers = headers;
                pluginInstance.TypeOptionsValue = typeOptionsValue;

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
```

**Key Implementation Details:**

1. **Plugin Type Caching:**
   - Uses `Dictionary<string, Type>` to cache loaded plugin types
   - Avoids repeated assembly loading for the same plugin path
   - Improves performance when creating multiple instances

2. **Path Resolution:**
   - Handles both absolute and relative paths
   - Resolves relative paths from `AppDomain.CurrentDomain.BaseDirectory`
   - Validates file existence before loading

3. **Assembly Loading:**
   - Uses `Assembly.LoadFrom()` to dynamically load plugin DLL
   - Searches for types implementing `IListener` interface
   - Validates that exactly one concrete implementation exists

4. **Instance Creation:**
   - Uses `Activator.CreateInstance()` to create plugin instance
   - Validates that created instance implements `IListener`
   - Throws descriptive exceptions on failure

5. **Error Handling:**
   - Comprehensive validation of inputs
   - Descriptive exception messages
   - Logging at method entry, exit, and on exceptions
   - Re-throws exceptions to let caller handle appropriately

6. **Logging:**
   - Inherits from `LogBase` for consistent logging
   - Logs method start with key parameters
   - Logs method end on success
   - Logs exceptions with full details

**Usage Example:**

```csharp
// In DefinitionManager.SetListenerPlugins()
var plugin = _listenerPluginManager.CreatePluginInstance(
    path: "listener-plugins/TypeA/TypeAListener.dll",
    url: "https://api.example.com/webhook",
    timeout: 60,
    headers: new List<string> { "Content-Type: application/json" },
    typeOptionsValue: "{\"clientId\":\"abc\",\"clientSecret\":\"xyz\"}"
);

// Plugin is now ready to be used
listenerDef.ListenerPlugin = plugin;
```

**Troubleshooting Common Issues:**

1. **FileNotFoundException: Plugin DLL not found**
   - Verify the path in `ListenerType.Path` is correct
   - Check if using relative path, it's relative to `AppDomain.CurrentDomain.BaseDirectory`
   - Ensure plugin DLL is deployed with the application

2. **InvalidOperationException: No IListener implementation found**
   - Verify plugin project references `SimpleHooks.ListenerInterfaces`
   - Ensure plugin class implements `IListener` interface
   - Check plugin class is public and not abstract

3. **ReflectionTypeLoadException: Unable to load assembly**
   - Verify all plugin dependencies are in the same directory as the plugin DLL
   - Check .NET runtime versions match between host and plugin
   - Review loader exceptions for specific missing dependencies

4. **Plugin properties not initialized**
   - This should not occur since properties are part of the `IListener` interface
   - All plugins implementing `IListener` must have these properties
   - Properties are set directly after instance creation

5. **Environment variable not found**
   - Verify environment variable name in `ListenerDefinition.TypeOptions` is correct
   - Check environment variable is set before server starts
   - Remember: Variable returns empty string if not found (doesn't throw)

**Performance Considerations:**

- Plugin types are cached after first load (Dictionary lookup vs Assembly load)
- Each `ListenerDefinition` gets its own plugin instance (not shared)
- Plugin instances are created once at startup (not per execution)
- Consider lazy loading if you have many plugins and want faster startup

### Phase 2: Listener Plugin Implementations

#### Task 2.1: Anonymous Plugin

**Project**: `SimpleHooks.ListenerPlugins.Anonymous`
**Location**: `code/listener-plugins/SimpleHooks.ListenerPlugins.Anonymous/`

**Purpose**:

- Replicate current listener execution logic
- No authentication, direct HTTP calls
- Maintain backward compatibility

**Implementation:**

- Move existing HTTP logic from `InstanceManager.ExecuteListener`. instead of direct log, add logs in ListenerResult.Logs.
- Use `HttpClient.Simple` for HTTP calls
- Handle timeouts and error scenarios. do not throw exceptions, add logs in ListenerResult.Logs and return ListenerResult with Succeeded = false and appropriate message.

**Class Structure:**

```csharp
using SimpleTools.SimpleHooks.ListenerInterfaces;
using SimpleTools.SimpleHooks.HttpClient.Interface;
using SimpleTools.SimpleHooks.Log.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleTools.SimpleHooks.ListenerPlugins.Anonymous
{
    public class AnonymousListener : IListener
    {
        // IListener properties - set by ListenerPluginManager
        public string Url { get; set; }
        public int Timeout { get; set; }
        public List<string> Headers { get; set; }
        public string TypeOptionsValue { get; set; }
        
        private readonly IHttpClient _httpClient;
        
        public AnonymousListener()
        {
            // Initialize with default HTTP client
            _httpClient = new SimpleTools.SimpleHooks.HttpClient.Simple.SimpleClient();
        }
        
        public async Task<ListenerResult> ExecuteAsync(string eventData, string typeOptions)
        {
            var result = new ListenerResult();
            int logCounter = 0;
            
            try
            {
                // Log start
                result.Logs.Add(logCounter++, new LogModel
                {
                    Message = $"Anonymous plugin executing call to {Url}",
                    CreateDate = DateTime.Now
                });
                
                // Make HTTP call using properties
                var httpResult = _httpClient.Post(Url, Headers, eventData, Timeout);
                
                // Check result
                if (httpResult.HttpCode >= 200 && httpResult.HttpCode < 300)
                {
                    result.Succeeded = true;
                    result.Message = $"HTTP call succeeded with status code {httpResult.HttpCode}";
                    
                    result.Logs.Add(logCounter++, new LogModel
                    {
                        Message = $"Success: {httpResult.HttpCode} - {httpResult.Body}",
                        CreateDate = DateTime.Now
                    });
                }
                else
                {
                    result.Succeeded = false;
                    result.Message = $"HTTP call failed with status code {httpResult.HttpCode}";
                    
                    result.Logs.Add(logCounter++, new LogModel
                    {
                        Message = $"Failed: {httpResult.HttpCode} - {httpResult.Body}",
                        CreateDate = DateTime.Now
                    });
                }
            }
            catch (Exception ex)
            {
                result.Succeeded = false;
                result.Message = $"Exception during execution: {ex.Message}";
                
                result.Logs.Add(logCounter++, new LogModel
                {
                    Message = $"Exception: {ex.Message}\n{ex.StackTrace}",
                    CreateDate = DateTime.Now
                });
            }
            
            return await Task.FromResult(result);
        }
    }
}
```

**Key Points:**

- Implements all four required properties from `IListener` interface
- Properties are set by `ListenerPluginManager` after instantiation
- Uses the properties (`Url`, `Timeout`, `Headers`) in `ExecuteAsync` execution
- `TypeOptionsValue` not used in Anonymous plugin but must be implemented
- No constructor parameters needed - default constructor only

#### Task 2.2: TypeA Plugin (Bearer Token Authentication)

**Project**: `SimpleHooks.ListenerPlugins.TypeA`
**Location**: `code/listener-plugins/SimpleHooks.ListenerPlugins.TypeA`

**Purpose**:

- OAuth2 client credentials flow
- Bearer token authentication
- Token caching and refresh logic

**Options JSON Structure:**

The `TypeOptionsValue` property will contain JSON configuration:

```json
{
  "identityProviderUrl": "https://auth.example.com/token",
  "clientId": "client_id",
  "clientSecret": "client_secret",
  "scope": "api.read api.write"
}
```

**Implementation Steps:**

1. Parse options JSON to extract auth parameters
2. Implement OAuth2 client credentials flow
3. Cache tokens with expiration handling
4. Add Authorization header to listener requests
5. Handle token refresh on 401 responses

**Class Structure:**

```csharp
using SimpleTools.SimpleHooks.ListenerInterfaces;
using SimpleTools.SimpleHooks.HttpClient.Interface;
using SimpleTools.SimpleHooks.Log.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json;

namespace SimpleTools.SimpleHooks.ListenerPlugins.TypeA
{
    public class TypeAListener : IListener
    {
        // IListener properties - set by ListenerPluginManager
        public string Url { get; set; }
        public int Timeout { get; set; }
        public List<string> Headers { get; set; }
        public string TypeOptionsValue { get; set; }
        
        private readonly IHttpClient _httpClient;
        private string _cachedToken;
        private DateTime _tokenExpiration;
        
        public TypeAListener()
        {
            _httpClient = new SimpleTools.SimpleHooks.HttpClient.Simple.SimpleClient();
        }
        
        public async Task<ListenerResult> ExecuteAsync(string eventData, string typeOptions)
        {
            var result = new ListenerResult();
            int logCounter = 0;
            
            try
            {
                // Log start
                result.Logs.Add(logCounter++, new LogModel
                {
                    Message = $"TypeA plugin executing call to {Url}",
                    CreateDate = DateTime.Now
                });
                
                // Parse TypeOptionsValue to get auth configuration
                var authConfig = JsonSerializer.Deserialize<TypeAOptions>(TypeOptionsValue);
                
                if (authConfig == null || string.IsNullOrWhiteSpace(authConfig.IdentityProviderUrl))
                {
                    result.Succeeded = false;
                    result.Message = "Invalid or missing authentication configuration";
                    result.Logs.Add(logCounter++, new LogModel
                    {
                        Message = "TypeOptionsValue is missing or invalid",
                        CreateDate = DateTime.Now
                    });
                    return result;
                }
                
                // Get bearer token (cached or fresh)
                string token = await GetBearerToken(authConfig, result, ref logCounter);
                
                if (string.IsNullOrWhiteSpace(token))
                {
                    result.Succeeded = false;
                    result.Message = "Failed to obtain bearer token";
                    return result;
                }
                
                // Add Authorization header
                var headersWithAuth = new List<string>(Headers ?? new List<string>());
                headersWithAuth.Add($"Authorization: Bearer {token}");
                
                // Make HTTP call
                var httpResult = _httpClient.Post(Url, headersWithAuth, eventData, Timeout);
                
                // Check result
                if (httpResult.HttpCode >= 200 && httpResult.HttpCode < 300)
                {
                    result.Succeeded = true;
                    result.Message = $"HTTP call succeeded with status code {httpResult.HttpCode}";
                    
                    result.Logs.Add(logCounter++, new LogModel
                    {
                        Message = $"Success: {httpResult.HttpCode} - {httpResult.Body}",
                        CreateDate = DateTime.Now
                    });
                }
                else if (httpResult.HttpCode == 401)
                {
                    // Token might be expired, try refreshing
                    result.Logs.Add(logCounter++, new LogModel
                    {
                        Message = "Received 401, attempting token refresh",
                        CreateDate = DateTime.Now
                    });
                    
                    _cachedToken = null; // Invalidate cache
                    token = await GetBearerToken(authConfig, result, ref logCounter);
                    
                    // Retry with new token
                    headersWithAuth = new List<string>(Headers ?? new List<string>());
                    headersWithAuth.Add($"Authorization: Bearer {token}");
                    httpResult = _httpClient.Post(Url, headersWithAuth, eventData, Timeout);
                    
                    if (httpResult.HttpCode >= 200 && httpResult.HttpCode < 300)
                    {
                        result.Succeeded = true;
                        result.Message = $"HTTP call succeeded after token refresh: {httpResult.HttpCode}";
                    }
                    else
                    {
                        result.Succeeded = false;
                        result.Message = $"HTTP call failed after token refresh: {httpResult.HttpCode}";
                    }
                    
                    result.Logs.Add(logCounter++, new LogModel
                    {
                        Message = $"Retry result: {httpResult.HttpCode}",
                        CreateDate = DateTime.Now
                    });
                }
                else
                {
                    result.Succeeded = false;
                    result.Message = $"HTTP call failed with status code {httpResult.HttpCode}";
                    
                    result.Logs.Add(logCounter++, new LogModel
                    {
                        Message = $"Failed: {httpResult.HttpCode} - {httpResult.Body}",
                        CreateDate = DateTime.Now
                    });
                }
            }
            catch (Exception ex)
            {
                result.Succeeded = false;
                result.Message = $"Exception during execution: {ex.Message}";
                
                result.Logs.Add(logCounter++, new LogModel
                {
                    Message = $"Exception: {ex.Message}\n{ex.StackTrace}",
                    CreateDate = DateTime.Now
                });
            }
            
            return result;
        }
        
        private async Task<string> GetBearerToken(TypeAOptions config, ListenerResult result, ref int logCounter)
        {
            // Check cache
            if (!string.IsNullOrWhiteSpace(_cachedToken) && DateTime.Now < _tokenExpiration)
            {
                result.Logs.Add(logCounter++, new LogModel
                {
                    Message = "Using cached bearer token",
                    CreateDate = DateTime.Now
                });
                return _cachedToken;
            }
            
            try
            {
                // Request new token
                result.Logs.Add(logCounter++, new LogModel
                {
                    Message = $"Requesting bearer token from {config.IdentityProviderUrl}",
                    CreateDate = DateTime.Now
                });
                
                var tokenRequestBody = $"grant_type=client_credentials&client_id={config.ClientId}&client_secret={config.ClientSecret}&scope={config.Scope}";
                var tokenHeaders = new List<string> { "Content-Type: application/x-www-form-urlencoded" };
                
                var tokenResult = _httpClient.Post(config.IdentityProviderUrl, tokenHeaders, tokenRequestBody, 5);
                
                if (tokenResult.HttpCode >= 200 && tokenResult.HttpCode < 300)
                {
                    var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(tokenResult.Body);
                    _cachedToken = tokenResponse.AccessToken;
                    _tokenExpiration = DateTime.Now.AddSeconds(tokenResponse.ExpiresIn - 60); // Refresh 60 seconds before expiry
                    
                    result.Logs.Add(logCounter++, new LogModel
                    {
                        Message = "Bearer token obtained successfully",
                        CreateDate = DateTime.Now
                    });
                    
                    return _cachedToken;
                }
                else
                {
                    result.Logs.Add(logCounter++, new LogModel
                    {
                        Message = $"Failed to obtain token: {tokenResult.HttpCode} - {tokenResult.Body}",
                        CreateDate = DateTime.Now
                    });
                    return null;
                }
            }
            catch (Exception ex)
            {
                result.Logs.Add(logCounter++, new LogModel
                {
                    Message = $"Exception obtaining token: {ex.Message}",
                    CreateDate = DateTime.Now
                });
                return null;
            }
        }
    }
    
    // Configuration model
    public class TypeAOptions
    {
        public string IdentityProviderUrl { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Scope { get; set; }
    }
    
    // Token response model
    public class TokenResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("access_token")]
        public string AccessToken { get; set; }
        
        [System.Text.Json.Serialization.JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
    }
}
```

**Key Points:**

- Implements all four required properties from `IListener` interface
- Properties are set by `ListenerPluginManager` after instantiation
- `TypeOptionsValue` contains the OAuth configuration JSON
- Parses `TypeOptionsValue` in `ExecuteAsync` to get authentication parameters
- Implements token caching with expiration handling
- Handles 401 responses with automatic token refresh
- Uses existing `Headers` property and adds Authorization header for each request

### Phase 3: Core System Integration

#### Task 3.1: Refactor InstanceManager

**File**: `code/Business/InstanceManager.cs`

**Changes:**

- Inject `ListenerPluginManager` dependency
- Replace `ExecuteListener` method logic
- Use plugin-based execution instead of direct HTTP calls
- Maintain existing logging and status update logic

**New ExecuteListener Logic:**

1. Get plugin instance by querying the DefinitionManager.ListenerDefinitions and get the ListenerPlugin property
2. Call plugin's ExecuteAsync method. pass the eventData as string parameter and type options as string parameter.
3. Process ListenerResult and update status. if the ListenerResult.Succeeded is false, update the listener instance status to Failed. if the ListenerResult.Succeeded is true, update the listener instance status to Succeeded.
4. Handle plugin execution errors. if the plugin execution throws an exception, update the listener instance status to Failed.
5. Loop over ListenerResult.Logs and save logs.

**Startup Sequence:**

1. **Service Registration**: Register `ListenerPluginManager` and dependencies in DI container
2. **Database Connection**: Ensure database connectivity for ListenerType queries
3. **Load Definitions**: Call `DefinitionManager.LoadDefinitions()` which loads types and creates plugin instances

#### Task 3.2: Dependency Injection Registration

**Files**: 
- `code/SimpleHooks.Server/Program.cs` or `Startup.cs`
- `code/SimpleHooks.Web/Program.cs` or `Startup.cs`

**Service Registration:**

```csharp
// Register ListenerPluginManager as singleton (to maintain plugin cache)
services.AddSingleton<ListenerPluginManager>();

// Register ListenerType repository
services.AddSingleton<IDataRepository<Models.Definition.ListenerType>, Repo.SQL.ListenerTypeDataRepo>();

// Update DefinitionManager registration to include new dependencies
services.AddSingleton<DefinitionManager>(serviceProvider =>
{
    var logger = serviceProvider.GetRequiredService<ILog>();
    var eventDefRepo = serviceProvider.GetRequiredService<IDataRepository<Models.Definition.EventDefinition>>();
    var listenerDefRepo = serviceProvider.GetRequiredService<IDataRepository<Models.Definition.ListenerDefinition>>();
    var eventDefListenerDefRepo = serviceProvider.GetRequiredService<IDataRepository<Models.Definition.EventDefinitionListenerDefinition>>();
    var appOptionRepo = serviceProvider.GetRequiredService<IDataRepository<Models.Definition.AppOption>>();
    var listenerTypeRepo = serviceProvider.GetRequiredService<IDataRepository<Models.Definition.ListenerType>>();
    var connectionRepo = serviceProvider.GetRequiredService<IConnectionRepository>();
    var listenerPluginManager = serviceProvider.GetRequiredService<ListenerPluginManager>();
    
    return new DefinitionManager(
        logger,
        eventDefRepo,
        listenerDefRepo,
        eventDefListenerDefRepo,
        appOptionRepo,
        listenerTypeRepo,
        connectionRepo,
        listenerPluginManager
    );
});
```

**Initialization on Startup:**

```csharp
// In Program.cs or Startup.cs Configure method
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    // ... other configuration ...
    
    // Load definitions (this will initialize all plugins)
    var definitionManager = app.ApplicationServices.GetRequiredService<DefinitionManager>();
    bool success = definitionManager.LoadDefinitions();
    
    if (!success)
    {
        throw new Exception("Failed to load definitions and initialize plugins");
    }
    
    // ... rest of configuration ...
}
```

#### Task 3.3: Configuration Management
**Files**:
- `code/SimpleHooks.Server/appsettings.json`
- Environment variable handling

#### Task 5.2: Plugin Deployment Structure
**Directory Structure:**
```
SimpleHooks.Server/
├── plugins/
│   ├── Anonymous/
│   │   ├── SimpleHooks.Plugins.Anonymous.dll
│   │   └── dependencies...
│   └── TypeA/
│       ├── SimpleHooks.Plugins.TypeA.dll
│       └── dependencies...
```

#### Task 5.3: Documentation Updates
**Files**:
- Update API documentation
- Plugin development guide
- Configuration examples
- Migration guide

## Detailed Unit Test Plans

### Anonymous Plugin Tests

#### Test Class: `AnonymousPluginTests.cs`

**Test Methods:**

1. **ExecuteAsync_ValidRequest_ReturnsSuccess**
   - Setup: Mock HTTP client with 200 response
   - Action: Call ExecuteAsync with valid parameters
   - Assert: ListenerResult.Succeeded = true

2. **ExecuteAsync_HttpError_ReturnsFailure**
   - Setup: Mock HTTP client with 500 response
   - Action: Call ExecuteAsync
   - Assert: ListenerResult.Succeeded = false, error logged

3. **ExecuteAsync_Timeout_ReturnsFailure**
   - Setup: Mock HTTP client with timeout
   - Action: Call ExecuteAsync with short timeout
   - Assert: ListenerResult.Succeeded = false, timeout logged

4. **ExecuteAsync_InvalidUrl_ReturnsFailure**
   - Setup: Invalid URL in parameters
   - Action: Call ExecuteAsync
   - Assert: ListenerResult.Succeeded = false, error logged

5. **ExecuteAsync_EmptyHeaders_AddsDefaultContentType**
   - Setup: Empty headers list
   - Action: Call ExecuteAsync
   - Assert: Content-Type header added automatically

### TypeA Plugin Tests

#### Test Class: `TypeAPluginTests.cs`

**Test Methods:**

1. **ExecuteAsync_ValidAuth_GetsTokenAndExecutes**
   - Setup: Mock identity provider returning valid token
   - Action: Call ExecuteAsync with auth options
   - Assert: Token obtained, Authorization header added, request succeeds

2. **ExecuteAsync_InvalidCredentials_ReturnsFailure**
   - Setup: Mock identity provider returning 401
   - Action: Call ExecuteAsync with invalid credentials
   - Assert: ListenerResult.Succeeded = false, auth error logged

3. **ExecuteAsync_TokenExpired_RefreshesToken**
   - Setup: Cached expired token, mock refresh endpoint
   - Action: Call ExecuteAsync
   - Assert: Token refreshed, new token used

4. **ExecuteAsync_MalformedOptions_ReturnsFailure**
   - Setup: Invalid JSON in options parameter
   - Action: Call ExecuteAsync
   - Assert: ListenerResult.Succeeded = false, parsing error logged

5. **ExecuteAsync_MissingRequiredOptions_ReturnsFailure**
   - Setup: Options missing clientId or clientSecret
   - Action: Call ExecuteAsync
   - Assert: ListenerResult.Succeeded = false, validation error logged

6. **ExecuteAsync_IdentityProviderDown_ReturnsFailure**
   - Setup: Mock identity provider unavailable
   - Action: Call ExecuteAsync
   - Assert: ListenerResult.Succeeded = false, connection error logged

### Integration Tests

#### Test Class: `TestPluginIntegration.cs`

**Test Methods:**

1. **EndToEnd_AnonymousPlugin_ProcessesEvent**
   - Setup: Complete system with Anonymous plugin
   - Action: Process event instance
   - Assert: Listener executed successfully, status updated

2. **EndToEnd_TypeAPlugin_ProcessesEventWithAuth**
   - Setup: Complete system with TypeA plugin and auth
   - Action: Process event instance
   - Assert: Token obtained, listener executed, status updated

3. **PluginLoading_AtStartup_LoadsAllPlugins**
   - Setup: Server startup simulation
   - Action: Initialize plugin system
   - Assert: All plugins loaded and available

4. **Configuration_EnvironmentVariables_LoadedCorrectly**
   - Setup: Set environment variables for auth options
   - Action: Execute TypeA plugin
   - Assert: Options read from environment correctly

5. **ErrorHandling_PluginException_SystemContinues**
   - Setup: Plugin that throws exception
   - Action: Execute listener
   - Assert: Exception handled, system remains stable

### Mock Implementations

#### MockPlugin.cs
```csharp
public class MockPlugin : IListenerExecute
{
    public bool ShouldSucceed { get; set; } = true;
    public string MockMessage { get; set; } = "Mock execution";
    public SortedList<int, LogModel> MockLogs { get; set; } = new SortedList<int, LogModel>();

    public Task<ListenerResult> ExecuteAsync(string eventData, List<string> listenerHeaders, int timeoutMinutes, string options)
    {
        return Task.FromResult(new ListenerResult
        {
            Succeeded = ShouldSucceed,
            Message = MockMessage,
            Logs = MockLogs
        });
    }
}
```

#### MockHttpClient.cs
```csharp
public class MockHttpClient : IHttpClient
{
    public HttpResult MockResult { get; set; }
    public bool ShouldThrow { get; set; } = false;
    public Exception ExceptionToThrow { get; set; }

    public HttpResult Post(string url, List<string> headers, string body, int timeout)
    {
        if (ShouldThrow)
            throw ExceptionToThrow ?? new HttpRequestException("Mock exception");
        
        return MockResult ?? new HttpResult { HttpCode = 200, Body = "Mock response" };
    }
}
```

## Implementation Timeline

### Week 1-2: Core Infrastructure
- Tasks 1.1 - 1.5: Plugin interface, model updates, database schema, DefinitionManager updates, ListenerPluginManager

### Week 3: Plugin Implementations  
- Tasks 2.1 - 2.2: Anonymous and TypeA plugins

### Week 4: System Integration
- Tasks 3.1 - 3.3: InstanceManager refactoring, DI registration, configuration management

### Week 5: Testing
- Tasks 4.1 - 4.3: Unit tests, integration tests, mocks

### Week 6: Migration and Deployment
- Tasks 5.1 - 5.3: Data migration, deployment structure, documentation

## Risk Mitigation

### Technical Risks
1. **Plugin Loading Failures**: Implement robust error handling and fallback mechanisms
2. **Performance Impact**: Cache plugin instances, minimize reflection usage
3. **Security Concerns**: Validate plugin assemblies, sandbox execution if needed
4. **Backward Compatibility**: Ensure existing listeners continue working with Anonymous plugin

### Operational Risks
1. **Deployment Complexity**: Provide clear migration scripts and documentation
2. **Configuration Errors**: Implement validation for plugin configurations
3. **Monitoring**: Add logging and metrics for plugin execution

## Success Criteria

1. **Functional Requirements**
   - All existing listeners work without modification (Anonymous plugin)
   - TypeA plugin successfully authenticates and executes requests
   - Plugin system loads and manages multiple plugin types
   - Environment-based configuration works correctly

2. **Non-Functional Requirements**
   - No performance degradation compared to current implementation
   - 100% unit test coverage for new components
   - Comprehensive integration test coverage
   - Clear documentation and migration guides

3. **Quality Requirements**
   - All existing tests continue to pass
   - New code follows established patterns and conventions
   - Proper error handling and logging throughout
   - Security best practices for credential handling

## Future Enhancements

1. **Additional Authentication Types**
   - API Key authentication
   - OAuth2 authorization code flow
   - Custom authentication schemes

2. **Plugin Management Features**
   - Hot-reload of plugins
   - Plugin versioning and updates
   - Plugin health monitoring

3. **Advanced Configuration**
   - Plugin-specific configuration files
   - Dynamic plugin discovery
   - Plugin dependency management
