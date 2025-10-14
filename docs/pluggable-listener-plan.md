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
    ListenerPlugin = _listnerPluginManager.CreatePluginInstance(pass Url as string, TypeId as id parameter, timeoutInMinutes as int, headers as List<string>) // in the CreatePluginInstance method, the plugin will be created and initialized with the configuration
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

the benefits of passing type options as string parameter to the method instead of persisting it directly in the listener definition is that the type options may contain sensitive information such as client id and client secret. So it is better to use it in the method execution only instead of storing it in the listener definition. So the listener definition object in the memory will not contain sensitive information.

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
3. **Runtime Configuration**: Used with `Type_Options` from ListenerDefinition to read environment variables

**Environment Variable Naming Convention:**

- Plugin configuration is read from environment variables named: `{ListenerType.Name}_CONFIG`
- Example: For ListenerType with Name="TypeA", the system reads `TYPEA_CONFIG` environment variable
- This allows each plugin type to have independent configuration without conflicts

**Server Startup Process:**

1. Query all ListenerType records from database
2. For each ListenerType:
   - Load plugin DLL from `Path`
   - Create plugin instance
   - Read configuration from `{Name}_CONFIG` environment variable
   - Initialize plugin with headers, timeout, URL, and type-specific options
   - Cache initialized plugin for runtime use

#### Task 1.3: Update Database Schema

**Files**:

- `code/SQL/operation-db/` - Add migration scripts
- Update repository classes in `code/Repo.SQL/`

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

#### Task 1.4: Listener Plugin Manager

**File**: `code/Business/ListenerPluginManager.cs`

**Responsibilities:**

- Load listener plugins from specified paths at startup
- Maintain dictionary of plugin instances by type
- Handle plugin lifecycle and error handling
- Validate plugin compatibility
- Initialize listener plugins during server startup with ListenerType configuration

**Key Methods:**

- `LoadListenerPlugins()` - Load all listener plugins at startup
- `GetListenerPlugin(string type)` - Retrieve plugin instance by type
- `ValidateListenerPlugin(Assembly assembly)` - Ensure plugin implements interface
- `InitializePluginsAtStartup()` - Initialize plugins with ListenerType data during server startup

**Server Startup Initialization:**

During server startup, the ListenerPluginManager will:

1. **Load ListenerType Configuration**: Query database for all ListenerType records
2. **Create Plugin Instances**: For each ListenerType, load the corresponding plugin DLL and create instances
3. **Initialize Plugin Properties**: Set up each plugin with:
   - Headers from environment variables
   - Timeout configuration
   - URL endpoints
   - Type-specific options from environment variables (using Type_Options as environment variable name)
4. **Cache Plugin Instances**: Store initialized plugins in memory for runtime execution
5. **Validate Plugin Compatibility**: Ensure all plugins implement IListener interface correctly

**Implementation Details:**

```csharp
public class ListenerPluginManager
{
    private readonly Dictionary<int, IListener> _pluginInstances = new();
    private readonly Dictionary<int, ListenerType> _listenerTypes = new();
    
    public async Task InitializePluginsAtStartupAsync()
    {
        // 1. Load ListenerType configuration from database
        var listenerTypes = await _listenerTypeRepository.GetAllAsync();
        
        foreach (var listenerType in listenerTypes)
        {
            try
            {
                // 2. Load plugin assembly
                var assembly = Assembly.LoadFrom(listenerType.Path);
                var pluginType = assembly.GetTypes()
                    .FirstOrDefault(t => t.GetInterface(nameof(IListener)) != null);
                
                if (pluginType != null)
                {
                    // 3. Create plugin instance
                    var pluginInstance = Activator.CreateInstance(pluginType) as IListener;
                    
                    // 4. Initialize plugin with environment-based configuration
                    await InitializePluginConfiguration(pluginInstance, listenerType);
                    
                    // 5. Cache plugin instance
                    _pluginInstances[listenerType.Id] = pluginInstance;
                    _listenerTypes[listenerType.Id] = listenerType;
                    
                    _logger.LogInformation($"Successfully initialized plugin: {listenerType.Name}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to initialize plugin: {listenerType.Name}");
            }
        }
    }
    
    private async Task InitializePluginConfiguration(IListener plugin, ListenerType listenerType)
    {
        // Read configuration from environment variables
        var environmentVariableName = listenerType.Name + "_CONFIG";
        var configJson = Environment.GetEnvironmentVariable(environmentVariableName);
        
        if (!string.IsNullOrEmpty(configJson))
        {
            // Parse and apply configuration to plugin
            // This allows each plugin type to have its own configuration
            // stored in environment variables like ANONYMOUS_CONFIG, TYPEA_CONFIG, etc.
        }
    }
}
```

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

#### Task 2.2: TypeA Plugin (Bearer Token Authentication)

**Project**: `SimpleHooks.ListenerPlugins.TypeA`
**Location**: `code/listener-plugins/SimpleHooks.Listener      Plugins.TypeA`

**Purpose**:

- OAuth2 client credentials flow
- Bearer token authentication
- Token caching and refresh logic

**Options JSON Structure:**

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

### Phase 3: Core System Integration

#### Task 3.1: Refactor InstanceManager

**File**: `code/Business/InstanceManager.cs`

**Changes:**

- Inject `ListenerPluginManager` dependency
- Replace `ExecuteListener` method logic
- Use plugin-based execution instead of direct HTTP calls
- Maintain existing logging and status update logic

**New ExecuteListener Logic:**

1. Get plugin instance from ListenerPluginManager by listener type
2. Read environment variable for options (if specified)
3. Call plugin's ExecuteAsync method
4. Process ListenerResult and update status
5. Handle plugin execution errors

#### Task 3.2: Update Dependency Injection
**Files**: 
- `code/SimpleHooks.Server/Program.cs`
- `code/SimpleHooks.Web/Startup.cs`

**Changes:**
- Register ListenerPluginManager as singleton
- Initialize plugins at startup with ListenerType configuration
- Handle plugin loading errors gracefully
- Set up environment variable-based configuration

**Server Startup Implementation:**

**For SimpleHooks.Server/Program.cs:**
```csharp
public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        // Register services
        builder.Services.AddSingleton<IListenerTypeRepository, ListenerTypeRepository>();
        builder.Services.AddSingleton<ListenerPluginManager>();
        
        var app = builder.Build();
        
        // Initialize listener plugins at startup
        var pluginManager = app.Services.GetRequiredService<ListenerPluginManager>();
        await InitializeListenerPluginsAsync(pluginManager);
        
        app.Run();
    }
    
    private static async Task InitializeListenerPluginsAsync(ListenerPluginManager pluginManager)
    {
        try
        {
            await pluginManager.InitializePluginsAtStartupAsync();
            Console.WriteLine("Listener plugins initialized successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error initializing listener plugins: {ex.Message}");
            // Log error but continue startup - system can work with fallback mechanisms
        }
    }
}
```

**For SimpleHooks.Web/Startup.cs:**
```csharp
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Register listener plugin services
        services.AddSingleton<IListenerTypeRepository, ListenerTypeRepository>();
        services.AddSingleton<ListenerPluginManager>();
        
        // Other service registrations...
    }
    
    public async void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        // Initialize listener plugins during application startup
        var pluginManager = app.ApplicationServices.GetRequiredService<ListenerPluginManager>();
        await InitializeListenerPluginsAsync(pluginManager);
        
        // Other middleware configuration...
    }
    
    private async Task InitializeListenerPluginsAsync(ListenerPluginManager pluginManager)
    {
        try
        {
            await pluginManager.InitializePluginsAtStartupAsync();
            // Log successful initialization
        }
        catch (Exception ex)
        {
            // Log error but allow application to continue
            // System can fall back to default behavior if plugins fail to load
        }
    }
}
```

**Environment Variable Configuration:**

The system will read plugin-specific configuration from environment variables during startup:

- `ANONYMOUS_CONFIG`: Configuration for Anonymous plugin (if needed)
- `TYPEA_CONFIG`: Configuration for TypeA plugin (OAuth2 settings)
- `PLUGIN_DIRECTORY`: Base directory for plugin DLLs (default: "listener-plugins")
- `PLUGIN_TIMEOUT`: Plugin loading timeout in seconds (default: 30)

**Example Environment Variables:**
```bash
# TypeA Plugin OAuth2 Configuration
TYPEA_CONFIG={"identityProviderUrl":"https://auth.example.com/token","clientId":"client_id","clientSecret":"client_secret","scope":"api.read api.write"}

# Plugin Directory Configuration
PLUGIN_DIRECTORY=F:\SimpleHooks\plugins
PLUGIN_TIMEOUT=60
```

**Startup Sequence:**

1. **Service Registration**: Register ListenerPluginManager and dependencies
2. **Database Connection**: Ensure database connectivity for ListenerType queries
3. **Plugin Discovery**: Load ListenerType records from database
4. **Plugin Loading**: Load and instantiate plugins from configured paths
5. **Configuration Application**: Apply environment variable configuration to plugins
6. **Validation**: Validate plugin compatibility and readiness
7. **Caching**: Store initialized plugins for runtime use
8. **Error Handling**: Log any initialization failures and continue with available plugins

#### Task 3.3: Configuration Management
**Files**:
- `code/SimpleHooks.Server/appsettings.json`
- Environment variable handling

**New Configuration:**
- Plugin directory path
- Plugin loading timeout
- Default plugin type for new listeners

### Phase 4: Testing Infrastructure

#### Task 4.1: Unit Tests for Plugin Interface
**Project**: `TestSimpleHooks.Plugins`
**Location**: `code/TestSimpleHooks/Plugins/`

**Test Classes:**
- `ListenerExecuterPluginManagerTests.cs`
- `AnonymousPluginTests.cs`
- `TypeAPluginTests.cs`
- `ListenerResultTests.cs`

#### Task 4.2: Integration Tests
**File**: `code/TestSimpleHooks/TestPluginIntegration.cs`

**Test Scenarios:**
- End-to-end listener execution with plugins
- Plugin loading and error handling
- Environment variable configuration
- Database integration with new fields

#### Task 4.3: Mock Implementations
**Files**:
- `code/TestSimpleHooks/Mocks/MockPlugin.cs`
- `code/TestSimpleHooks/Mocks/MockHttpClient.cs`
- `code/TestSimpleHooks/Mocks/MockIdentityProvider.cs`

### Phase 5: Migration and Deployment

#### Task 5.1: Data Migration
**File**: `code/SQL/operation-db/migrate-to-plugins.sql`

**Migration Steps:**
1. Create ListenerDefinitionType table with proper constraints
2. Populate ListenerDefinitionType with initial data (Anonymous and TypeA)
3. Add Type_Id and Type_Options columns to ListenerDefinition table
4. Set default values for existing records (Type_Id = 1 for Anonymous)
5. Add foreign key constraint between ListenerDefinition and ListenerDefinitionType
6. Update existing data repository methods to handle new schema

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

### ListenerExecuterPluginManager Tests

#### Test Class: `ListenerExecuterPluginManagerTests.cs`

**Test Methods:**

1. **LoadPlugins_ValidDirectory_LoadsAllPlugins**
   - Setup: Create test plugins in temp directory
   - Action: Call LoadPlugins()
   - Assert: All valid plugins loaded, invalid ones ignored

2. **GetPlugin_ExistingType_ReturnsPlugin**
   - Setup: Load test plugins
   - Action: Call GetPlugin("TestType")
   - Assert: Returns correct plugin instance

3. **GetPlugin_NonExistentType_ThrowsException**
   - Setup: Load test plugins
   - Action: Call GetPlugin("InvalidType")
   - Assert: Throws appropriate exception

4. **LoadPlugins_InvalidAssembly_LogsErrorAndContinues**
   - Setup: Place invalid DLL in plugins directory
   - Action: Call LoadPlugins()
   - Assert: Error logged, other plugins still loaded

5. **LoadPlugins_MissingInterface_SkipsPlugin**
   - Setup: Plugin without IListenerExecute interface
   - Action: Call LoadPlugins()
   - Assert: Plugin skipped, warning logged

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
- Tasks 1.1 - 1.4: Plugin interface, model updates, listener executer plugin manager

### Week 3: Plugin Implementations  
- Tasks 2.1 - 2.2: Anonymous and TypeA plugins

### Week 4: System Integration
- Tasks 3.1 - 3.3: InstanceManager refactoring, DI updates

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
