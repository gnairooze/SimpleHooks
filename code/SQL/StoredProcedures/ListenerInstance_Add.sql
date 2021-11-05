use SimpleHooks
go

create procedure dbo.ListenerInstance_Add
@Active bit,
@CreateBy nvarchar(50),
@CreateDate datetime2,
@ModifyBy nvarchar(50),
@ModifyDate datetime2,
@Notes nvarchar(1000),
@EventInstance_Id bigint,
@ListenerDefinition_Id bigint,
@ListenerInstanceStatus_Id int,
@RemainingTrialCount int,
@NextRun datetime2,
@Id bigint out,
@Timestamp timestamp out
as
begin
	insert ListenerInstance
	(
		EventInstance_Id,
		ListenerDefinition_Id,
		ListenerInstanceStatus_Id,
		RemainingTrialCount,
		NextRun,
		Active,
		CreateBy,
		CreateDate,
		ModifyBy,
		ModifyDate,
		Notes
	)
	values
	(
		@EventInstance_Id,
		@ListenerDefinition_Id,
		@ListenerInstanceStatus_Id,
		@RemainingTrialCount,
		@NextRun,
		@Active,
		@CreateBy,
		@CreateDate,
		@ModifyBy,
		@ModifyDate,
		@Notes
	)

	set @Id = SCOPE_IDENTITY()

	select @Timestamp = [TimeStamp] from ListenerInstance where Id = @Id
end

go

