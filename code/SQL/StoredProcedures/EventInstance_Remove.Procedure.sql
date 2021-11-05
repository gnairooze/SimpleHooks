use SimpleHooks
go

create procedure dbo.EventInstance_Remove
@Id bigint,
@Timestamp timestamp
as
begin
	delete EventInstance
	where Id = @Id
	and [TimeStamp] = @Timestamp
end

go

