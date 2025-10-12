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

#### Task 1.1: Create Plugin Interface Project
**Project**: `SimpleHooks.ListenerExecuterInterfaces`
**Namespace**: `SimpleTools.SimpleHooks.SimpleExecuterInterfaces`

**Files to Create:**
- `IListenerExecute.cs` - Main plugin interface
- `ListenerResult.cs` - Plugin execution result model
- `SimpleHooks.ListenerExecuterInterfaces.csproj` - Project file

**Interface Definition:**
```csharp
public interface IListenerExecute
{
    Task<ListenerResult> ExecuteAsync(
        string eventData,
        List<string> listenerHeaders,
        int timeoutMinutes,
        string options
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
- `int Type_Id { get; set; }` - Foreign key to ListenerDefinitionType.Id (default: 1 for Anonymous)
- `string Type_Options { get; set; }` - Environment variable name for auth options

**New Model to Create:**
**File**: `code/Models/Definition/ListenerDefinitionType.cs`
- `int Id { get; set; }` - Primary key
- `string Name { get; set; }` - Plugin type name (e.g., "Anonymous", "TypeA")
- `string Path { get; set; }` - Plugin DLL path (e.g., "listener-plugins/TypeA/TypeAListener.dll")
- `string CreateBy { get; set; }` - Created by user
- `DateTime CreateDate { get; set; }` - Creation date
- `string ModifyBy { get; set; }` - Modified by user
- `DateTime ModifyDate { get; set; }` - Modification date
- `string Notes { get; set; }` - Additional notes

#### Task 1.3: Update Database Schema
**Files**: 
- `code/SQL/operation-db/` - Add migration scripts
- Update repository classes in `code/Repo.SQL/`

**Schema Changes:**

1. **Create ListenerDefinitionType table:**
```sql
USE [SimpleHooks]
GO

CREATE TABLE [dbo].[ListenerDefinitionType](
	[Id] [int] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Path] [nvarchar](1000) NOT NULL,
	[CreateBy] [nvarchar](50) NOT NULL,
	[CreateDate] [datetime2](7) NOT NULL,
	[ModifyBy] [nvarchar](50) NOT NULL,
	[ModifyDate] [datetime2](7) NOT NULL,
	[Notes] [nvarchar](1000) NOT NULL,
 CONSTRAINT [PK_ListenerDefinitionAuthType] PRIMARY KEY CLUSTERED
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_ListenerDefinitionType] ON [dbo].[ListenerDefinitionType]
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

ALTER TABLE [dbo].[ListenerDefinitionType] ADD  CONSTRAINT [DF_ListenerDefinitionType_CreateDate]  DEFAULT (getdate()) FOR [CreateDate]
GO
ALTER TABLE [dbo].[ListenerDefinitionType] ADD  CONSTRAINT [DF_ListenerDefinitionType_ModifyDate]  DEFAULT (getdate()) FOR [ModifyDate]
GO
ALTER TABLE [dbo].[ListenerDefinitionType] ADD  CONSTRAINT [DF_ListenerDefinitionType_Notes]  DEFAULT ('') FOR [Notes]
GO
```

2. **Populate ListenerDefinitionType with initial data:**
```sql
INSERT ListenerDefinitionType(Id, [Name], [Path], CreateBy, ModifyBy, Notes) 
VALUES (1, 'Anonymous', 'listener-plugins/Anonymous/Anonymous.dll', 'system.admin', 'system.admin', '');

INSERT ListenerDefinitionType(Id, [Name], [Path], CreateBy, ModifyBy, Notes) 
VALUES (2, 'TypeA', 'listener-plugins/TypeA/TypeAListener.dll', 'system.admin', 'system.admin', 'include Bearer token in header');
```

3. **Alter ListenerDefinition table:**
```sql
ALTER TABLE ListenerDefinition 
ADD Type_Id INT NOT NULL DEFAULT (1),
    Type_Options NVARCHAR(200) NULL;

ALTER TABLE ListenerDefinition
ADD CONSTRAINT FK_ListenerDefinition_ListenerDefinitionType
FOREIGN KEY (Type_Id) REFERENCES ListenerDefinitionType(Id);
```

#### Task 1.4: Listener Executer Plugin Manager Service
**File**: `code/Business/ListenerExecuterPluginManager.cs`

**Responsibilities:**
- Load plugins from specified paths at startup
- Maintain dictionary of plugin instances by type
- Handle plugin lifecycle and error handling
- Validate plugin compatibility

**Key Methods:**
- `LoadPlugins()` - Load all plugins at startup
- `GetPlugin(string type)` - Retrieve plugin instance by type
- `ValidatePlugin(Assembly assembly)` - Ensure plugin implements interface

### Phase 2: Plugin Implementations

#### Task 2.1: Anonymous Plugin
**Project**: `SimpleHooks.Plugins.Anonymous`
**Location**: `code/SimpleHooks.Plugins.Anonymous/`

**Purpose**: 
- Replicate current listener execution logic
- No authentication, direct HTTP calls
- Maintain backward compatibility

**Implementation:**
- Move existing HTTP logic from `InstanceManager.ExecuteListener`
- Use `HttpClient.Simple` for HTTP calls
- Handle timeouts and error scenarios

#### Task 2.2: TypeA Plugin (Bearer Token Authentication)
**Project**: `SimpleHooks.Plugins.TypeA`
**Location**: `code/SimpleHooks.Plugins.TypeA/`

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
- Inject `ListenerExecuterPluginManager` dependency
- Replace `ExecuteListener` method logic
- Use plugin-based execution instead of direct HTTP calls
- Maintain existing logging and status update logic

**New ExecuteListener Logic:**
1. Get plugin instance from ListenerExecuterPluginManager by listener type
2. Read environment variable for options (if specified)
3. Call plugin's ExecuteAsync method
4. Process ListenerResult and update status
5. Handle plugin execution errors

#### Task 3.2: Update Dependency Injection
**Files**: 
- `code/SimpleHooks.Server/Program.cs`
- `code/SimpleHooks.Web/Startup.cs`

**Changes:**
- Register ListenerExecuterPluginManager as singleton
- Initialize plugins at startup
- Handle plugin loading errors gracefully

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
