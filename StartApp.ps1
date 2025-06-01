# Run this script to start the Microfinance App
Write-Host "Preparing to start Microfinance App..."

# Kill any existing dotnet or MicrofinanceApp processes
Get-Process | Where-Object { $_.ProcessName -like "*dotnet*" -or $_.ProcessName -like "*Microfinance*" } | ForEach-Object { 
    Write-Host "Stopping process: $($_.ProcessName) (ID: $($_.Id))"
    Stop-Process -Id $_.Id -Force -ErrorAction SilentlyContinue 
}

# Clean temp files
Write-Host "Cleaning temporary files..."
Remove-Item -Path "bin\Debug\net8.0\*.dll.locked" -Force -ErrorAction SilentlyContinue
Remove-Item -Path "bin\Debug\net8.0\*.exe.locked" -Force -ErrorAction SilentlyContinue

# Set environment variables
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:ASPNETCORE_URLS = "http://localhost:5901"

# Build the application
Write-Host "Building the application..."
dotnet build

# Run the application
Write-Host "Starting the application..."
Write-Host "The app should be available at http://localhost:5901"
Write-Host "Press Ctrl+C to stop the application"
dotnet run --no-build

# Keep window open
Write-Host "Application has stopped. Press any key to exit..."
$null = $host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
