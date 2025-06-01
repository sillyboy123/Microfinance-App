@echo off
echo ======================================================
echo SQLite Database Recovery Tool for Microfinance App
echo ======================================================
echo.

echo This script will set up the application to use SQLite instead of SQL Server.
echo Use this if you're having issues with SQL Server connection.
echo.

set /p CONTINUE=Do you want to continue? (Y/N): 

if /i "%CONTINUE%" NEQ "Y" (
    echo Operation canceled.
    pause
    exit /b
)

echo.
echo Updating connection string to use SQLite...
echo.

REM Create a temporary file with the updated connection string
echo {> tmp_appsettings.json
echo   "ConnectionStrings": {>> tmp_appsettings.json
echo     "DefaultConnection": "Data Source=MicrofinanceApp.db">> tmp_appsettings.json
echo   },>> tmp_appsettings.json
echo   "Logging": {>> tmp_appsettings.json
echo     "LogLevel": {>> tmp_appsettings.json
echo       "Default": "Information",>> tmp_appsettings.json
echo       "Microsoft.AspNetCore": "Warning">> tmp_appsettings.json
echo     }>> tmp_appsettings.json
echo   },>> tmp_appsettings.json
echo   "AllowedHosts": "*">> tmp_appsettings.json
echo }>> tmp_appsettings.json

REM Backup the original appsettings.json
if exist appsettings.json (
    echo Backing up original appsettings.json to appsettings.json.bak
    copy appsettings.json appsettings.json.bak
)

REM Replace the appsettings.json with our new version
copy tmp_appsettings.json appsettings.json
del tmp_appsettings.json

echo.
echo Connection string updated to use SQLite.
echo.
echo If a SQLite database file doesn't exist yet, it will be created
echo when you run the application next time.
echo.
echo ======================================================
echo.
echo Next steps:
echo 1. Run the application to create the SQLite database
echo 2. Visit /diagnostics to verify the connection
echo.
echo To switch back to SQL Server, restore your appsettings.json.bak file.
echo.

pause
