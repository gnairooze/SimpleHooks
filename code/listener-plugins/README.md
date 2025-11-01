# SimpleHooks Listener Plugins

This directory contains listener plugin implementations for SimpleHooks. The plugin system allows multiple authentication methods and execution strategies through a plugin-based approach.

## Plugin Architecture

### Overview

The plugin system is built on these key components:

1. **IListener Interface** - Defines the contract all plugins must implement
2. **ListenerPluginManager** - Manages plugin lifecycle and loading
3. **DefinitionManager** - Initializes plugins during startup
4. **InstanceManager** - Executes plugins for listener instances

### Plugin Initialization Flow

```
Server Startup
    ↓
DI Container Registration
    ↓
DefinitionManager.LoadDefinitions()
    ├── Load ListenerTypes from database
    ├── Load ListenerDefinitions from database
    └── SetListenerPlugins()
        ├── For each ListenerDefinition:
        │   ├── Find matching ListenerType
        │   ├── Get plugin DLL path from ListenerType.Path
        │   ├── Read environment variable from ListenerDefinition.TypeOptions
        │   └── Call ListenerPluginManager.CreatePluginInstance()
        │       ├── Load assembly from DLL path
        │       ├── Find IListener implementation
        │       ├── Create plugin instance
        │       ├── Initialize plugin properties
        │       └── Return plugin instance
        └── Store plugin in ListenerDefinition.ListenerPlugin
```

### Plugin Execution Flow

```
Event Instance Processing
    ↓
InstanceManager.ExecuteListener()
    ↓
Get plugin from ListenerDefinition.ListenerPlugin
    ↓
Read type options from environment variable
    ↓
Call plugin.ExecuteAsync(eventData, typeOptions)
    ↓
Process ListenerResult
    ├── Check Succeeded property
    ├── Save all logs from Logs collection
    └── Update listener instance status
```

## Available Plugins

### Anonymous Plugin

**Location**: `SimpleHooks.ListenerPlugins.Anonymous`

**Purpose**: Direct HTTP calls without authentication (backward compatible)

**DLL Path**: `listener-plugins/Anonymous/Anonymous.dll`

**Type Options**: Not used

**Features**:
- Direct HTTP POST requests
- No authentication
- Maintains backward compatibility with existing listeners
- Handles timeouts and error scenarios

### TypeA Plugin

**Location**: `SimpleHooks.ListenerPlugins.TypeA`

**Purpose**: OAuth2 client credentials flow with bearer token authentication

**DLL Path**: `listener-plugins/TypeA/TypeAListener.dll`

**Type Options**: Environment variable name containing JSON configuration

**Configuration Format**:
```json
{
  "identityProviderUrl": "https://auth.example.com/token",
  "clientId": "your_client_id",
  "clientSecret": "your_client_secret",
  "scope": "api.read api.write"
}
```

**Features**:
- OAuth2 client credentials authentication
- Bearer token caching with expiration handling
- Automatic token refresh on 401 responses
- Secure credential management via environment variables

## Building Plugins

### Build Individual Plugin

```bash
cd code
dotnet build listener-plugins/SimpleHooks.ListenerPlugins.Anonymous/SimpleHooks.ListenerPlugins.Anonymous.csproj
dotnet build listener-plugins/SimpleHooks.ListenerPlugins.TypeA/SimpleHooks.ListenerPlugins.TypeA.csproj
```

### Build All Plugins

```bash
cd code
dotnet build SimpleHooks.sln
```

## Deploying Plugins

### Deployment Structure

```
SimpleHooks.Server/
├── listener-plugins/
│   ├── Anonymous/
│   │   └── Anonymous.dll
│   └── TypeA/
│       └── TypeAListener.dll
```

### Steps

1. Build the plugin projects
2. Copy plugin DLLs to the deployment directory
3. Update database with ListenerType records pointing to plugin paths
4. Set environment variables for plugin configurations
5. Restart the server to load plugins

## Database Configuration

### ListenerType Table

```sql
INSERT ListenerType(Id, [Name], [Path], CreateBy, ModifyBy, Notes)
VALUES (1, 'Anonymous', 'listener-plugins/Anonymous/Anonymous.dll', 'system.admin', 'system.admin', '');

INSERT ListenerType(Id, [Name], [Path], CreateBy, ModifyBy, Notes)
VALUES (2, 'TypeA', 'listener-plugins/TypeA/TypeAListener.dll', 'system.admin', 'system.admin', 'OAuth2 Bearer token authentication');
```

### ListenerDefinition Configuration

```sql
-- Anonymous listener (no authentication)
UPDATE ListenerDefinition
SET Type_Id = 1,
    Type_Options = ''
WHERE Id = 1;

-- TypeA listener (OAuth2 authentication)
UPDATE ListenerDefinition
SET Type_Id = 2,
    Type_Options = 'OAUTH_CLIENT_CREDENTIALS'
WHERE Id = 2;
```

### Environment Variables

```bash
# For TypeA plugin
export OAUTH_CLIENT_CREDENTIALS='{"identityProviderUrl":"https://auth.example.com/token","clientId":"abc123","clientSecret":"secret","scope":"api.read"}'
```

## Creating Custom Plugins

### Step 1: Create Plugin Project

```bash
mkdir code/listener-plugins/SimpleHooks.ListenerPlugins.YourPlugin
cd code/listener-plugins/SimpleHooks.ListenerPlugins.YourPlugin
dotnet new classlib -f net8.0
```

### Step 2: Add References

Edit `.csproj` file:

```xml
<ItemGroup>
  <ProjectReference Include="..\..\SimpleHooks.ListenerInterfaces\SimpleHooks.ListenerInterfaces.csproj" />
  <ProjectReference Include="..\..\HttpClient.Interface\HttpClient.Interface.csproj" />
  <ProjectReference Include="..\..\HttpClient.Simple\HttpClient.Simple.csproj" />
  <ProjectReference Include="..\..\Log.Interface\Log.Interface.csproj" />
</ItemGroup>
```

### Step 3: Implement IListener

```csharp
using SimpleTools.SimpleHooks.ListenerInterfaces;
using SimpleTools.SimpleHooks.Log.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleTools.SimpleHooks.ListenerPlugins.YourPlugin
{
    public class YourPluginListener : IListener
    {
        // IListener properties - set by ListenerPluginManager
        public string Url { get; set; }
        public int Timeout { get; set; }
        public List<string> Headers { get; set; }
        public string TypeOptionsValue { get; set; }

        public async Task<ListenerResult> ExecuteAsync(string eventData, string typeOptions)
        {
            var result = new ListenerResult();
            int logCounter = 0;

            try
            {
                // Your custom authentication/execution logic here

                result.Succeeded = true;
                result.Message = "Execution succeeded";

                result.Logs.Add(logCounter++, new LogModel
                {
                    LogType = LogModel.LogTypes.Information,
                    NotesA = "Your plugin execution log",
                    CreateDate = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                result.Succeeded = false;
                result.Message = $"Exception: {ex.Message}";

                result.Logs.Add(logCounter++, new LogModel
                {
                    LogType = LogModel.LogTypes.Error,
                    NotesA = ex.Message,
                    CreateDate = DateTime.UtcNow
                });
            }

            return result;
        }
    }
}
```

### Step 4: Add to Solution

```bash
cd code
dotnet sln SimpleHooks.sln add listener-plugins/SimpleHooks.ListenerPlugins.YourPlugin/SimpleHooks.ListenerPlugins.YourPlugin.csproj
```

### Step 5: Register in Database

```sql
INSERT ListenerType(Id, [Name], [Path], CreateBy, ModifyBy, Notes)
VALUES (3, 'YourPlugin', 'listener-plugins/YourPlugin/YourPluginListener.dll', 'system.admin', 'system.admin', 'Your plugin description');
```

## Troubleshooting

### Plugin Not Loading

**Error**: `FileNotFoundException: Plugin DLL not found`

**Solution**:
- Verify path in `ListenerType.Path` is correct
- Ensure plugin DLL is in the deployment directory
- Check if path is relative to `AppDomain.CurrentDomain.BaseDirectory`

### No IListener Implementation Found

**Error**: `InvalidOperationException: No type implementing IListener interface found`

**Solution**:
- Verify plugin class implements `IListener` interface
- Ensure plugin class is public and not abstract
- Check all IListener properties are implemented

### Environment Variable Not Found

**Issue**: Plugin receives empty string for type options

**Solution**:
- Verify environment variable name matches `ListenerDefinition.TypeOptions`
- Ensure environment variable is set before server starts
- Check variable is accessible to the server process

### Plugin Properties Not Initialized

**Issue**: Plugin properties (Url, Timeout, etc.) are null or default

**Solution**:
- This should not occur as properties are part of IListener interface
- Verify `ListenerPluginManager.CreatePluginInstance()` is called correctly
- Check `DefinitionManager.SetListenerPlugins()` is executed during startup

## Performance Considerations

- **Plugin Type Caching**: Plugin types are cached after first load
- **Instance Per Definition**: Each ListenerDefinition gets its own plugin instance
- **Startup Loading**: Plugins are loaded once at startup (not per execution)
- **Token Caching**: TypeA plugin caches authentication tokens with expiration

## Security Best Practices

1. **Environment Variables**: Store sensitive credentials in environment variables, not database
2. **Plugin Validation**: Only load plugins from trusted sources
3. **Assembly Loading**: Plugins are loaded from known paths only
4. **Credential Handling**: Never log credentials or tokens
5. **Token Expiration**: Always implement token expiration and refresh logic

## Version Compatibility

- **.NET Version**: net8.0
- **SimpleHooks**: Compatible with pluggable-listener branch
- **Backward Compatibility**: Anonymous plugin maintains full compatibility with existing listeners

## Support

For issues or questions about the plugin system:
1. Check this README and the main documentation
2. Review the plan document: `docs/pluggable-listener-plan.md`
3. Examine the existing plugin implementations as examples
4. Check server logs for detailed error messages

