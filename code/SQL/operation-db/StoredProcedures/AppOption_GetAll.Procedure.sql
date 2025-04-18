use SimpleHooks
go

create procedure dbo.AppOption_GetAll
as
begin
	select
		Id,
		Category,
		[Name],
		[Value],
		Active,
		CreateBy,
		CreateDate,
		ModifyBy,
		ModifyDate,
		Notes
	from AppOption
end

go
