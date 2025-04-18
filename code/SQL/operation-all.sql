use SimpleHooks
go

select 
	EventInstanceStatus_Name = EventInstanceStatus.Name,
	EventInstance.*
from EventInstance
inner join EventInstanceStatus
	on EventInstance.EventInstanceStatus_Id = EventInstanceStatus.Id
where 1=1
and EventInstance.CreateDate > '2025-04-15'
and EventInstance.Id = 10004
and EventInstance.EventInstanceStatus_Id = 16 -- failed

select
	*
from EventInstanceStatus
/*
Id	Name
1	InQueue
2	Processing
4	Hold
8	Succeeded
16	Failed
32	Aborted
*/

select
	ListenerInstanceStatus_Name = ListenerInstanceStatus.Name,
	ListenerInstance.*
from ListenerInstance
inner join ListenerInstanceStatus
	on ListenerInstance.ListenerInstanceStatus_Id = ListenerInstanceStatus.Id
where ListenerInstance.EventInstance_Id = 10005
and ListenerInstance.ListenerInstanceStatus_Id = 16 -- failed

select
	*
from ListenerInstanceStatus

/*
Id	Name
1	InQueue
2	Processing
4	Hold
8	Succeeded
16	Failed
32	Aborted
64	WaitingForRetrial
*/


select
	*
from EventDefinition

select
	*
from EventDefinition_ListenerDefinition

select
	*
from ListenerDefinition

/*
update ListenerDefinition
set URL = 'https://simplehookssamplelistener.dev.local:5011/api/sample'
where id = 1

--https://simplehookssamplelistener.dev.local:5011/
--http://localhost:8081/sample-listener-api/api/sample
*/

select
	*
from SimpleHooks_Log_DB.dbo.SimpleHooks_Log
where 1=1
and 1=1
and CreateDate > '2025-04-14'
and Id > 10473
and Step = 'events count'

select
	*
from sampledb.dbo.SampleModels
where CreatedDate > '2025-04-14'

exec dbo.Support_RetryFailedEvent 
	@EventInstance_Id = 1, 
	@Read_Only = 1
