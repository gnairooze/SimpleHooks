use SimpleHooks
go

create procedure dbo.ListenerInstance_Remove
@Id bigint
as
begin
	delete ListenerInstance
	where Id = @Id
end

go

