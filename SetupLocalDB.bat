@echo off
echo ======================================================
echo SQL Server LocalDB Setup Tool for Microfinance App
echo ======================================================
echo.

REM Check if SqlLocalDB is installed
where SqlLocalDB >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo SQL Server LocalDB is not found in PATH.
    echo.
    
    if exist "%~dp0SqlLocalDB.exe" (
        echo Found SqlLocalDB.exe in the current directory.
        echo Installing SQL Server LocalDB...
        start "" "%~dp0SqlLocalDB.exe" /qn IACCEPTSQLLOCALDBLICENSETERMS=YES
        echo.
        echo Please wait for the installation to complete and then run this script again.
        pause
        exit /b
    ) else (
        echo Please download and install SQL Server LocalDB from:
        echo https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb
        echo.
        echo After installation, run this script again.
        pause
        exit /b
    )
)

echo SQL Server LocalDB is installed.
echo.

REM Check existing instances
echo Checking for existing LocalDB instances...
SqlLocalDB info

echo.
echo Creating or starting MicrofinanceDB instance...
SqlLocalDB create "MicrofinanceDB" -s

echo.
echo Displaying instance details:
SqlLocalDB info "MicrofinanceDB"

echo.
echo ======================================================
echo Connection String Setup Instructions
echo ======================================================
echo.
echo Use the following connection string in your appsettings.json:
echo.
echo "DefaultConnection": "Server=(localdb)\MicrofinanceDB;Database=MicrofinanceApp;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
echo.
echo ======================================================
echo.
echo After updating your connection string, restart the application.
echo.

pause
