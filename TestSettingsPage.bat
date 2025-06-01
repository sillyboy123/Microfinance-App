@echo off
echo Stopping any running dotnet processes...
taskkill /IM dotnet.exe /F
timeout /t 2 /nobreak > nul

echo Building the application...
dotnet build

echo Starting the application...
start "" dotnet run --urls=http://localhost:5901

echo Opening test page...
timeout /t 5 /nobreak > nul
start "" http://localhost:5901/test-settings-link.html
