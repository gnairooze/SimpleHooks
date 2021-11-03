use SimpleHooks
go

create procedure dbo.EventInstance_GetNotProcessed
@Date Datetime2
as
begin
	declare @top_count int

	declare @listenerInstances table
	(
		Id bigint,
		EventInstance_Id bigint
	)

	declare @eventInstances table
	(
		Id bigint
	)

	select 
		@top_count = convert(int, Value)
	from AppOption with (nolock)
	where Category = 'GetNotProcessed' 
	and Name = 'TopCount'

	insert @listenerInstances
	(Id, EventInstance_Id)
	select
		ListenerInstance.Id,
		ListenerInstance.EventInstance_Id
	from ListenerInstance with (nolock)
	inner join EventInstance with (nolock)
		on ListenerInstance.EventInstance_Id = EventInstance.Id
	where 1=1
	and EventInstance.Active = 1
	and EventInstance.EventInstanceStatus_Id in (1,2) -- InQueue = 1, Processing = 2,
	and ListenerInstance.Active = 1
	and ListenerInstance.ListenerInstanceStatus_Id in (1,64) --InQueue = 1, WaitingForRetrial = 64
	and ListenerInstance.NextRun <= @Date

	insert @eventInstances
	(Id)
	select distinct top (@top_count)
		EventInstance_Id
	from @listenerInstances
	order by EventInstance_Id

	select
		EventInstance.Id,
		EventInstance.EventDefinition_Id,
		EventInstance.EventData,
		EventInstance.ReferenceName,
		EventInstance.ReferenceValue,
		EventInstance.EventInstanceStatus_Id,
		EventInstance.Active,
		EventInstance.CreateBy,
		EventInstance.CreateDate,
		EventInstance.ModifyBy,
		EventInstance.ModifyDate,
		EventInstance.Notes,
		EventInstance.TimeStamp
	from EventInstance with (nolock)
	inner join @eventInstances eventInstanceIds
		on EventInstance.Id = eventInstanceIds.Id
	order by EventInstance.Id

	select
		ListenerInstance.Id,
		ListenerInstance.EventInstance_Id,
		ListenerInstance.ListenerDefinition_Id,
		ListenerInstance.ListenerInstanceStatus_Id,
		ListenerInstance.RemainingTrialCount,
		ListenerInstance.NextRun,
		ListenerInstance.Active,
		ListenerInstance.CreateBy,
		ListenerInstance.CreateDate,
		ListenerInstance.ModifyBy,
		ListenerInstance.ModifyDate,
		ListenerInstance.Notes,
		ListenerInstance.TimeStamp
	from ListenerInstance with (nolock)
	inner join @listenerInstances listenerInstanceIds
		on ListenerInstance.Id = listenerInstanceIds.Id
	inner join @eventInstances eventInstanceIds
		on listenerInstanceIds.EventInstance_Id = eventInstanceIds.Id
	order by ListenerInstance.Id
end

go
