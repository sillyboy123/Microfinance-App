using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace MicrofinanceApp.Data
{
    /// <summary>
    /// Database configuration helper that provides automatic fallback to SQLite
    /// when SQL Server connection fails
    /// </summary>
    public static class DatabaseConfigHelper
    {
    /// <summary>
    /// Configures the database with automatic fallback to SQLite if SQL Server fails
    /// </summary>
    public static void ConfigureWithFallback(
            DbContextOptionsBuilder options, 
            string connectionString,
            string sqliteFilename = "MicrofinanceApp.db",
            ILogger? logger = null)        {            // Check if the connection string is for SQLite
            if (connectionString != null && 
                (connectionString.Contains("Data Source=") || 
                 connectionString.ToLowerInvariant().Contains(".db") ||
                 connectionString.ToLowerInvariant().Contains("sqlite")))
            {
                logger?.LogInformation("Detected SQLite connection string");
                options.UseSqlite(connectionString);
                return;
            }
            
            try
            {
                // Try to use SQL Server if the connection string appears to be for SQL Server
                logger?.LogInformation("Attempting to connect to SQL Server");
                options.UseSqlServer(connectionString);
            }
            catch (Exception ex)
            {
                // Log the SQL Server connection failure
                logger?.LogWarning(ex, "SQL Server connection failed. Falling back to SQLite");
                
                // Create SQLite connection
                var dataDir = AppDomain.CurrentDomain.GetData("DataDirectory")?.ToString() 
                    ?? AppContext.BaseDirectory;
                
                var sqliteConnectionString = new SqliteConnectionStringBuilder
                {
                    DataSource = Path.Combine(dataDir, sqliteFilename)
                }.ToString();
                
                logger?.LogInformation("Using SQLite with connection string: {ConnectionString}", 
                    $"DataSource={sqliteFilename}");
                
                // Configure to use SQLite
                options.UseSqlite(sqliteConnectionString);
            }
        }
          /// <summary>
        /// Creates a database connection status report
        /// </summary>
        public static DbConnectionStatus CheckConnection(ApplicationDbContext context)
        {
            var status = new DbConnectionStatus();
            
            try
            {
                status.CanConnect = context.Database.CanConnect();
                status.ProviderName = context.Database.ProviderName ?? "Unknown";
                
                if (status.CanConnect)
                {
                    status.PendingMigrations = context.Database.GetPendingMigrations();
                    
                    // Add more details for SQLite databases
                    if (status.ProviderName.Contains("Sqlite"))
                    {
                        var connection = context.Database.GetDbConnection();
                        var connectionString = connection.ConnectionString;
                        var dataSource = new SqliteConnectionStringBuilder(connectionString).DataSource;
                        
                        status.ConnectionDetails["DatabasePath"] = dataSource;
                        status.ConnectionDetails["FileExists"] = File.Exists(dataSource) ? "Yes" : "No";
                        
                        if (File.Exists(dataSource))
                        {
                            var fileInfo = new FileInfo(dataSource);
                            status.ConnectionDetails["FileSize"] = $"{fileInfo.Length / 1024.0:F2} KB";
                            status.ConnectionDetails["LastModified"] = fileInfo.LastWriteTime.ToString("g");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                status.CanConnect = false;
                status.ErrorMessage = ex.Message ?? "Unknown error";
                status.InnerErrorMessage = ex.InnerException?.Message ?? string.Empty;
            }
            
            return status;
        }
    }
      /// <summary>
    /// Represents the status of a database connection
    /// </summary>
    public class DbConnectionStatus
    {
        public bool CanConnect { get; set; }
        public string ProviderName { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public Dictionary<string, string> ConnectionDetails { get; set; } = new Dictionary<string, string>();
        public string InnerErrorMessage { get; set; } = string.Empty;
        public System.Collections.Generic.IEnumerable<string> PendingMigrations { get; set; } = 
            Array.Empty<string>();
    }
}
