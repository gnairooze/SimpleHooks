use SimpleHooks
go

create procedure dbo.ListenerInstance_Remove
@Id bigint,
@Timestamp timestamp
as
begin
	delete ListenerInstance
	where Id = @Id
	and [TimeStamp] = @Timestamp
end

go

