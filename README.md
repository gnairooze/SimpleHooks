# Simple-Hooks

[![Build and Publish](https://github.com/gnairooze/SimpleHooks/actions/workflows/build.yml/badge.svg?branch=main)](https://github.com/gnairooze/SimpleHooks/actions/workflows/build.yml)
[![Docker Image](https://img.shields.io/docker/v/gnairooze/simple-hooks?sort=semver&label=docker%20image)](https://hub.docker.com/r/gnairooze/simple-hooks)

version: 3.0.3

![high level diagram](simple-hooks-system-context.svg)

## Description

Simple-Hooks is a solution to empower your app with events, and let other APIs to subscribe to various events.
It posts the event data to every subscribed API.

All you have to do in your app is to trigger the event you want, by calling Simple-Hooks API.

Simple-Hooks will call all the subscribers, with the event data and retry on them in case of failure.

---

## Environment Variables Configuration

Both SimpleHooks.Web and SimpleHooks.Server projects support configuration through environment variables, which override the default values in `appsettings.json`. This is particularly useful for containerized deployments and different environments (development, staging, production).

### Supported Environment Variables

| Environment Variable | Description | Used By | Default/Example Value |
|---------------------|-------------|---------|----------------------|
| `SIMPLE_HOOKS_CONNECTIONSTRING_SIMPLE_HOOKS` | Connection string for the main SimpleHooks database | Web & Server | `Server=localhost;Database=SimpleHooks;Integrated Security=true;` |
| `SIMPLE_HOOKS_CONNECTIONSTRING_LOG` | Connection string for the logging database | Web & Server | `Server=localhost;Database=SimpleHooks_Log;Integrated Security=true;` |
| `SIMPLE_HOOKS_LOGGER_MIN_LOG_LEVEL` | Minimum logging level | Web & Server | `Debug` |
| `SIMPLE_HOOKS_LOGGER_FUNCTION` | Stored procedure name for logging | Web & Server | `SimpleHooks_Log_Add` |

### Additional Configuration for SimpleHooks.Server

The SimpleHooks.Server project has an additional configuration parameter that is set in `appsettings.json` and cannot be overridden by environment variables:

- **group-id**: An integer that determines which console app instance will process specific event instances. This enables parallel processing across multiple server instances. Each server instance should have a unique group ID that doesn't exceed the `MaxGroups` value set in the AppOption table.

Example `appsettings.json` for SimpleHooks.Server:
```json
{
  "connectionStrings": {
    "simpleHooks": "",
    "log": ""
  },
  "logger": {
    "min-log-level": "Debug",
    "function": "SimpleHooks_Log_Add"
  },
  "group-id": 1
}
```

### Notes
- Environment variables take precedence over `appsettings.json` values when both are present
- If an environment variable is not set or is empty, the application will use the value from `appsettings.json`
- For the logger minimum log level, valid values are: `Debug`, `Information`, `Warning`, `Error`
- Connection strings should be properly escaped when set as environment variables
- Urls key in SimpleHooks.Web `appsettings.json` should be set to `0.0.0.0` for docker container to work as this value is used to bind the web server to all network interfaces so accepting requests from any IP address but if it set to `localhost` or `127.0.0.1`, it will only accept requests from the same machine.
- In production environments, consider using secure secret management solutions instead of plain text environment variables for sensitive data like connection strings
- Containers examples exist in [simple-hooks-containers](https://github.com/gnairooze/simple-hooks-containers) repository.
- Docker images published in [simple-hooks](https://hub.docker.com/r/gnairooze/simple-hooks).

---

## How to Setup Simple-Hooks

### Prerequisites

1. dot net 8.0 framework
2. SQL Server with DB for Simple-Hooks storage. Run the scripts in the [repository path](/code/SQL).

### Components

Simple-Hooks contains 3 components:

1. Web API - SimpleHooks.Web - to trigger events -TriggerEventController, and to inquire about event instance status - EventInstanceController . You need to host it under web server that support dot net 8.0 (IIS, Apache2, Nginx ...)
2. Console Application - SimpleHooks.Server - to run in a scheduler (windows task scheduler or cron demon). you can make multiple instances of it to run in parallel. every instance should have a unique group id. group id is an integer number set in the `appsettings.json` file. iit should not exceed the max groups set in the app options table.
3. dotNET Standard 2.0 library - Trigger Event Helper - to trigger events from any .NET project.

---

## Configuration Steps

> The setup is currently using SQL server scripts to DB. It is planned in the future to have web site for configuration.

### Create databases

#### SimpleHooks Db

1. create a new database in your SQL server instance.
2. run the batch file [execute-operation-schema-data-sql.bat](/code/SQL/operation-db/execute-operation-schema-data-sql.bat) to create tables, stored procedures, and seed data.

##### SimpleHooks_Log Db

1. create a new database in your SQL server instance.
2. run the batch file [execute-log-schema-data-sql.bat](/code/SQL/log-db/execute-log-schema-data-sql.bat) to create tables, stored procedures, and seed data.

### Define Events

just add a unique name for the event.

### Define Listeners (Subscribers)

define the listener name, the URL to post the event data to, and the headers to be added to the HTTP request. You also define the timeout, the number of retries, and the period between each retry.

you will find a sample implementation of the listener in the [repository path](/code/SampleListener/SampleListenerAPI)

```sql
insert EventDefinition 
(
	Name, 
	Active, 
	CreateBy, 
	CreateDate, 
	ModifyBy, 
	ModifyDate,
	Notes
) 
values 
(
	'Test Event', 
	1, 
	'admin', 
	'2024-02-20', 
	'admin', 
	'2024-02-20',
	'test event to test the system'
);
```

### Subscribe Listeners to Events

associate the listeners to the events. You can have multiple listeners for the same event.

use `EventDefiinition.Id` created in the previous step.

```sql
insert ListenerDefinition 
(
	Name, 
	Active, 
	URL,
	Headers,
	Timeout,
	TrialCount,
	RetrialDelay,
	CreateBy, 
	CreateDate, 
	ModifyBy, 
	ModifyDate,
	Notes
)
values
(
	'sample api', 
	1, 
	'http://test.test.com/api',
	'content-type: application/json',
	60,
	3,
	1,
	'admin', 
	'2024-02-20', 
	'admin', 
	'2024-02-20',
	'save event data'
);
```

use `EventDeinition.Id` and `ListenerDefinition.Id` created in the previous steps.
```sql
insert EventDefinition_ListenerDefinition 
(
	EventDefinition_Id,
	ListenerDefinition_Id,
	Active,
	CreateBy,
	CreateDate,
	ModifyBy,
	ModifyDate,
	Notes
)
values
(
	1,
	1,
	1,
	'admin',
	'2024-02-20',
	'admin',
	'2024-02-20',
	'sample json to test event'
);
```

### set options
1. set top count of events to be processed in one run.
2. set the max number of groups to be processed in parallel.

```sql
insert AppOption (Category, [Name], [Value], CreateBy, ModifyBy) values ('GetNotProcessed', 'TopCount', '100', 'system.admin', 'system.admin')
insert AppOption (Category, [Name], [Value], CreateBy, ModifyBy) values ('GetNotProcessed', 'MaxGroups', '1', 'system.admin', 'system.admin')
```

---

## How to Trigger Events

you need to include the event definition id, the event data, reference name and reference value in the request body.
note that the event data should be a string. in the string, you will add the data in json format.

```json
{
	"EventDefinitionId": "1",
	"EventData": { "ClientId": "34544433", "ClientName": "John Doe", "ClientEmail": "john.doe@email.com" },
	"ReferenceName": "client.techId",
	"ReferenceValue": "abd123-1001"
}
```

---

## What JSON Data will be Posted to the Listener

simple hooks will post the listener definition headers and the event data along with the metadata of the event to the listener url respecting the listener definition timeout. and in case of failure, it will retry respecting the listener definition retries and retries delay.

```json
{
	"ClientId": "34544433",
	"ClientName": "John Doe",
	"ClientEmail": "john.doe@email.com",
	"simpleHooksMetadata": {
		"eventDefinitionId": 1,
		"eventDefinitionName": "Test Event 1",
		"eventBusinessId": "5d364e8f-c27c-4e3d-9889-41fc40ba78b2",
		"eventCreateDate": "2024-07-01T09:47:06.29",
		"eventReferenceName": "client.techId",
		"eventReferenceValue": "abd123-1001"
	}
}
```

---

## Database Documentation

### Metadata Columns

Metadata columns exist in every table for support purposes.

1. Id: the identity of the row.
2. CreateBy: the user who created the row.
3. CreateDate: when the row was created.
4. ModifyBy: the user last modified the row.
5. ModifyDate: when the row last modified.
6. Notes: notes about this row.

### Definition Tables

#### A. EventDefinition

1. Name: it is a unique name to identify the event.
2. Active: marks if the event is active or not. If the event is not active, it cannot be triggered.

#### B. Listener Definition

1. Name: the name of the subscriber / listener.
2. Active: marks if the listener is active or not. If the listener is not active, it will not be associated with the event(s) when triggered.
3. URL: the url of the listener to be called when processing the event.
4. Headers: the headers that will be added to the HTTP request when posting to the URL. multiple headers allowed. Each header on separate line. as the following sample:

	```headers
	content-type:application/json
	token:65yrbwiw752342kjhjljwhlsf
	```

5. Timeout: when the HTTP request will timeout. In minutes.
6. TrialCount: the max number of retries on the request if not succeeded to declare its failure.
7. RetrialDelay: the period between each retry. In minutes.

#### C. EventDefinition_ListenerDefinition

1. EventDefinition_Id: Id of event definition row.
2. ListenerDefinition_Id: Id of the related listener.
3. Active: marks if the relation between the event definition and listener definition is active. If the relation is not active, it will not be associated with the event when triggered.

### Instance Tables

#### D. EventInstance

1. EventDefinition_Id: Id of event definition row.
2. BusinessId: event instance Id to be communicated in the event data.
3. EventData: JSON data that will be post to all the subscribers / listeners when processing the events.
4. ReferenceName: name of the reference for that particular event instance. (example: "ClientId" in case the event definition is "Client Added").
5. ReferenceValue: value of the reference for that particular event instance. (example: "34544433" which is the value of the Client Id being added in "Client Added" event).
6. EventInstanceStatus_Id: Id of the status of the event instance.
7. Active: marks if the event instance is active. Only active event instance will be processed.
8. GroupId: it is used to process events in parallel. it determines which console app will process this event instance. the console app - with the same group id in its configuration file - will process this event instance.

#### E. ListenerInstance

1. EventInstance_Id: the Id of the related event instance.
2. ListenerDefinition_Id: the Id of the listener definition.
3. ListenerInstanceStatus_Id: Id of the status of the listener instance.
4. RemainingTrialCount: the remaining number of retrials that can be executed on this listener instance.
5. NextRun: when the next time this listener instance will be used in the post request.
6. Active: marks if the listener instance is active. Only active listeners are processed.

#### F. EventInstanceStatus

1. Name: name of the status. It can have one of the following values:  
1.1. InQueue | 1: waiting to be processed.  
1.2. Processing | 2: currently under processing.  
1.3. Hold | 4: on hold. used in support as a temporary status.  
1.4. Succeeded | 8: all listeners succeeded.  
1.5. Failed | 16: at least one of the listeners failed.  
1.6. Aborted | 32: aborted. used in support as a final status.  

#### G. ListenerInstanceStatus

1. Name: name of the status. It can have one of the following values:  
1.1. InQueue | 1: waiting to be processed.  
1.2. Processing | 2: currently under processing.  
1.3. Hold | 4: on hold. used in support as a temporary status.  
1.4. Succeeded | 8: this listener called successfully.  
1.5. Failed | 16: this listener failed after consuming all the retrials.  
1.6. Aborted | 32: aborted. used in support as a final status.  
1.7. WaitingForRetrial | 64: failed at least once, waiting for the next retrial.  

### Operation Tables

#### H. AppOptions

This table contains configurations of Simple-Hooks

1. Category: the category of the option.
2. Name: the name of the option.
3. Value: the value of the option.
4. Active: marks if the option is currently active.

##### _Simple-Hooks Options_

1. TopCount of the GetNotProcessed operation. It determines the number of event instance rows returns every time the operation of processing events started.
2. MaxGroups: the max number of groups to be processed in parallel.

#### I. SimpleHooks_Log

1. LogType: the Id of the type of the log.
2. Owner: who added the log.
3. Machine: the machine related to the log.
4. Location: the assembly location on the machine.
5. Operation: name of the operation.
6. Step: name of the step in the operation.
7. Counter: the number of the log in the same step in the same operation. Used for ordering the logs in the same step or different steps.
8. Correlation: Id to unify different logs in the same step or the same operations.
9. CodeReference: Namespace, class name, method name,...
10. ReferenceName: name of a key reference related to the log.
11. ReferenceValue: value of the key reference related to the log.
12. NotesA: large text to describe data. Most of the time used to log parameters' values.
13. NotesB: large text to describe data. Most of the time used to log error details.
14. Duration: when calculation method execution duration.

#### J. LogType

1. Name: name of the type of the log.

_Log Types_

1. Debug | 1
2. Information | 2
3. Warning | 4
4. Error | 8

## SimpleHooks.Assist Library

`SimpleHooks.Assist` is a .NET Standard 2.0 class library that provides a simple way to trigger events, check event instance status and load definitions in the SimpleHooks.Web API and SimpleHooks.AuthApi by calling their relative controllers' actions.

### Features
- Easy integration with any .NET project (Standard 2.0 compatible)
- Async methods for non-blocking event triggering
- Supports sending event data as either an object or a JSON string

### Usage Example

please refer to the project SimpleHooks.TestConsole for code samples to use the library.

---

## LoadDefinitions API Endpoint

The `LoadDefinitions` endpoint is used to reload event and listener definitions, as well as application options, from the database into the running Simple-Hooks system. This is useful if you have made changes to event definitions, listeners, or app options in the database and want those changes to take effect without restarting the service.

### Endpoint
- **URL:** `/api/Definitions/load-definitions`
- **Method:** `POST`

### Request
No request body is required.

### Response
- **200 OK**: Returns the current application options as loaded from the database.
- **Error**: Returns an error if the definitions could not be loaded.

#### Example Request
```http
POST /api/Definitions/load-definitions HTTP/1.1
Host: your-simplehooks-url
Content-Type: application/json
```

#### Example Response
```json
[
  {
    "Category": "Processing",
    "Name": "TopCount",
    "Value": "100",
    "Active": true
  },
  {
    "Category": "General",
    "Name": "EnableLogging",
    "Value": "true",
    "Active": true
  }
]
```

### Notes
- Use this endpoint after making changes to event/listener definitions or app options in the database.
- If the reload fails, the endpoint will return an error with status 500 and a message.

---

## Get Event Instance Status by Business ID API Endpoint

### Description
Returns a brief status of the event instance associated with the provided businessId. If no event instance is found, a 404 response is returned.

### Query Parameters

| Name        | Type   | Required | Description |
|-------------|--------|----------|-----------------------------------| 
| businessId  | Guid   | Yes      | The business ID of the event      |

### Sample Request

```http
GET /api/EventInstance/get-status?businessId=123e4567-e89b-12d3-a456-426614174000
```

### Sample Response (200 OK)
```json 
{
  "id": 42,
  "businessId": "123e4567-e89b-12d3-a456-426614174000",
  "status": 8
}
```

the status can be one of numerical values of EventInstanceStatus (see above).

### Sample Response (404 Not Found)
```json
{
  "error": "no event instance found for business id = 67b26931-e512-4a6d-9317-98c9d32dd6cf"
}
```

## SimpleHooks Authentication

simple hooks support authentication by openiddict in its SimpleHooks.AuthApi asp.net web api. you can configure it in evironment variables or in the appsettings.json file by setting the section of IdentityServer.

SimpleHooks.Web asp.net web api remains anonymous to be called by any client that has access to it.

it is recommended to use SimpleHooks.AuthApi.

### Environment Variables for IdentityServer Configuration

| Environment Variable | Description | Used By | Default/Example Value |
|---------------------|-------------|---------|----------------------|
| `SIMPLE_HOOKS_IDENTITYSERVER_AUTHORITY` | the base url for the identity provider | Web | `https://identity.dev.test:8091` |
| `SIMPLE_HOOKS_IDENTITYSERVER_AUDIENCE` | name of the resource api in openiddict configuration | Web | `identity_simplehooks_api` |
| `SIMPLE_HOOKS_IDENTITYSERVER_INTROSPECTIONENDPOINT` | introspect endpoint | Web | `https://identity.dev.test:8091/connect/introspect` |
| `SIMPLE_HOOKS_IDENTITYSERVER_CLIENTID` | api client id | Web | `my-resource-api-client` |
| `SIMPLE_HOOKS_IDENTITYSERVER_CLIENTSECRET` | api client secret | Web | `your-secure-secret-here` |

### appsettings.json Configuration for IdentityServer

```json
  "IdentityServer": {
    "Authority": "https://identity.dev.test:8091",
    "Audience": "identity_simplehooks_api",
    "IntrospectionEndpoint": "https://identity.dev.test:8091/connect/introspect",
    "ClientId": "my-resource-api-client",
    "ClientSecret": "your-secure-secret-here"
  }
```

### OpenIddict Configuration

configure openiddict using [simple identity server cli](https://github.com/gnairooze/simple-identity-server/releases/download/0.1/SimpleIdentityServer.CLI-v0.1-win64.zip):
1. add the simplehooks api scopes
	```sh
	./SimpleIdentityServer.CLI.exe scope add --name "simplehooks_api" --display-name "resource simple hooks api" --resources "simplehooks_api"

	./SimpleIdentityServer.CLI.exe scope add --name "simplehooks_api.trigger_event" --display-name "trigger event to simple hooks api" --resources "simplehooks_api"

	./SimpleIdentityServer.CLI.exe scope add --name "simplehooks_api.load_definitions" --display-name "load definitions from simple hooks api" --resources "simplehooks_api"

	./SimpleIdentityServer.CLI.exe scope add --name "simplehooks_api.get_event_instance_status" --display-name "get event instance status from simple hooks api" --resources "simplehooks_api"
	```

2. simplehooks api to call introspect endpoint to validate the token when called by client applications that passes their tokens.the simplehooks api client_id must be exactly the same as the resource in the scope definition.

	```sh
	./SimpleIdentityServer.CLI.exe app add --client-id "simplehooks_api" --client-secret "P@ssw0rdP@ssw0rd" --display-name "simple-hooks auth api" --permissions "ept:introspection"
	```

3. client application to trigger events and check status
	```sh
	./SimpleIdentityServer.CLI.exe app add --client-id "postman-client-trigger-event" --client-secret "P@ssw0rdP@ssw0rd" --display-name "simple-hooks api postman client to trigger event" --permissions "ept:token" --permissions "ept:introspection" --permissions "gt:client_credentials" --permissions "scp:simplehooks_api.trigger_event"
	```

4. client to check status
	```sh
	./SimpleIdentityServer.CLI.exe app add --client-id "postman-client-check-status" --client-secret "P@ssw0rdP@ssw0rd" --display-name "simple-hooks api postman client to check status" --permissions "ept:token" --permissions "ept:introspection" --permissions "gt:client_credentials" --permissions "scp:simplehooks_api.get_event_instance_status"
	```

5. client to load definitions - mostly support admin
	```sh
	./SimpleIdentityServer.CLI.exe app add --client-id "postman-client-load-defs" --client-secret "P@ssw0rdP@ssw0rd" --display-name "simple-hooks api postman client to load definitions" --permissions "ept:token" --permissions "ept:introspection" --permissions "gt:client_credentials" --permissions "scp:simplehooks_api.load_definitions"
	```

6. support client have all permissions for testing
	```sh
	./SimpleIdentityServer.CLI.exe app add --client-id "postman-client-admin" --client-secret "P@ssw0rdP@ssw0rd" --display-name "simple-hooks api postman client admin" --permissions "ept:token" --permissions "ept:introspection" --permissions "gt:client_credentials" --permissions "scp:simplehooks_api.trigger_event" --permissions "scp:simplehooks_api.load_definitions" --permissions "scp:simplehooks_api.get_event_instance_status"
	```
