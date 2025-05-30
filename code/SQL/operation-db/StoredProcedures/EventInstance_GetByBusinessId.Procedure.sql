use SimpleHooks
go

create procedure dbo.EventInstance_GetByBusinessId
@BusinessId uniqueidentifier
as
begin
	declare @eventInstance_Id bigint
	select 
		@eventInstance_Id = Id 
	from dbo.EventInstance with (nolock) 
	where BusinessId = @BusinessId

	select
		EventInstance.Id,
		EventInstance.EventDefinition_Id,
		EventInstance.BusinessId,
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
		EventInstance.GroupId,
		EventInstance.TimeStamp
	from EventInstance with (nolock)
	where EventInstance.Id = @eventInstance_Id

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
	where ListenerInstance.EventInstance_Id = @eventInstance_Id
	order by ListenerInstance.Id
end

go
