# PowerShell Script to Test SQL Server Connection
Write-Host "Checking SQL Server connection for Microfinance App..."

try {
    # Import SQL Server module if available
    if (Get-Module -ListAvailable -Name SqlServer) {
        Import-Module SqlServer
    } else {
        Write-Host "SqlServer module not available. Proceeding with System.Data.SqlClient only."
    }
    
    # Get connection string from appsettings.json
    $appSettingsContent = Get-Content -Path ".\appsettings.json" -Raw
    $appSettings = $appSettingsContent | ConvertFrom-Json
    $connectionString = $appSettings.ConnectionStrings.DefaultConnection
    
    Write-Host "Using connection string: $connectionString"
    
    # Create SQL Connection
    $conn = New-Object System.Data.SqlClient.SqlConnection
    $conn.ConnectionString = $connectionString
    
    Write-Host "Attempting to open connection..."
    $conn.Open()
    
    if ($conn.State -eq "Open") {
        Write-Host "Connection successful! SQL Server is accessible." -ForegroundColor Green
        
        # Get SQL Server version
        $cmd = $conn.CreateCommand()
        $cmd.CommandText = "SELECT @@VERSION"
        $reader = $cmd.ExecuteReader()
        
        if ($reader.Read()) {
            $version = $reader.GetValue(0)
            Write-Host "SQL Server Version: $version" -ForegroundColor Green
        }
        
        $reader.Close()
        
        # Check if the database exists
        $cmd = $conn.CreateCommand()
        $cmd.CommandText = "SELECT DB_ID('MicrofinanceApp')"
        $result = $cmd.ExecuteScalar()
        
        if ($result -ne $null) {
            Write-Host "Database 'MicrofinanceApp' exists." -ForegroundColor Green
            
            # Check tables
            $cmd = $conn.CreateCommand()
            $cmd.CommandText = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE'"
            $reader = $cmd.ExecuteReader()
            
            Write-Host "Tables in the database:" -ForegroundColor Cyan
            while ($reader.Read()) {
                Write-Host "  - $($reader.GetValue(0))"
            }
            $reader.Close()
        } else {
            Write-Host "Database 'MicrofinanceApp' does not exist." -ForegroundColor Yellow
        }
    } 
    
    # Close the connection
    if ($conn.State -eq "Open") {
        $conn.Close()
        Write-Host "Connection closed."
    }
} catch {
    Write-Host "Error connecting to SQL Server: $_" -ForegroundColor Red
    Write-Host "Check if SQL Server is running and the connection string is correct." -ForegroundColor Red
    
    Write-Host "`nTrying to check SQL Server availability..." -ForegroundColor Yellow
    
    # Check if SQL Server service is running
    $sqlService = Get-Service -Name *SQL* | Where-Object { $_.DisplayName -like "*SQL Server (*" }
    
    if ($sqlService) {
        Write-Host "SQL Server Service Status:" -ForegroundColor Cyan
        foreach ($service in $sqlService) {
            Write-Host "  - $($service.DisplayName): $($service.Status)"
        }
    } else {
        Write-Host "No SQL Server service found. Make sure SQL Server is installed." -ForegroundColor Red
    }
    
    Write-Host "`nChecking for SQLite fallback..." -ForegroundColor Yellow
    if (Test-Path ".\MicrofinanceApp.db") {
        Write-Host "SQLite database file found: MicrofinanceApp.db" -ForegroundColor Green
    } else {
        Write-Host "No SQLite database file found." -ForegroundColor Red
    }
}

# Keep window open
Write-Host "`nPress any key to exit..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
