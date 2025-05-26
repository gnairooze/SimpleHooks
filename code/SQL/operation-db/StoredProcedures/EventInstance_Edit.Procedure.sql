use SimpleHooks
go

create procedure dbo.EventInstance_Edit
@Active bit,
@BusinessId uniqueidentifier,
@ModifyBy nvarchar(50),
@ModifyDate datetime2,
@Notes nvarchar(1000),
@EventDefinition_Id bigint,
@EventData nvarchar(max),
@ReferenceName nvarchar(50),
@ReferenceValue nvarchar(100),
@EventInstanceStatus_Id int,
@GroupId int = 1,
@Id bigint
as
begin
	update EventInstance
	set EventDefinition_Id = @EventDefinition_Id,
		BusinessId = @BusinessId,
		[EventData] = @EventData,
		ReferenceName = @ReferenceName,
		ReferenceValue = @ReferenceValue,
		EventInstanceStatus_Id = @EventInstanceStatus_Id,
		Active = @Active,
		ModifyBy = @ModifyBy,
		ModifyDate = @ModifyDate,
		Notes = @Notes,
		GroupId = @GroupId
	where Id = @Id
end

go

