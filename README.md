# Simple-Hooks

![high level diagram](simple-hooks-system-context.svg)

## Description

Simple-Hooks is a solution to empower your app with events, and let other APIs to subscribe to various events.
It posts the event data to every subscribed API.

All you have to do in your app is to trigger the event you want, by calling Simple-Hooks API.

Simple-Hooks will call all the subscribers, with the event data and retry on them in case of failure.

---

## How to Setup Simple-Hooks

### Prerequisites

1. dot net 8.0 framework
2. SQL Server with DB for Simple-Hooks storage. Run the scripts in the [repository path](/code/SQL).

### Components

Simple-Hooks contains 2 components:

1. Web API to trigger events. You need to host it under web server that support dot net 8.0 (IIS, Apache2, Nginx ...)
2. Console Application to run in a scheduler (windows task scheduler or cron demon).

---

## Configuration Steps

> The setup is currently using SQL server scripts to DB. It is planned in the future to have web site for configuration.

### Define Events

just add a unique name for the event.

### Define Listeners (Subscribers)

define the listener name, the URL to post the event data to, and the headers to be added to the HTTP request. You also define the timeout, the number of retries, and the period between each retry.

you will find a sample implementation of the listener in the [repository path](/code/SampleListener/SampleListenerAPI)

### Subscribe Listeners to Events

associate the listeners to the events. You can have multiple listeners for the same event.

---

## How to Trigger Events

you need to include the event definition id, the event data, reference name and reference value in the request body.
note that the event data should be a string. in the string, you will add the data in json format.

```json
{
	"EventDefinitionId": "1",
	"EventData": "{ 'ClientId': '34544433', 'ClientName': 'John Doe', 'ClientEmail': 'john.doe@email.com' }",
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
6. TrialCount: the max number of retries on the request if not succeeded to decalre its failure.
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
