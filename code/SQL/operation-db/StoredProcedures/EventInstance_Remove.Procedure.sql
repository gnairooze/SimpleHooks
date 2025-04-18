use SimpleHooks
go

create procedure dbo.EventInstance_Remove
@Id bigint
as
begin
	delete EventInstance
	where Id = @Id
end

go

