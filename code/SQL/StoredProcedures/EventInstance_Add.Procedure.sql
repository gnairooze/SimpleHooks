use SimpleHooks
go

create procedure dbo.EventInstance_Add
@Active bit,
@BusinessId uniqueidentifier,
@CreateBy nvarchar(50),
@CreateDate datetime2,
@ModifyBy nvarchar(50),
@ModifyDate datetime2,
@Notes nvarchar(1000),
@EventDefinition_Id bigint,
@EventData nvarchar(max),
@ReferenceName nvarchar(50),
@ReferenceValue nvarchar(100),
@EventInstanceStatus_Id int,
@Id bigint out
as
begin
	insert EventInstance
	(
		EventDefinition_Id,
		BusinessId,
		[EventData],
		ReferenceName,
		ReferenceValue,
		EventInstanceStatus_Id,
		Active,
		CreateBy,
		CreateDate,
		ModifyBy,
		ModifyDate,
		Notes
	)
	values
	(
		@EventDefinition_Id,
		@BusinessId,
		@EventData,
		@ReferenceName,
		@ReferenceValue,
		@EventInstanceStatus_Id,
		@Active,
		@CreateBy,
		@CreateDate,
		@ModifyBy,
		@ModifyDate,
		@Notes
	)

	set @Id = SCOPE_IDENTITY()
end

go

