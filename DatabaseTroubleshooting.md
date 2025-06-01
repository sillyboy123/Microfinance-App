# Database Troubleshooting Guide for Microfinance App

This guide will help you resolve database connection issues in the Microfinance App.

## Quick Solutions

1. **Run Diagnostics Page**: Visit `/Home/Diagnostics` in the application to see the current database status.

2. **Switch to LocalDB**: Run `SetupLocalDB.bat` to configure SQL Server LocalDB.

3. **Switch to SQLite**: If SQL Server is not working, run `SetupSQLiteBackup.bat` to switch to SQLite.

## Detailed Troubleshooting Steps

### Step 1: Check Current Database Status

1. Launch the application
2. Visit `/Home/Diagnostics` or `/ConnectionTest`
3. Check the connection status and error messages

### Step 2: Verify SQL Server Installation

For SQL Server Express:
1. Check if SQL Server Express is installed
2. Verify the service is running (Search for "Services" in Windows and look for "SQL Server (SQLEXPRESS)")
3. If not running, start the service

For LocalDB:
1. Open Command Prompt as Administrator
2. Run `SqlLocalDB info` to see all LocalDB instances
3. If needed, run `SqlLocalDB create "MicrofinanceDB"` to create a new instance
4. Run `SqlLocalDB start "MicrofinanceDB"` to start it

### Step 3: Check Connection String

Your connection string in `appsettings.json` should match your SQL Server setup:

For SQL Server Express:
```json
"DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=MicrofinanceApp;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
```

For LocalDB:
```json
"DefaultConnection": "Server=(localdb)\\MicrofinanceDB;Database=MicrofinanceApp;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
```

For SQLite:
```json
"DefaultConnection": "Data Source=MicrofinanceApp.db"
```

### Step 4: Check for Database Creation

The application should automatically create the database and apply migrations when it starts.

If you need to manually apply migrations:
1. Open Command Prompt in the project directory
2. Run:
   ```
   dotnet ef database update
   ```

### Step 5: Fall Back to SQLite

If SQL Server continues to have issues:
1. Run `SetupSQLiteBackup.bat` to switch to SQLite
2. Restart the application
3. The application should automatically create the SQLite database file

### Common Error Messages and Solutions

| Error Message | Possible Solution |
|---------------|-------------------|
| "Cannot open database ... requested by the login." | The database doesn't exist. Let the application create it or run migrations manually. |
| "Login failed for user ..." | Check your connection string and ensure you're using Windows Authentication or the correct SQL credentials. |
| "A network-related or instance-specific error..." | SQL Server is not running. Start the SQL Server service. |
| "The SqlInstance 'MSSQLLocalDB' does not exist." | LocalDB is not installed or the instance doesn't exist. Run `SetupLocalDB.bat`. |

## Need Further Help?

If these steps don't resolve your issue:

1. Delete any existing database files (both SQL and SQLite)
2. Restore a clean `appsettings.json` file
3. Run the application with a fresh database

For advanced help, check the console output and logs for specific error details.
