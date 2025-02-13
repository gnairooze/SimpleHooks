DECLARE @path NVARCHAR(512) = N'/var/opt/mssql/data/'
declare @ServerName varchar(100) = '.'
declare @DatabaseName varchar(100) = 'SimpleHooks'
declare @Username varchar(100) = 'sa'
declare @Password varchar(100) = 'sample@Strong23Password'

DECLARE @cmd NVARCHAR(1024)
DECLARE @file NVARCHAR(255)

-- Create a temporary table to hold the file names
declare @Files table 
(
	Id int identity(1,1),
	FileName NVARCHAR(255)
)

-- Insert file names into the temporary table
INSERT INTO @Files
(FileName)
values
('Tables/01-LogTypes.Table.sql'),
('Tables/02-SimpleHooks_Log.Table.sql'),
('Tables/03-dbo.ListenerInstanceStatus.Table.sql'),
('Tables/04-dbo.ListenerDefinition.Table.sql'),
('Tables/05-dbo.EventInstanceStatus.Table.sql'),
('Tables/06-dbo.EventDefinition.Table.sql'),
('Tables/07-dbo.EventDefinition_ListenerDefinition.Table.sql'),
('Tables/08-dbo.EventInstance.Table.sql'),
('Tables/09-dbo.ListenerInstance.Table.sql'),
('Tables/10-dbo.AppOption.Table.sql'),
('StoredProcedures/AppOption_GetAll.Procedure.sql'),
('StoredProcedures/EventDefinition_GetAll.Procedure.sql'),
('StoredProcedures/EventDefinition_ListenerDefinition_GetAll.Procedure.sql'),
('StoredProcedures/EventInstance_Add.Procedure.sql'),
('StoredProcedures/EventInstance_Edit.Procedure.sql'),
('StoredProcedures/EventInstance_GetNotProcessed.Procedure.sql'),
('StoredProcedures/EventInstance_Remove.Procedure.sql'),
('StoredProcedures/ListenerDefinition_GetAll.Procedure.sql'),
('StoredProcedures/ListenerInstance_Add.sql'),
('StoredProcedures/ListenerInstance_Edit.sql'),
('StoredProcedures/ListenerInstance_GetByEventInstance_Id.Procedure.sql'),
('StoredProcedures/ListenerInstance_Remove.sql'),
('StoredProcedures/SimpleHooks_Log_Add.sql'),
('Data/AppOption-Data.sql'),
('Data/EventInstanceStatus-Data.sql'),
('Data/ListenerInstanceStatus-Data.sql'),
('Data/SimpleHooks_Log-Data.sql')

-- iterate through the file names
declare @counter int, @max int
set @counter = 1
select @max = max(id) from @Files

while @counter <= @max
begin
	select @file = FileName from @Files where Id = @counter
	SET @cmd = 'sqlcmd -S ' + @ServerName + ' -d ' + @DatabaseName + ' -i "' + @path + @file + '" -U ' + @Username + ' -P ' + @Password
    EXEC xp_cmdshell @cmd

	set @counter = @counter + 1
end

