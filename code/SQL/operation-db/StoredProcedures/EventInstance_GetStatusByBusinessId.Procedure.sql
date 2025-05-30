use SimpleHooks
go

create procedure dbo.EventInstance_GetStatusByBusinessId
@BusinessId uniqueidentifier
as
begin
	select
		EventInstance.Id,
		EventInstance.BusinessId,
		EventInstance.EventInstanceStatus_Id
	from EventInstance with (nolock)
	where EventInstance.BusinessId = @BusinessId
end

go
