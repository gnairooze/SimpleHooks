use SimpleHooks
go

create procedure dbo.Support_RetryFailedEvent
@EventInstance_Id bigint, 
@Read_Only bit
as
begin
	--retry failed listeners of a failed event

	/* -- for testing
	declare @EventInstance_Id bigint, @Read_Only bit
	set @EventInstance_Id = 10008
	set @Read_Only = 0
	*/

	declare @Retrial_Utc_Time datetime2
	set @Retrial_Utc_Time = GETUTCDATE()

	select
		Comment = 'ListenerInstance - Before',
		*
	from ListenerInstance
	where 1=1
	and ListenerInstance.EventInstance_Id = @EventInstance_Id
	and ListenerInstance.ListenerInstanceStatus_Id = 16 -- failed

	select
		Comment = 'EventInstance - Before',
		*
	from EventInstance
	where 1=1
	and EventInstance.Id = @EventInstance_Id
	and EventInstance.EventInstanceStatus_Id = 16 -- failed

	if @Read_Only is null
	begin
		set @Read_Only = 1
	end

	if @Read_Only = 1
	begin
		select
			Comment = 'Operation aborted because it is read only.'
		
		return
	end

	begin tran
		update ListenerInstance
		set ListenerInstanceStatus_Id = 64, -- Waiting For Retrial
			NextRun = @Retrial_Utc_Time,
			RemainingTrialCount = 1,
			ModifyDate = @Retrial_Utc_Time,
			ModifyBy = 'support'
		where 1=1
		and ListenerInstance.EventInstance_Id = @EventInstance_Id
		and ListenerInstance.ListenerInstanceStatus_Id = 16 -- failed

		update EventInstance
		set EventInstanceStatus_Id = 2, -- Processing
			ModifyDate = @Retrial_Utc_Time,
			ModifyBy = 'support'
		where 1=1
		and EventInstance.Id = @EventInstance_Id
		and EventInstance.EventInstanceStatus_Id = 16 -- failed
	commit tran

	select
		Comment = 'ListenerInstance - After',
		*
	from ListenerInstance
	where 1=1
	and ListenerInstance.EventInstance_Id = @EventInstance_Id
	and ListenerInstance.ListenerInstanceStatus_Id = 64 -- Waiting For Retrial

	select
		Comment = 'EventInstance - After',
		*
	from EventInstance
	where 1=1
	and EventInstance.Id = @EventInstance_Id
	and EventInstance.EventInstanceStatus_Id = 2 -- Processing

	select
		Comment = 'Operation completed.'
end

go
