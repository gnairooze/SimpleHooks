use SimpleHooks
go

select 
	EventInstanceStatus_Name = EventInstanceStatus.Name,
	EventInstance.*
from EventInstance
inner join EventInstanceStatus
	on EventInstance.EventInstanceStatus_Id = EventInstanceStatus.Id
where EventInstance.CreateDate > '2025-04-15'
and EventInstance.Id = 10004

select
	ListenerInstanceStatus_Name = ListenerInstanceStatus.Name,
	ListenerInstance.*
from ListenerInstance
inner join ListenerInstanceStatus
	on ListenerInstance.ListenerInstanceStatus_Id = ListenerInstanceStatus.Id
where ListenerInstance.EventInstance_Id = 10005


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
from SimpleHooks_Log
where CreateDate > '2025-04-14'

select
	*
from sampledb.dbo.SampleModels
where CreatedDate > '2025-04-14'
