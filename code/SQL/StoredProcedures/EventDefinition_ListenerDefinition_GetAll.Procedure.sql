use SimpleHooks
go

create procedure dbo.EventDefinition_ListenerDefinition_GetAll
as
begin
	select
		Id,
		Active,
		CreateBy,
		CreateDate,
		ModifyBy,
		ModifyDate,
		Notes,
		EventDefinition_Id,
		ListenerDefinition_Id
	from EventDefinition_ListenerDefinition
end

go
