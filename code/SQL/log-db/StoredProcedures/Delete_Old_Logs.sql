use [SimpleHooks_Log_DB]
go

create procedure [dbo].[Delete_Old_Logs]
as
begin
	declare @read_only bit = 0
	declare @retention_days int = 7
	declare @batch_count int = 1000

	declare @date date = dateadd(d, @retention_days * -1, getdate())

	declare @count int = 0

	select
		@count = count(Id)
		from dbo.SimpleHooks_Log with (nolock)
		where CreateDate < @date

	print 'remaining row count = ' + convert(varchar, @count)

	if @read_only = 1
	begin
		print 'read only mode'
		return;
	end

	while @count > 0
	begin
		delete dbo.SimpleHooks_Log
			where Id in
			(
				select top (@batch_count) Id
				from dbo.SimpleHooks_Log with (nolock)
				where CreateDate < @date
				order by Id asc
			)

		select
			@count = count(Id)
			from dbo.SimpleHooks_Log with (nolock)
			where CreateDate < @date

		print 'remaining row count = ' + convert(varchar, @count)
	end
end
go

