use SimpleHooks
go

create procedure dbo.ListenerDefinition_GetAll
as
begin
	select
		Id,
		[Name],
		Active,
		CreateBy,
		CreateDate,
		ModifyBy,
		ModifyDate,
		Notes,
		[URL],
		Headers,
		[Timeout],
		TrialCount,
		RetrialDelay,
		Type_Id,
		Type_Options
	from ListenerDefinition
end

go
