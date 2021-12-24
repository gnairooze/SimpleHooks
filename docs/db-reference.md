---
layout: default
title: Database Reference
---

## Metadata Columns

Metadata columns exist in every table for support purposes.

1. Id: the identitiy of the row.
2. CreateBy: the user who created the row.
3. CreateDate: when the row was created.
4. ModifyBy: the user last modified the row.
5. ModifyDate: when the row last modified.
6. Notes: notes about this row.

## Definition Tables

### A. EventDefinition

1. Name: carries the name of the event.
2. Active: marks if the event is active or not. If the event is not active, it cannot be triggered.

### B. Listener Definition

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

### C. EventDefinition_ListenerDefinition

1. EventDefinition_Id: Id of event definition row.
2. ListenerDefinition_Id: Id of the related listener.
3. Active: marks if the relation between the event definition and listener definition is active. If the relation is not active, it will not be associated with the event when triggered.

## Instance Tables

### D. EventInstance

1. EventDefinition_Id: Id of event definition row.
2. BusinessId: event instance Id to be communicated in the event data.
3. EventData: JSON data that will be post to all the subscribers / listeners when processing the events.
4. ReferenceName: name of the reference for that particular event instance. (example: "ClientId" in case the event definition is "Client Added").
5. ReferenceValue: value of the reference for that particular event instance. (example: "34544433" which is the value of the Client Id being added in "Client Added" event).
6. EventInstanceStatus_Id: Id of the status of the event instance.
7. Active: marks if the event instance is active. Only active event intance will be processed.

### E. ListenerInstance

1. EventInstance_Id: the Id of the related event instance.
2. ListenerDefinition_Id: the Id of the lsitener definition.
3. ListenerInstanceStatus_Id: Id of the status of the listener instance.
4. RemainingTrialCount: the remaining number of retrials that can be executed on this listener instance.
5. NextRun: when the next time this listener instance will be used in the post request.
6. Acitve: marks if the listener instance is active. Only active listeners are processed.

### F. EventInstanceStatus

1. Name: name of the status. It can have one of the following values:  
1.1. InQueue: waiting to be processed.  
1.2. Processing: currently under processing.  
1.3. Hold: on hold. used in support as a temporary status.  
1.4. Succeeded: all listeners succeeded.  
1.5. Failed: at least one of the listeners failed.  
1.6. Aborted: aborted. used in support as a final status.  

### G. ListenerInstanceStatus

1. Name: name of the status. It can have one of the following values:  
1.1. InQueue: waiting to be processed.  
1.2. Processing: currently under processing.  
1.3. Hold: on hold. used in support as a temporary status.  
1.4. Succeeded: this listener called successfully.  
1.5. Failed: this listener failed after consuming all the retrials.  
1.6. Aborted: aborted. used in support as a final status.  
1.7. WaitingForRetrial: failed at least once, waiting for the next retrial.  

## Operation Tables

### H. AppOptions

This table contains configurations of Simple-Hooks

1. Category: the category of the option.
2. Name: the name of the option.
3. Value: the value of the option.
4. Active: marks if the option is currently active.

#### _Simple-Hooks Options_

1. TopCount of the GetNotProcessed operation. It determines the number of event instance rows returns every time the opertation of processing events started.

### I. SimpleHooks_Log

1. LogType: the Id of the type of the log.
2. Owner: who added the log.
3. Machine: the machine related to the log.
4. Location: the assembly location on the machine.
5. Operation: name of the operation.
6. Step: name of the step in the operation.
7. Counter: the number of the log in the same step in the same operation. Used for ordering the logs in the same step or different steps.
8. Correlation: Id to unidfy different logs in the same step or the same operations.
9. CodeReference: Namespace, class name, method name,...
10. ReferenceName: name of a key reference related to the log.
11. ReferenceValue: value of the key reference related to the log.
12. NotesA: large text to describe data. Most of the time used to log parameters' values.
13. NotesB: large text to describe data. Most of the time used to log error details.
14. Duration: when calculation method execution duration.

### J. LogType

1. Name: name of the type of the log.

#### _Log Types_

1. Error
2. Warning
3. Information
4. Debug

### Links

[Home](/SimpleHooks/index)
