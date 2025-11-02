# SimpleHooks Listener Plugin Development Guide

## Overview

This guide walks you through creating a custom listener plugin for SimpleHooks, using the TypeA plugin (Bearer Token Authentication) as a practical example. You'll learn how to develop, install, configure, and deploy your plugin.

## Table of Contents

1. [Understanding the Plugin Architecture](#understanding-the-plugin-architecture)
2. [Creating Your Plugin](#creating-your-plugin)
3. [Installation and Configuration](#installation-and-configuration)
4. [Database Setup](#database-setup)
5. [Testing Your Plugin](#testing-your-plugin)
6. [Deployment](#deployment)
7. [Use Cases and Examples](#use-cases-and-examples)

---

## Understanding the Plugin Architecture

### Plugin Lifecycle

1. **Startup**: Server loads `ListenerType` records from database
2. **Initialization**: `ListenerPluginManager` loads plugin DLLs and creates instances
3. **Execution**: When an event is triggered, the plugin's `ExecuteAsync` method is called
4. **Completion**: Plugin returns `ListenerResult` with success status and logs

### Key Components

- **IListener Interface**: Contract that all plugins must implement
- **ListenerResult**: Return object containing execution status and logs
- **ListenerType**: Database table defining available plugin types
- **ListenerDefinition**: Database table defining the listener configuration linking it to a specific plugin type
- **ListenerPluginManager**: Manages plugin loading and initialization

---

## Creating Your Plugin

### Step 1: Create the Plugin Project

Create a new .NET class library project:

```bash
cd code/listener-plugins
dotnet new classlib -n <ListenerPluginName>
```

### Step 2: Add Required References

Edit `<ListenerPluginName>.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net6.0</TargetFramework>
    <AssemblyVersion>2.8.0</AssemblyVersion>
    <FileVersion>2.8.0</FileVersion>
  </PropertyGroup>

  <ItemGroup>
    <!-- Core plugin interface -->
    <ProjectReference Include="..\..\SimpleHooks.ListenerInterfaces\SimpleHooks.ListenerInterfaces.csproj" />
    
    <!-- HTTP client for making requests -->
    <ProjectReference Include="..\..\HttpClient.Interface\HttpClient.Interface.csproj" />
    <ProjectReference Include="..\..\HttpClient.Simple\HttpClient.Simple.csproj" />
    
    <!-- Logging -->
    <ProjectReference Include="..\..\Log.Interface\Log.Interface.csproj" />
  </ItemGroup>

  <ItemGroup>
    <!-- For JSON parsing -->
    <PackageReference Include="System.Text.Json" Version="6.0.0" />
  </ItemGroup>

</Project>
```

### Step 3: Implement the IListener Interface

Create `<ListenerPluginName>.cs`:

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
        // Properties set by ListenerPluginManager during initialization
        public string Url { get; set; }
        public int Timeout { get; set; }
        public List<string> Headers { get; set; }
        public string TypeOptionsValue { get; set; }
        
        // Internal dependencies
        private readonly IHttpClient _httpClient;
        private string _cachedToken;
        private DateTime _tokenExpiration;
        
        public TypeAListener()
        {
            _httpClient = new SimpleTools.SimpleHooks.HttpClient.Simple.SimpleClient();
        }
        
        public async Task<ListenerResult> ExecuteAsync(int listenerInstanceId, string eventData, string typeOptions)
        {
            var result = new ListenerResult();
            int logCounter = 0;
            
            try
            {
                // Log plugin start
                result.Logs.Add(logCounter++, CreateLog("TypeA plugin starting execution"));
                
                // Parse authentication configuration
                var authConfig = ParseAuthConfig(typeOptions, result, ref logCounter);
                if (authConfig == null)
                {
                    result.Succeeded = false;
                    result.Message = "Invalid authentication configuration";
                    return result;
                }
                
                // Obtain bearer token
                string token = await GetBearerToken(authConfig, result, ref logCounter);
                if (string.IsNullOrWhiteSpace(token))
                {
                    result.Succeeded = false;
                    result.Message = "Failed to obtain bearer token";
                    return result;
                }
                
                // Prepare headers with authorization
                var headersWithAuth = new List<string>(Headers ?? new List<string>());
                headersWithAuth.Add($"Authorization: Bearer {token}");
                
                // Execute HTTP request
                result.Logs.Add(logCounter++, CreateLog($"Calling target URL: {Url}"));
                var httpResult = _httpClient.Post(Url, headersWithAuth, eventData, Timeout);
                
                // Process response
                if (httpResult.HttpCode >= 200 && httpResult.HttpCode < 300)
                {
                    result.Succeeded = true;
                    result.Message = $"HTTP call succeeded with status {httpResult.HttpCode}";
                    result.Logs.Add(logCounter++, CreateLog($"Success: {httpResult.HttpCode}"));
                }
                else if (httpResult.HttpCode == 401)
                {
                    // Token expired - retry with fresh token
                    result.Logs.Add(logCounter++, CreateLog("Received 401, refreshing token"));
                    result = await RetryWithNewToken(authConfig, eventData, result, logCounter);
                }
                else
                {
                    result.Succeeded = false;
                    result.Message = $"HTTP call failed with status {httpResult.HttpCode}";
                    result.Logs.Add(logCounter++, CreateLog($"Failed: {httpResult.HttpCode} - {httpResult.Body}"));
                }
            }
            catch (Exception ex)
            {
                result.Succeeded = false;
                result.Message = $"Exception: {ex.Message}";
                result.Logs.Add(logCounter++, CreateLog($"Exception: {ex.Message}\n{ex.StackTrace}"));
            }
            
            return result;
        }
        
        private TypeAOptions ParseAuthConfig(string typeOptions, ListenerResult result, ref int logCounter)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(typeOptions))
                {
                    result.Logs.Add(logCounter++, CreateLog("TypeOptions is empty"));
                    return null;
                }
                
                var config = JsonSerializer.Deserialize<TypeAOptions>(typeOptions);
                
                if (string.IsNullOrWhiteSpace(config?.IdentityProviderUrl))
                {
                    result.Logs.Add(logCounter++, CreateLog("IdentityProviderUrl is missing"));
                    return null;
                }
                
                return config;
            }
            catch (Exception ex)
            {
                result.Logs.Add(logCounter++, CreateLog($"Failed to parse auth config: {ex.Message}"));
                return null;
            }
        }
        
        private async Task<string> GetBearerToken(TypeAOptions config, ListenerResult result, ref int logCounter)
        {
            // Check token cache
            if (!string.IsNullOrWhiteSpace(_cachedToken) && DateTime.UtcNow < _tokenExpiration)
            {
                result.Logs.Add(logCounter++, CreateLog("Using cached bearer token"));
                return _cachedToken;
            }
            
            try
            {
                result.Logs.Add(logCounter++, CreateLog($"Requesting token from {config.IdentityProviderUrl}"));
                
                // Build token request
                var tokenRequestBody = $"grant_type=client_credentials&client_id={config.ClientId}&client_secret={config.ClientSecret}&scope={Uri.EscapeDataString(config.Scope ?? "")}";
                var tokenHeaders = new List<string> { "Content-Type: application/x-www-form-urlencoded" };
                
                // Request token
                var tokenResult = _httpClient.Post(config.IdentityProviderUrl, tokenHeaders, tokenRequestBody, 5);
                
                if (tokenResult.HttpCode >= 200 && tokenResult.HttpCode < 300)
                {
                    var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(tokenResult.Body);
                    _cachedToken = tokenResponse.AccessToken;
                    
                    // Cache token with 60 second buffer before expiry
                    _tokenExpiration = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn - 60);
                    
                    result.Logs.Add(logCounter++, CreateLog("Bearer token obtained successfully"));
                    return _cachedToken;
                }
                else
                {
                    result.Logs.Add(logCounter++, CreateLog($"Token request failed: {tokenResult.HttpCode} - {tokenResult.Body}"));
                    return null;
                }
            }
            catch (Exception ex)
            {
                result.Logs.Add(logCounter++, CreateLog($"Exception obtaining token: {ex.Message}"));
                return null;
            }
        }
        
        private async Task<ListenerResult> RetryWithNewToken(TypeAOptions config, string eventData, ListenerResult result, int logCounter)
        {
            // Invalidate cached token
            _cachedToken = null;
            
            // Get fresh token
            string token = await GetBearerToken(config, result, ref logCounter);
            if (string.IsNullOrWhiteSpace(token))
            {
                result.Succeeded = false;
                result.Message = "Failed to refresh token";
                return result;
            }
            
            // Retry request with new token
            var headersWithAuth = new List<string>(Headers ?? new List<string>());
            headersWithAuth.Add($"Authorization: Bearer {token}");
            
            var httpResult = _httpClient.Post(Url, headersWithAuth, eventData, Timeout);
            
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
            
            result.Logs.Add(logCounter++, CreateLog($"Retry result: {httpResult.HttpCode}"));
            return result;
        }
        
        private LogModel CreateLog(string message)
        {
            return new LogModel
            {
                LogType = LogModel.LogTypes.Information,
                Step = "TypeA Plugin",
                NotesA = message,
                CreateDate = DateTime.UtcNow
            };
        }
    }
}
```

### Step 4: Create Configuration Models

Add supporting classes in the same file or separate files:

```csharp
namespace <plugin-namespace>
{
    /// <summary>
    /// Authentication configuration for OAuth2 client credentials flow
    /// </summary>
    public class <PluginOptionsName>
    {
        public string IdentityProviderUrl { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Scope { get; set; }
    }
    
    /// <summary>
    /// OAuth2 token response model
    /// </summary>
    public class TokenResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("access_token")]
        public string AccessToken { get; set; }
        
        [System.Text.Json.Serialization.JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
        
        [System.Text.Json.Serialization.JsonPropertyName("token_type")]
        public string TokenType { get; set; }
    }
}
```

### Step 5: Build Your Plugin

```bash
dotnet build code/listener-plugins/<ListenerPluginName>
```

---

## Installation and Configuration

### Step 1: Add Plugin to Solution

Edit `code/SimpleHooks.sln` to include your plugin project (or use Visual Studio to add it):

```bash
dotnet sln code/SimpleHooks.sln add code/listener-plugins/<ListenerPluginName>/<ListenerPluginName>.csproj
```

### Step 2: Update Release Automation Script

Edit `release_automation.py` to include your plugin in the build process:

```python
# Add to listener_plugins dictionary
self.listener_plugins = {
    "<ListenerPluginName>": {
        "path": self.code_path / "listener-plugins/SimpleHooks.ListenerPlugins.Anonymous",
        "csproj": "<ListenerPluginName>.csproj",
        "docker_tag": None
    }
}
```

### Step 3: Publish Plugin

```bash
# Manual publish for testing
dotnet publish code/listener-plugins/<ListenerPluginName>/<ListenerPluginName>.csproj \
  -c Release \
  -r win-x64 \
  --self-contained true \
  -o publish/<ListenerPluginName>

# Or use the automation script
python release_automation.py 2.8.3 --steps publish
```

---

## Database Setup

### Step 1: Register Plugin Type

Insert a record into the `ListenerType` table:

```sql
USE [SimpleHooks]
GO

INSERT INTO [dbo].[ListenerType] (
    [Id], 
    [Name], 
    [Path], 
    [CreateBy], 
    [ModifyBy], 
    [Notes]
)
VALUES (
    <ListenerPluginId>, 
    '<ListenerPluginName>', 
    'listener-plugins/<ListenerPluginName>/<ListenerPluginName>.dll', 
    'system.admin', 
    'system.admin', 
    '<PluginDescription>'
);
```

### Step 2: Configure Environment Variables

Set the environment variable for authentication configuration:

**Windows:**
```powershell
# Set for current session
$env:<PluginOptionsName> = '{"identityProviderUrl":"https://auth.example.com/oauth/token","clientId":"your_client_id","clientSecret":"your_client_secret","scope":"api.read api.write"}'

# Set permanently (system-wide)
[System.Environment]::SetEnvironmentVariable("<PluginOptionsName>", '{"identityProviderUrl":"https://auth.example.com/oauth/token","clientId":"your_client_id","clientSecret":"your_client_secret","scope":"api.read api.write"}', "Machine")
```

**Linux/macOS:**
```bash
# Add to ~/.bashrc or /etc/environment
export <PluginOptionsName>='{"identityProviderUrl":"https://auth.example.com/oauth/token","clientId":"your_client_id","clientSecret":"your_client_secret","scope":"api.read api.write"}'
```

**Docker:**
```yaml
# docker-compose.yml
services:
  simplehooks-server:
    environment:
      - <PluginOptionsName>={"identityProviderUrl":"https://auth.example.com/oauth/token","clientId":"your_client_id","clientSecret":"your_client_secret","scope":"api.read api.write"}
```

### Step 3: Create Listener Definition

Insert or update a `ListenerDefinition` to use your plugin:

```sql
USE [SimpleHooks]
GO

-- Create or update listener definition
DECLARE @ListenerDefId INT = 1001;

INSERT INTO [dbo].[ListenerDefinition] (
    [Id],
    [Name],
    [Url],
    [Type_Id],
    [Type_Options],
    [Timeout],
    [TrialCount],
    [RetrialDelay],
    [Active],
    [CreateBy],
    [ModifyBy],
    [Notes]
)
VALUES (
    @ListenerDefId,
    'My Protected API',
    'https://api.example.com/webhooks/simplehooks',
    <ListenerPluginId>, -- <ListenerPluginName> plugin
    '<PluginOptionsName>', -- Environment variable name
    60, -- 60 minute timeout
    3, -- 3 retry attempts
    5, -- 5 minute retry delay
    1, -- Active
    'admin',
    'admin',
    '<PluginDescription>'
);

-- Link to an event definition
INSERT INTO [dbo].[EventDefinitionListenerDefinition] (
    [EventDefinitionId],
    [ListenerDefinitionId],
    [Active],
    [CreateBy],
    [ModifyBy]
)
VALUES (
    1, -- Your event definition ID
    @ListenerDefId,
    1,
    'admin',
    'admin'
);
```

---

## Testing Your Plugin

### Unit Testing

Create a test project:

```bash
dotnet new xunit -n <ListenerPluginName>.Tests
cd <ListenerPluginName>.Tests
dotnet add reference ../<ListenerPluginName>/<ListenerPluginName>.csproj
```

Example test:

```csharp
using Xunit;
using <plugin-namespace>;

public class <ListenerPluginName>ListenerTests
{
    [Fact]
    public async Task ExecuteAsync_ValidConfig_ReturnsSuccess()
    {
        // Arrange
        var listener = new <ListenerPluginName>Listener();
        listener.Url = "https://httpbin.org/post";
        listener.Timeout = 5;
        listener.Headers = new List<string> { "Content-Type: application/json" };
        
        var authConfig = new <PluginOptionsName>
        {
            IdentityProviderUrl = "https://auth.example.com/token",
            ClientId = "test_client",
            ClientSecret = "test_secret",
            Scope = "api.read"
        };
        
        var typeOptions = System.Text.Json.JsonSerializer.Serialize(authConfig);
        var eventData = "{\"test\":\"data\"}";
        
        // Act
        var result = await listener.ExecuteAsync(1, eventData, typeOptions);
        
        // Assert
        Assert.NotNull(result);
        Assert.True(result.Logs.Count > 0);
    }
}
```

### Integration Testing

Test with SimpleHooks server:

1. Deploy plugin to `listener-plugins/<ListenerPluginName>/` directory
2. Configure database with plugin type and listener definition
3. Set environment variables
4. Restart SimpleHooks server
5. Trigger an event and verify execution

---

## Deployment

### Deployment Structure

```
SimpleHooks.Server/
├── SimpleHooks.Server.exe
├── listener-plugins/
│   ├── Anonymous/
│   │   ├── <ListenerPluginName>.dll
│   │   └── [dependencies]
│   └── <ListenerPluginName>/
│       ├── <ListenerPluginName>.dll
│       └── [dependencies]
└── [other server files]
```

### Deployment Steps

1. **Build and publish plugin:**
   ```bash
   python release_automation.py 2.8.3 --steps publish
   ```

2. **Copy plugin files to server:**
   ```bash
   # The automation script handles this, or manually:
   xcopy /E /I publish\<ListenerPluginName> \
         C:\SimpleHooks\listener-plugins\<ListenerPluginName>\
   ```

3. **Configure environment variables** on the server

4. **Update database** with ListenerType and ListenerDefinition records

5. **Restart SimpleHooks services:**
   ```bash
   # Windows Service
   net stop SimpleHooksServer
   net start SimpleHooksServer
   
   # Or Docker
   docker-compose restart simplehooks-server
   ```

### Docker Deployment

The release automation script automatically copies plugins to each Docker build context. Your plugin will be included in the Docker image.

Verify with:
```dockerfile
# In Dockerfile (already configured)
COPY publish/listener-plugins /app/listener-plugins
```

---

## Use Cases and Examples

### 1. RESTful API Integration with Authentication

**Scenario**: Call protected APIs that require OAuth2 authentication

**Example**: Integration with Salesforce, Microsoft Graph API, or custom enterprise APIs

```sql
-- Salesforce webhook listener
INSERT INTO ListenerDefinition (Name, Url, Type_Id, Type_Options, ...)
VALUES (
    'Salesforce Lead Notification',
    'https://your-instance.salesforce.com/services/apexrest/webhook',
    <ListenerPluginId>, -- <ListenerPluginName>
    'SALESFORCE_AUTH_CONFIG',
    ...
);
```

Environment variable:
```json
{
  "identityProviderUrl": "https://login.salesforce.com/services/oauth2/token",
  "clientId": "your_salesforce_client_id",
  "clientSecret": "your_salesforce_client_secret",
  "scope": "api"
}
```

### 2. Microservices Communication

**Scenario**: Notify internal microservices about events with service-to-service authentication

**Example**: Order processing system notifying inventory and shipping services

```sql
-- Inventory service listener
INSERT INTO ListenerDefinition (Name, Url, Type_Id, Type_Options, ...)
VALUES (
    'Inventory Update Service',
    'https://inventory-service.internal/api/v1/stock/update',
    <ListenerPluginId>, -- <ListenerPluginName>
    '<PluginOptionsName>',
    ...
);

-- Shipping service listener
INSERT INTO ListenerDefinition (Name, Url, Type_Id, Type_Options, ...)
VALUES (
    'Shipping Notification Service',
    'https://shipping-service.internal/api/v1/orders/new',
    <ListenerPluginId>, -- <ListenerPluginName>
    '<PluginOptionsName>',
    ...
);
```

### 3. Third-Party SaaS Integration

**Scenario**: Integrate with SaaS platforms that require authenticated webhooks

**Example**: Slack, Microsoft Teams, Zendesk, or custom applications

```sql
-- Slack notification with bot token
INSERT INTO ListenerDefinition (Name, Url, Type_Id, Type_Options, ...)
VALUES (
    'Slack Notification Channel',
    'https://slack.com/api/chat.postMessage',
    <ListenerPluginId>, -- <ListenerPluginName>
    'SLACK_BOT_AUTH',
    ...
);
```

### 4. Business Logic Execution

**Scenario**: Execute custom business logic without creating a full plugin

**Example**: Call Azure Functions, AWS Lambda, or custom endpoints

```sql
-- Azure Function trigger
INSERT INTO ListenerDefinition (Name, Url, Type_Id, Type_Options, ...)
VALUES (
    'Data Processing Function',
    'https://my-function-app.azurewebsites.net/api/process-data',
    <ListenerPluginId>, -- <ListenerPluginName>
    'AZURE_FUNCTION_AUTH',
    ...
);
```

### 5. Multi-Tenant Event Distribution

**Scenario**: Route events to different tenant endpoints with tenant-specific authentication

**Example**: SaaS application notifying multiple customer systems

```sql
-- Tenant A listener
INSERT INTO ListenerDefinition (Name, Url, Type_Id, Type_Options, ...)
VALUES (
    'Tenant A Webhook',
    'https://tenant-a.example.com/api/webhooks/events',
    <ListenerPluginId>, -- <ListenerPluginName>
    'TENANT_A_AUTH',
    ...
);

-- Tenant B listener
INSERT INTO ListenerDefinition (Name, Url, Type_Id, Type_Options, ...)
VALUES (
    'Tenant B Webhook',
    'https://tenant-b.example.com/api/webhooks/events',
    <ListenerPluginId>, -- <ListenerPluginName>
    'TENANT_B_AUTH',
    ...
);
```

### 6. Audit and Compliance Systems

**Scenario**: Send audit events to compliance systems with secure authentication

**Example**: Financial audit logs, HIPAA compliance tracking

```sql
-- Audit system listener
INSERT INTO ListenerDefinition (Name, Url, Type_Id, Type_Options, ...)
VALUES (
    'Compliance Audit Logger',
    'https://audit.compliance.example.com/api/v2/events',
    <ListenerPluginId>, -- <ListenerPluginName>
    '<PluginOptionsName>',
    ...
);
```

### 7. Data Synchronization

**Scenario**: Synchronize data between systems with authenticated API calls

**Example**: CRM to ERP integration, database replication triggers

```sql
-- ERP synchronization
INSERT INTO ListenerDefinition (Name, Url, Type_Id, Type_Options, ...)
VALUES (
    'ERP Customer Sync',
    'https://erp.company.com/api/customers/sync',
    <ListenerPluginId>, -- <ListenerPluginName>
    'ERP_INTEGRATION_AUTH',
    ...
);
```

### 8. IoT and Device Management

**Scenario**: Send commands to IoT devices or platforms with authentication

**Example**: Azure IoT Hub, AWS IoT Core integration

```sql
-- IoT command dispatcher
INSERT INTO ListenerDefinition (Name, Url, Type_Id, Type_Options, ...)
VALUES (
    'IoT Device Controller',
    'https://iot-hub.azure.com/devices/command',
    <ListenerPluginId>, -- <ListenerPluginName>
    '<PluginOptionsName>',
    ...
);
```

---

## Advanced Plugin Patterns

### Pattern 1: Retry Logic with Exponential Backoff

Enhance your plugin to implement exponential backoff for transient failures:

```csharp
private async Task<HttpResult> ExecuteWithRetry(string url, List<string> headers, string body, int maxRetries = 3)
{
    for (int attempt = 0; attempt < maxRetries; attempt++)
    {
        var result = _httpClient.Post(url, headers, body, Timeout);
        
        if (result.HttpCode < 500) // Don't retry on client errors
            return result;
        
        if (attempt < maxRetries - 1)
        {
            int delaySeconds = (int)Math.Pow(2, attempt); // Exponential backoff
            await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
        }
    }
}
```

### Pattern 2: Request Transformation

Transform event data before sending:

```csharp
public async Task<ListenerResult> ExecuteAsync(int listenerInstanceId, string eventData, string typeOptions)
{
    // Parse and transform event data
    var eventJson = JsonSerializer.Deserialize<JsonElement>(eventData);
    var transformedData = TransformForTargetSystem(eventJson);
    
    // Send transformed data
    var httpResult = _httpClient.Post(Url, Headers, transformedData, Timeout);
    // ...
}
```

### Pattern 3: Multi-Step Workflows

Implement multi-step API workflows:

```csharp
public async Task<ListenerResult> ExecuteAsync(int listenerInstanceId, string eventData, string typeOptions)
{
    // Step 1: Create resource
    var createResult = await CreateResource(eventData);
    
    // Step 2: Update metadata
    if (createResult.Succeeded)
    {
        var updateResult = await UpdateMetadata(createResult.ResourceId);
    }
    
    // Step 3: Trigger downstream process
    // ...
}
```

---

## Troubleshooting

### Common Issues

1. **Plugin Not Loading**
   - Check DLL path in `ListenerType.Path` is correct
   - Verify all dependencies are in the plugin folder
   - Review server logs for assembly loading errors

2. **Authentication Failures**
   - Verify environment variable name matches `Type_Options` field
   - Check JSON format in environment variable
   - Validate credentials with OAuth provider

3. **Timeout Issues**
   - Increase `Timeout` value in `ListenerDefinition`
   - Check network connectivity to target URL
   - Review OAuth token acquisition time

4. **Token Not Refreshing**
   - Verify token expiration logic
   - Check if identity provider returns `expires_in` correctly
   - Review token caching implementation

---

## Best Practices

1. **Security**
   - Never hardcode credentials in plugin code
   - Always use environment variables for secrets
   - Implement secure token storage and caching
   - Validate all input data

2. **Error Handling**
   - Catch and log all exceptions
   - Return meaningful error messages
   - Don't throw exceptions from `ExecuteAsync`
   - Log detailed diagnostics for troubleshooting

3. **Performance**
   - Cache tokens when possible
   - Reuse HTTP client connections
   - Implement connection pooling
   - Set appropriate timeouts

4. **Logging**
   - Log key execution steps
   - Include timing information
   - Log request/response details (without secrets)
   - Use structured logging

5. **Testing**
   - Unit test token acquisition logic
   - Mock external dependencies
   - Test retry and timeout scenarios
   - Perform integration tests with real endpoints

---

## Additional Resources

- [IListener Interface Documentation](./support-pluggable-listeners-executers.md)
- [Plugin Quick Start Guide](./PLUGIN_QUICK_START.md)
- [Deployment Guide](./DEPLOYMENT_GUIDE.md)
- [SimpleHooks Architecture](../README.md)

## Support

For questions or issues:
- Create an issue on GitHub
- Review existing documentation
- Check server logs in `logs/` directory
- Contact: [project maintainer contact info]

---

**Version**: 3.0.3  
**Last Updated**: 2025-11-02  
**Author**: SimpleHooks Team
