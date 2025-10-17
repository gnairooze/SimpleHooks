use SimpleHooks
go

create procedure dbo.ListenerType_GetAll
as
begin
	select
		Id,
		[Name],
		[Path],
		CreateBy,
		CreateDate,
		ModifyBy,
		ModifyDate,
		Notes
	from ListenerType
end

go

