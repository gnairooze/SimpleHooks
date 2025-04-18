use SimpleHooks
go

create procedure dbo.ListenerInstance_GetByEventInstance_Id
@EventInstance_Id bigint
as
begin
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
	where ListenerInstance.EventInstance_Id = @EventInstance_Id
	order by ListenerInstance.Id
end

go
