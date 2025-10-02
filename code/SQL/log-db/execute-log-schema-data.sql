DECLARE @path NVARCHAR(512) = N'/var/opt/mssql/data/'
declare @ServerName varchar(100) = '.'
declare @DatabaseName varchar(100) = 'SimpleHooks_Log_DB'
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
('StoredProcedures/SimpleHooks_Log_Add.sql'),
('StoredProcedures/Delete_Old_Logs.sql'),
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

