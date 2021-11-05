use SimpleHooks
go

create procedure dbo.ListenerInstance_Edit
@Active bit,
@ModifyBy nvarchar(50),
@ModifyDate datetime2,
@Notes nvarchar(1000),
@EventInstance_Id bigint,
@ListenerDefinition_Id bigint,
@ListenerInstanceStatus_Id int,
@RemainingTrialCount int,
@NextRun datetime2,
@Id bigint
as
begin
	update ListenerInstance
	set EventInstance_Id = @EventInstance_Id,
		ListenerDefinition_Id = @ListenerDefinition_Id,
		ListenerInstanceStatus_Id = @ListenerInstanceStatus_Id,
		RemainingTrialCount = @RemainingTrialCount,
		NextRun = @NextRun,
		Active = @Active,
		ModifyBy = @ModifyBy,
		ModifyDate = @ModifyDate,
		Notes = @Notes
	where Id = @Id
end

go

