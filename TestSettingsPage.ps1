# Stop any running dotnet processes
Write-Host "Stopping any running dotnet processes..."
Get-Process -Name dotnet -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep -Seconds 2

# Build the application
Write-Host "Building the application..."
dotnet build

# Start the application
Write-Host "Starting the application..."
Start-Process -FilePath "dotnet" -ArgumentList "run", "--urls=http://localhost:5901"

# Wait for the app to start
Write-Host "Waiting for application to start..."
Start-Sleep -Seconds 5

# Open the test page
Write-Host "Opening test page..."
Start-Process -FilePath "http://localhost:5901/test-settings-link.html"
