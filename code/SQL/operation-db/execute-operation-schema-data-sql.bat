set servername=".,14333"
set sqlusername="sa"
set sqlpassword="sample@Strong23Password"
::by default sqlcmd uses windows authentication
::use the arguments -U username -P password at the end of the sqlcmd command to use SQL authentication

:: execute all sql files in the Tables folder

set source="Tables\*.sql"

for %%f in (%source%) do (
    sqlcmd -S %servername% -i "%%f" -U %sqlusername% -P %sqlpassword%
)

:: execute all sql files in the StoredProcedures folder

set source="StoredProcedures\*.sql"

for %%f in (%source%) do (
    sqlcmd -S %servername% -i "%%f" -U %sqlusername% -P %sqlpassword%
)

:: execute all sql files in the Data folder

set source="Data\*.sql"

for %%f in (%source%) do (
    sqlcmd -S %servername% -i "%%f" -U %sqlusername% -P %sqlpassword%
)
