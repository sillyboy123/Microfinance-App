@echo off
echo Starting Microfinance App with explicit port...
set ASPNETCORE_URLS=http://localhost:5901
dotnet run --no-build
pause
