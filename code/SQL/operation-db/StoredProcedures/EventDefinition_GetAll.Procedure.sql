use SimpleHooks
go

create procedure dbo.EventDefinition_GetAll
as
begin
	select
		Id,
		Name,
		Active,
		CreateBy,
		CreateDate,
		ModifyBy,
		ModifyDate,
		Notes
	from EventDefinition
end

go
