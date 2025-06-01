@echo off
echo ======================================================
echo SQLite Database Reset Tool for Microfinance App
echo ======================================================
echo.

echo This script will delete and recreate the SQLite database.
echo All existing data will be lost.
echo.

set /p CONTINUE=Do you want to continue? (Y/N): 

if /i "%CONTINUE%" NEQ "Y" (
    echo Operation canceled.
    pause
    exit /b
)

echo.
echo Checking if application is using SQLite...
echo.

findstr /i "Data Source=" appsettings.json >nul
if %errorlevel% neq 0 (
    echo.
    echo Your application is not configured to use SQLite.
    echo Please run SetupSQLiteBackup.bat first.
    echo.
    pause
    exit /b
)

echo.
echo Stopping any running instance of the application...
taskkill /f /im MicrofinanceApp.exe 2>nul

echo.
echo Deleting existing database file...
if exist MicrofinanceApp.db (
    del /f MicrofinanceApp.db
    echo Existing database deleted.
) else (
    echo No existing database file found.
)

echo.
echo ======================================================
echo Database reset complete.
echo.
echo Next steps:
echo 1. Run the application to create a fresh SQLite database
echo 2. The application will automatically create the schema and seed initial data
echo ======================================================
echo.
pause
