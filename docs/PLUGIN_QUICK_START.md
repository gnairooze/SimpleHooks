# SimpleHooks Plugin Quick Start Guide

## For Developers: Using the Plugin System

### Quick Reference

#### Anonymous Plugin (No Authentication)

```sql
-- Database setup
INSERT ListenerType(Id, [Name], [Path], CreateBy, ModifyBy, Notes) 
VALUES (1, 'Anonymous', 'listener-plugins/Anonymous/Anonymous.dll', 'system.admin', 'system.admin', '');

-- Listener configuration
INSERT ListenerDefinition(Id, Name, Url, Timeout, TrialCount, RetrialDelay, Type_Id, Type_Options, CreateBy, ModifyBy)
VALUES (1, 'My Webhook', 'https://example.com/webhook', 5, 3, 1, 1, '', 'admin', 'admin');
```

**No environment variable needed!**

#### TypeA Plugin (OAuth2 Bearer Token)

```sql
-- Database setup
INSERT ListenerType(Id, [Name], [Path], CreateBy, ModifyBy, Notes) 
VALUES (2, 'TypeA', 'listener-plugins/TypeA/TypeAListener.dll', 'system.admin', 'system.admin', 'OAuth2 Bearer token');

-- Listener configuration
INSERT ListenerDefinition(Id, Name, Url, Timeout, TrialCount, RetrialDelay, Type_Id, Type_Options, CreateBy, ModifyBy)
VALUES (2, 'Secure API', 'https://api.example.com/endpoint', 5, 3, 1, 2, 'MY_API_AUTH', 'admin', 'admin');
```

**Environment variable setup**:

```bash
# Linux/Mac
export MY_API_AUTH='{"identityProviderUrl":"https://auth.example.com/token","clientId":"abc123","clientSecret":"secret","scope":"api.read"}'

# Windows PowerShell
$env:MY_API_AUTH='{"identityProviderUrl":"https://auth.example.com/token","clientId":"abc123","clientSecret":"secret","scope":"api.read"}'

# Windows CMD
set MY_API_AUTH={"identityProviderUrl":"https://auth.example.com/token","clientId":"abc123","clientSecret":"secret","scope":"api.read"}
```

### How It Works

#### 1. Server Startup

```
1. Server starts
2. Loads ListenerType table → knows which plugins exist
3. Loads ListenerDefinition table → knows which listeners to use
4. For each listener:
   - Finds its plugin DLL path from ListenerType
   - Loads the plugin DLL
   - Creates plugin instance
   - Stores instance in memory
```

#### 2. Event Processing

```
1. Event comes in
2. System finds associated listeners
3. For each listener:
   - Gets plugin instance (already loaded)
   - Reads environment variable if needed
   - Calls plugin.ExecuteAsync()
   - Saves result and logs
   - Updates status
```

### Common Scenarios

#### Scenario 1: Add New Listener (No Auth)

```sql
-- Just use Type_Id = 1 for Anonymous plugin
INSERT ListenerDefinition(Name, Url, Timeout, TrialCount, RetrialDelay, Type_Id, Type_Options, CreateBy, ModifyBy)
VALUES ('My New Webhook', 'https://example.com/webhook', 5, 3, 1, 1, '', 'admin', 'admin');
```

Restart server. Done! ✓

#### Scenario 2: Add New Listener (OAuth2)

```sql
-- Use Type_Id = 2 for TypeA plugin
INSERT ListenerDefinition(Name, Url, Timeout, TrialCount, RetrialDelay, Type_Id, Type_Options, CreateBy, ModifyBy)
VALUES ('Secure API', 'https://api.example.com/endpoint', 5, 3, 1, 2, 'SECURE_API_AUTH', 'admin', 'admin');
```

Set environment variable:

```bash
export SECURE_API_AUTH='{"identityProviderUrl":"https://auth.example.com/token","clientId":"client123","clientSecret":"secret123","scope":"api.read api.write"}'
```

Restart server. Done! ✓

#### Scenario 3: Update OAuth2 Credentials

**Option A: Change environment variable (recommended)**

```bash
# Update the environment variable
export MY_API_AUTH='{"identityProviderUrl":"https://auth.example.com/token","clientId":"NEW_CLIENT","clientSecret":"NEW_SECRET","scope":"api.read"}'

# Restart server
```

**Option B: Change environment variable name**

```sql
-- Update the listener definition
UPDATE ListenerDefinition 
SET Type_Options = 'NEW_ENV_VAR_NAME'
WHERE Id = 2;
```

```bash
# Set new environment variable
export NEW_ENV_VAR_NAME='{"identityProviderUrl":"...","clientId":"...","clientSecret":"...","scope":"..."}'

# Restart server
```

#### Scenario 4: Convert Anonymous to OAuth2

```sql
-- Change Type_Id from 1 to 2 and add Type_Options
UPDATE ListenerDefinition 
SET Type_Id = 2, 
    Type_Options = 'MY_AUTH_CONFIG'
WHERE Id = 1;
```

```bash
# Set environment variable
export MY_AUTH_CONFIG='{"identityProviderUrl":"...","clientId":"...","clientSecret":"...","scope":"..."}'

# Restart server
```

#### Scenario 5: Convert OAuth2 to Anonymous

```sql
-- Change Type_Id from 2 to 1 and clear Type_Options
UPDATE ListenerDefinition 
SET Type_Id = 1, 
    Type_Options = ''
WHERE Id = 2;
```

No environment variable needed. Restart server. Done! ✓

### Troubleshooting

#### Problem: Listener fails with "No plugin instance found"

**Cause**: Plugin not loaded at startup

**Solutions**:
1. Check ListenerType table has record for the plugin
2. Verify plugin DLL path is correct
3. Ensure plugin DLL exists in deployment directory
4. Check server logs for plugin loading errors
5. Restart server

#### Problem: TypeA plugin fails with "Invalid or missing authentication configuration"

**Cause**: Environment variable not found or malformed

**Solutions**:
1. Verify environment variable name matches `Type_Options` value
2. Check environment variable is set: `echo $MY_VAR_NAME`
3. Verify JSON is valid (use JSON validator)
4. Ensure all required fields present: identityProviderUrl, clientId, clientSecret, scope
5. Restart server after setting environment variable

#### Problem: TypeA plugin fails with "Failed to obtain bearer token"

**Cause**: OAuth2 authentication failed

**Solutions**:
1. Check clientId and clientSecret are correct
2. Verify identityProviderUrl is accessible
3. Test token endpoint manually: 
   ```bash
   curl -X POST https://auth.example.com/token \
     -H "Content-Type: application/x-www-form-urlencoded" \
     -d "grant_type=client_credentials&client_id=abc123&client_secret=secret&scope=api.read"
   ```
4. Check scope is valid for your application
5. Review plugin logs for detailed error messages

#### Problem: Changes not taking effect

**Cause**: Server needs restart to reload plugins

**Solution**: Restart the server after:
- Database changes to ListenerDefinition or ListenerType
- Environment variable changes
- Plugin DLL updates

### Best Practices

#### 1. Environment Variable Naming

✓ **Good**: 
- `OAUTH_CLIENT_CREDENTIALS`
- `SECURE_API_AUTH`
- `PARTNER_API_CONFIG`

✗ **Bad**:
- `config` (too generic)
- `secret` (unclear purpose)
- `xyz123` (not descriptive)

#### 2. OAuth2 Configuration

✓ **Good**:
```json
{
  "identityProviderUrl": "https://auth.example.com/token",
  "clientId": "my-client-id",
  "clientSecret": "very-secret-value",
  "scope": "api.read api.write"
}
```

✗ **Bad**:
```json
{
  "url": "...",  // Wrong key name
  "client": "...",  // Missing clientId
  "secret": "..."  // Missing clientSecret
}
```

#### 3. Security

✓ **Do**:
- Store credentials in environment variables
- Use different credentials per environment
- Rotate credentials regularly
- Use minimal required scopes

✗ **Don't**:
- Store credentials in database
- Hardcode credentials in code
- Log credentials or tokens
- Share credentials between environments

#### 4. Testing

✓ **Do**:
- Test in dev/staging before production
- Verify token expiration and refresh works
- Test with invalid credentials
- Monitor logs for errors

✗ **Don't**:
- Test with production credentials in dev
- Skip testing OAuth2 token refresh
- Ignore error logs
- Deploy without testing

### Deployment Checklist

#### Before Deploying

- [ ] Build all plugin projects
- [ ] Copy plugin DLLs to deployment directory
- [ ] Update database with ListenerType records
- [ ] Configure ListenerDefinition records
- [ ] Set all required environment variables
- [ ] Test plugins in staging environment
- [ ] Verify server starts without errors
- [ ] Test at least one event through each plugin

#### Deployment Structure

```
SimpleHooks.Server/
├── SimpleHooks.Server.dll
├── listener-plugins/
│   ├── Anonymous/
│   │   ├── Anonymous.dll
│   │   ├── SimpleHooks.ListenerInterfaces.dll
│   │   ├── HttpClient.Interface.dll
│   │   ├── HttpClient.Simple.dll
│   │   └── Log.Interface.dll
│   └── TypeA/
│       ├── TypeAListener.dll
│       ├── SimpleHooks.ListenerInterfaces.dll
│       ├── HttpClient.Interface.dll
│       ├── HttpClient.Simple.dll
│       └── Log.Interface.dll
```

### Quick Commands

#### Build Plugins

```bash
cd code
dotnet build listener-plugins/SimpleHooks.ListenerPlugins.Anonymous/SimpleHooks.ListenerPlugins.Anonymous.csproj
dotnet build listener-plugins/SimpleHooks.ListenerPlugins.TypeA/SimpleHooks.ListenerPlugins.TypeA.csproj
```

#### Build Everything

```bash
cd code
dotnet build SimpleHooks.sln
```

#### Check Environment Variables

```bash
# Linux/Mac
printenv | grep -i auth

# Windows PowerShell
Get-ChildItem Env: | Where-Object {$_.Name -like "*auth*"}

# Windows CMD
set | findstr /i auth
```

#### View Server Logs

Check the logs for these important messages:

- Plugin loading: "CreatePluginInstance"
- Plugin execution: "Anonymous plugin executing" or "TypeA plugin executing"
- Token acquisition: "Requesting bearer token" or "Using cached bearer token"
- Errors: Look for "Exception" or "Error" log types

### Need Help?

1. **Check documentation**: 
   - `code/listener-plugins/README.md` - Detailed plugin documentation
   - `PHASE2_PHASE3_IMPLEMENTATION_SUMMARY.md` - Implementation details
   - `docs/pluggable-listener-plan.md` - Original design plan

2. **Check logs**:
   - Server startup logs for plugin loading issues
   - Event processing logs for execution errors
   - Plugin-specific logs in ListenerResult

3. **Common issues**:
   - Server restart required after changes
   - Environment variables must be set before server starts
   - Plugin DLLs must be in correct directory
   - JSON configuration must be valid

4. **Test incrementally**:
   - Start with Anonymous plugin (simpler)
   - Move to TypeA plugin once Anonymous works
   - Test authentication separately before full integration

