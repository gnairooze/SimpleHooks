# SimpleHooks Plugin System Deployment Guide

## Overview

This guide provides step-by-step instructions for deploying the pluggable listener system to development, staging, and production environments.

## Prerequisites

- .NET 8.0 SDK installed
- SQL Server database access
- Admin access to application server
- Ability to set environment variables on target server

## Deployment Steps

### Step 1: Build the Solution

```bash
cd code
dotnet build SimpleHooks.sln --configuration Release
```

**Expected Output**:
```
Build succeeded with 2 warning(s)
```

**Verify**:
- All projects build successfully
- Plugin DLLs are created in `bin/Release/net8.0/` directories

### Step 2: Collect Plugin DLLs

Create deployment package with plugin DLLs and their dependencies:

```bash
# Create deployment directory structure
mkdir -p deployment/listener-plugins/Anonymous
mkdir -p deployment/listener-plugins/TypeA

# Copy Anonymous plugin
cp listener-plugins/SimpleHooks.ListenerPlugins.Anonymous/bin/Release/net8.0/*.dll deployment/listener-plugins/Anonymous/

# Copy TypeA plugin
cp listener-plugins/SimpleHooks.ListenerPlugins.TypeA/bin/Release/net8.0/*.dll deployment/listener-plugins/TypeA/
```

**Files to Deploy**:

For Anonymous plugin:
- `SimpleHooks.ListenerPlugins.Anonymous.dll`
- `SimpleHooks.ListenerInterfaces.dll`
- `HttpClient.Interface.dll`
- `HttpClient.Simple.dll`
- `Log.Interface.dll`

For TypeA plugin:
- `SimpleHooks.ListenerPlugins.TypeA.dll`
- `SimpleHooks.ListenerInterfaces.dll`
- `HttpClient.Interface.dll`
- `HttpClient.Simple.dll`
- `Log.Interface.dll`
- `System.Text.Json.dll` (if not already in main app)

### Step 3: Update Database Schema

Connect to your SimpleHooks database and run these scripts:

#### 3.1: Create ListenerType Table

```sql
USE [SimpleHooks]
GO

-- Create table
CREATE TABLE [dbo].[ListenerType](
	[Id] [int] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Path] [nvarchar](1000) NOT NULL,
	[CreateBy] [nvarchar](50) NOT NULL,
	[CreateDate] [datetime2](7) NOT NULL,
	[ModifyBy] [nvarchar](50) NOT NULL,
	[ModifyDate] [datetime2](7) NOT NULL,
	[Notes] [nvarchar](1000) NOT NULL,
 CONSTRAINT [PK_ListenerType] PRIMARY KEY CLUSTERED ([Id] ASC)
) ON [PRIMARY]
GO

-- Create unique index
CREATE UNIQUE NONCLUSTERED INDEX [IX_ListenerType_Name] 
ON [dbo].[ListenerType]([Name] ASC)
GO

-- Add defaults
ALTER TABLE [dbo].[ListenerType] 
ADD CONSTRAINT [DF_ListenerType_CreateDate] DEFAULT (getdate()) FOR [CreateDate]
GO

ALTER TABLE [dbo].[ListenerType] 
ADD CONSTRAINT [DF_ListenerType_ModifyDate] DEFAULT (getdate()) FOR [ModifyDate]
GO

ALTER TABLE [dbo].[ListenerType] 
ADD CONSTRAINT [DF_ListenerType_Notes] DEFAULT ('') FOR [Notes]
GO
```

#### 3.2: Populate ListenerType Data

```sql
-- Insert Anonymous plugin
INSERT INTO ListenerType(Id, [Name], [Path], CreateBy, ModifyBy, Notes) 
VALUES (1, 'Anonymous', 'listener-plugins/Anonymous/SimpleHooks.ListenerPlugins.Anonymous.dll', 'system.admin', 'system.admin', 'Direct HTTP calls without authentication');

-- Insert TypeA plugin
INSERT INTO ListenerType(Id, [Name], [Path], CreateBy, ModifyBy, Notes) 
VALUES (2, 'TypeA', 'listener-plugins/TypeA/SimpleHooks.ListenerPlugins.TypeA.dll', 'system.admin', 'system.admin', 'OAuth2 client credentials with bearer token');
GO
```

#### 3.3: Alter ListenerDefinition Table

```sql
-- Add new columns
ALTER TABLE ListenerDefinition 
ADD Type_Id INT NOT NULL DEFAULT (1),
    Type_Options NVARCHAR(200) NOT NULL DEFAULT ('');
GO

-- Add foreign key constraint
ALTER TABLE ListenerDefinition
ADD CONSTRAINT FK_ListenerDefinition_ListenerType
FOREIGN KEY (Type_Id) REFERENCES ListenerType(Id);
GO
```

#### 3.4: Verify Schema Changes

```sql
-- Verify ListenerType table
SELECT * FROM ListenerType;

-- Should return:
-- Id | Name      | Path                                    | Notes
-- 1  | Anonymous | listener-plugins/Anonymous/Anonymous.dll | ...
-- 2  | TypeA     | listener-plugins/TypeA/TypeAListener.dll | ...

-- Verify ListenerDefinition columns
SELECT Id, Name, Type_Id, Type_Options FROM ListenerDefinition;

-- All existing listeners should have Type_Id = 1 (Anonymous)
```

### Step 4: Create Stored Procedures (if needed)

If you're using stored procedures for data access, create these:

```sql
-- ListenerType_GetAll
CREATE PROCEDURE [dbo].[ListenerType_GetAll]
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, [Name], [Path], CreateBy, CreateDate, ModifyBy, ModifyDate, Notes
    FROM ListenerType
    ORDER BY Id;
END
GO

-- ListenerType_GetById
CREATE PROCEDURE [dbo].[ListenerType_GetById]
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, [Name], [Path], CreateBy, CreateDate, ModifyBy, ModifyDate, Notes
    FROM ListenerType
    WHERE Id = @Id;
END
GO

-- Update existing ListenerDefinition procedures to include new columns
-- (Adjust based on your actual stored procedures)
```

### Step 5: Deploy Application Files

#### 5.1: Stop the Application

```bash
# Stop SimpleHooks.Server service
sudo systemctl stop simplehooks-server

# Or if running as Windows Service
net stop SimpleHooksServer
```

#### 5.2: Deploy Application DLLs

```bash
# Backup existing deployment
cp -r /opt/simplehooks /opt/simplehooks.backup.$(date +%Y%m%d_%H%M%S)

# Deploy new version
cp -r deployment/* /opt/simplehooks/

# Ensure correct permissions
chown -R simplehooks:simplehooks /opt/simplehooks
chmod -R 755 /opt/simplehooks
```

#### 5.3: Verify File Structure

```bash
ls -la /opt/simplehooks/listener-plugins/

# Should show:
# drwxr-xr-x  Anonymous/
# drwxr-xr-x  TypeA/

ls -la /opt/simplehooks/listener-plugins/Anonymous/
# Should show:
# -rwxr-xr-x  SimpleHooks.ListenerPlugins.Anonymous.dll
# -rwxr-xr-x  SimpleHooks.ListenerInterfaces.dll
# ... other dependencies
```

### Step 6: Configure Environment Variables

#### 6.1: Development Environment

```bash
# Edit environment file
sudo nano /opt/simplehooks/environment.conf

# Add OAuth2 configurations
OAUTH_CLIENT_CREDENTIALS='{"identityProviderUrl":"https://auth-dev.example.com/token","clientId":"dev-client","clientSecret":"dev-secret","scope":"api.read api.write"}'

# Save and exit
```

#### 6.2: Staging Environment

```bash
# Use staging credentials
OAUTH_CLIENT_CREDENTIALS='{"identityProviderUrl":"https://auth-staging.example.com/token","clientId":"staging-client","clientSecret":"staging-secret","scope":"api.read api.write"}'
```

#### 6.3: Production Environment

```bash
# Use production credentials (SECURE THESE!)
OAUTH_CLIENT_CREDENTIALS='{"identityProviderUrl":"https://auth.example.com/token","clientId":"prod-client","clientSecret":"prod-secret","scope":"api.read api.write"}'

# Set proper permissions
sudo chmod 600 /opt/simplehooks/environment.conf
```

#### 6.4: Load Environment Variables

For systemd services:

```bash
# Edit service file
sudo nano /etc/systemd/system/simplehooks-server.service

# Add EnvironmentFile
[Service]
EnvironmentFile=/opt/simplehooks/environment.conf
ExecStart=/opt/simplehooks/SimpleHooks.Server

# Reload systemd
sudo systemctl daemon-reload
```

For Windows Services, set via:
- System Properties → Advanced → Environment Variables
- Or use `setx` command for system-wide variables

### Step 7: Update Listener Configurations

#### 7.1: Keep Existing Listeners as Anonymous

No action needed! Existing listeners default to Type_Id = 1 (Anonymous).

#### 7.2: Configure New OAuth2 Listeners

```sql
-- Example: Add new listener with OAuth2 authentication
INSERT INTO ListenerDefinition(
    Name, Url, Timeout, TrialCount, RetrialDelay, 
    Type_Id, Type_Options, 
    CreateBy, ModifyBy, Notes
)
VALUES (
    'Secure Partner API', 
    'https://api.partner.com/webhooks/events', 
    5, 3, 1,
    2, 'OAUTH_CLIENT_CREDENTIALS',
    'system.admin', 'system.admin', 
    'OAuth2 authenticated webhook'
);
GO
```

#### 7.3: Convert Existing Listener to OAuth2

```sql
-- Update specific listener to use OAuth2
UPDATE ListenerDefinition
SET Type_Id = 2,
    Type_Options = 'OAUTH_CLIENT_CREDENTIALS',
    ModifyBy = 'admin',
    ModifyDate = GETUTCDATE()
WHERE Id = 5;  -- Replace with actual listener ID
GO
```

### Step 8: Start the Application

```bash
# Start SimpleHooks.Server
sudo systemctl start simplehooks-server

# Check status
sudo systemctl status simplehooks-server

# View logs
sudo journalctl -u simplehooks-server -f
```

### Step 9: Verify Deployment

#### 9.1: Check Server Startup Logs

Look for these messages in logs:

```
[INFO] DefinitionManager.LoadDefinitions - Started
[INFO] Loading ListenerTypes
[INFO] Loading ListenerDefinitions
[INFO] SetListenerPlugins - Started
[INFO] CreatePluginInstance - Path: listener-plugins/Anonymous/Anonymous.dll
[INFO] CreatePluginInstance - Path: listener-plugins/TypeA/TypeAListener.dll
[INFO] LoadDefinitions - Completed successfully
```

#### 9.2: Test Anonymous Plugin

```bash
# Trigger an event that uses Anonymous listener
curl -X POST http://localhost:5000/api/events \
  -H "Content-Type: application/json" \
  -d '{
    "eventDefinitionId": 1,
    "businessId": "test-12345",
    "eventData": "{\"test\":\"data\"}"
  }'

# Check logs for execution
# Should see: "Anonymous plugin executing call to..."
```

#### 9.3: Test TypeA Plugin

```bash
# Trigger an event that uses TypeA listener
curl -X POST http://localhost:5000/api/events \
  -H "Content-Type: application/json" \
  -d '{
    "eventDefinitionId": 2,
    "businessId": "test-oauth-12345",
    "eventData": "{\"test\":\"data\"}"
  }'

# Check logs for execution
# Should see:
# - "TypeA plugin executing call to..."
# - "Requesting bearer token from..." or "Using cached bearer token"
# - "Bearer token obtained successfully"
```

#### 9.4: Verify Database Records

```sql
-- Check listener instances were created
SELECT TOP 10 
    li.Id,
    li.ListenerDefinitionId,
    ld.Name,
    ld.Type_Id,
    lt.Name as PluginType,
    li.Status,
    li.CreateDate
FROM ListenerInstance li
JOIN ListenerDefinition ld ON li.ListenerDefinitionId = ld.Id
JOIN ListenerType lt ON ld.Type_Id = lt.Id
ORDER BY li.CreateDate DESC;

-- Check for any failed listeners
SELECT * FROM ListenerInstance
WHERE Status = 'Failed'
ORDER BY CreateDate DESC;
```

### Step 10: Monitoring and Maintenance

#### 10.1: Monitor Plugin Execution

```sql
-- Daily plugin usage
SELECT 
    lt.Name as PluginType,
    COUNT(*) as ExecutionCount,
    SUM(CASE WHEN li.Status = 'Succeeded' THEN 1 ELSE 0 END) as SuccessCount,
    SUM(CASE WHEN li.Status = 'Failed' THEN 1 ELSE 0 END) as FailureCount
FROM ListenerInstance li
JOIN ListenerDefinition ld ON li.ListenerDefinitionId = ld.Id
JOIN ListenerType lt ON ld.Type_Id = lt.Id
WHERE li.CreateDate >= DATEADD(day, -1, GETUTCDATE())
GROUP BY lt.Name;
```

#### 10.2: Check for Plugin Errors

```sql
-- Find listeners with high failure rates
SELECT 
    ld.Id,
    ld.Name,
    lt.Name as PluginType,
    COUNT(*) as TotalExecutions,
    SUM(CASE WHEN li.Status = 'Failed' THEN 1 ELSE 0 END) as FailureCount,
    CAST(SUM(CASE WHEN li.Status = 'Failed' THEN 1 ELSE 0 END) * 100.0 / COUNT(*) as DECIMAL(5,2)) as FailureRate
FROM ListenerInstance li
JOIN ListenerDefinition ld ON li.ListenerDefinitionId = ld.Id
JOIN ListenerType lt ON ld.Type_Id = lt.Id
WHERE li.CreateDate >= DATEADD(day, -7, GETUTCDATE())
GROUP BY ld.Id, ld.Name, lt.Name
HAVING SUM(CASE WHEN li.Status = 'Failed' THEN 1 ELSE 0 END) > 0
ORDER BY FailureRate DESC;
```

#### 10.3: Review Application Logs

```bash
# Search for plugin-related errors
sudo journalctl -u simplehooks-server | grep -i "plugin\|exception\|error"

# Monitor in real-time
sudo journalctl -u simplehooks-server -f | grep -i "plugin"
```

## Rollback Procedure

If issues occur after deployment:

### Step 1: Stop Application

```bash
sudo systemctl stop simplehooks-server
```

### Step 2: Restore Previous Version

```bash
# Restore from backup
rm -rf /opt/simplehooks
cp -r /opt/simplehooks.backup.YYYYMMDD_HHMMSS /opt/simplehooks
```

### Step 3: Rollback Database (if needed)

```sql
-- If you need to remove the new columns
ALTER TABLE ListenerDefinition DROP CONSTRAINT FK_ListenerDefinition_ListenerType;
ALTER TABLE ListenerDefinition DROP COLUMN Type_Id;
ALTER TABLE ListenerDefinition DROP COLUMN Type_Options;
DROP TABLE ListenerType;
GO
```

### Step 4: Restart Application

```bash
sudo systemctl start simplehooks-server
```

## Troubleshooting

### Issue: Server fails to start

**Check**:
1. Plugin DLLs are in correct locations
2. File permissions are correct
3. Database schema is updated
4. Environment variables are set

**Logs to review**:
```bash
sudo journalctl -u simplehooks-server --since "10 minutes ago"
```

### Issue: "No plugin instance found"

**Cause**: Plugin not loaded at startup

**Fix**:
1. Verify ListenerType records exist
2. Check plugin DLL paths are correct
3. Ensure DLLs are deployed
4. Restart server

### Issue: OAuth2 authentication fails

**Cause**: Environment variable or credentials issue

**Fix**:
1. Verify environment variable is set: `printenv | grep OAUTH`
2. Test credentials manually with curl
3. Check auth provider URL is accessible
4. Review TypeOptions matches environment variable name

## Security Checklist

- [ ] Plugin DLLs deployed to secure directory
- [ ] File permissions set correctly (744 or 755)
- [ ] Environment variables secured (600 permissions for config file)
- [ ] Different credentials per environment
- [ ] Production credentials not in version control
- [ ] Database access restricted
- [ ] Application runs as non-root user
- [ ] HTTPS enabled for auth provider URLs
- [ ] Logs don't contain credentials or tokens

## Post-Deployment Checklist

- [ ] All services started successfully
- [ ] No errors in startup logs
- [ ] Database schema updated
- [ ] Plugin DLLs deployed
- [ ] Environment variables set
- [ ] Anonymous plugin tested
- [ ] TypeA plugin tested (if used)
- [ ] Existing listeners still work
- [ ] New OAuth2 listeners work
- [ ] Monitoring alerts configured
- [ ] Documentation updated
- [ ] Team notified of deployment

## Support Contacts

- **Database Issues**: DBA Team
- **Server Access**: DevOps Team
- **Plugin Development**: Development Team
- **OAuth2 Credentials**: Security Team

## Additional Resources

- Plugin README: `code/listener-plugins/README.md`
- Implementation Summary: `PHASE2_PHASE3_IMPLEMENTATION_SUMMARY.md`
- Quick Start Guide: `PLUGIN_QUICK_START.md`
- Design Documentation: `docs/pluggable-listener-plan.md`

