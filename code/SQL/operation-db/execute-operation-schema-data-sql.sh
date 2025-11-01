#!/bin/bash

# SQL Server connection parameters
SERVER_NAME="simple-hooks-dbs"
SQL_USERNAME="sa"
SQL_PASSWORD="sample@Strong23Password"

# Function to execute SQL files from a directory
execute_sql_files() {
    local directory=$1
    for sql_file in "$directory"/*.sql; do
        if [ -f "$sql_file" ]; then
            echo "Executing $sql_file..."
            sqlcmd -S "$SERVER_NAME" -i "$sql_file" -U "$SQL_USERNAME" -P "$SQL_PASSWORD" -C
        fi
    done
}


echo "creating simplehooks db ..."
sqlcmd -S "$SERVER_NAME" -i "create-db.sql" -U "$SQL_USERNAME" -P "$SQL_PASSWORD" -C

# Execute SQL files from Tables directory
echo "Executing SQL files from Tables directory..."
execute_sql_files "Tables"

# Execute SQL files from StoredProcedures directory
echo "Executing SQL files from StoredProcedures directory..."
execute_sql_files "StoredProcedures"

# Execute SQL files from Data directory
echo "Executing SQL files from Data directory..."
execute_sql_files "Data"

echo "All SQL files have been executed." 