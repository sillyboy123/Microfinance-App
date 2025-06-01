using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MicrofinanceApp.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MicrofinanceApp.Pages
{
    public class ConnectionTestModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        
        public bool CanConnect { get; private set; }
        public bool DatabaseExists { get; private set; }
        public string ErrorMessage { get; private set; } = string.Empty;
        public string ConnectionString { get; private set; } = string.Empty;
        public string ProviderName { get; private set; } = string.Empty;
        public int PendingMigrations { get; private set; }
        public Dictionary<string, int> Tables { get; private set; } = new Dictionary<string, int>();
        
        public ConnectionTestModel(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            
            // Obfuscate password in connection string
            var connStr = _configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
            if (!string.IsNullOrEmpty(connStr))
            {
                ConnectionString = Regex.Replace(connStr, 
                    @"(Password|pwd)=[^;]*", 
                    "$1=********", 
                    RegexOptions.IgnoreCase);
            }
        }
        
        public void OnGet()
        {
            TestConnection();
        }
        
        public void OnPost()
        {
            TestConnection();
        }
        
        private void TestConnection()
        {
            try
            {
                CanConnect = _context.Database.CanConnect();
                
                if (CanConnect)
                {                    ProviderName = _context.Database.ProviderName ?? "Unknown";
                    DatabaseExists = _context.Database.EnsureCreated();
                    PendingMigrations = _context.Database.GetPendingMigrations().Count();
                      // Get table information depending on the provider
                    string tableQuery;
                    
                    if (ProviderName.Contains("SqlServer"))
                    {
                        tableQuery = @"
                            SELECT 
                                t.name AS TableName,
                                p.rows AS RowCount
                            FROM 
                                sys.tables t
                            INNER JOIN 
                                sys.indexes i ON t.OBJECT_ID = i.object_id
                            INNER JOIN 
                                sys.partitions p ON i.object_id = p.OBJECT_ID AND i.index_id = p.index_id
                            WHERE 
                                t.is_ms_shipped = 0 AND
                                i.OBJECT_ID > 255 AND
                                i.index_id <= 1
                            GROUP BY 
                                t.name, p.rows
                            ORDER BY 
                                t.name";
                    }
                    else if (ProviderName.Contains("Sqlite"))
                    {
                        tableQuery = @"
                            SELECT 
                                name AS TableName,
                                0 AS RowCount
                            FROM 
                                sqlite_master
                            WHERE 
                                type='table' AND
                                name NOT LIKE 'sqlite_%' AND
                                name NOT LIKE 'ef_%'
                            ORDER BY 
                                name";
                    }
                    else
                    {
                        // Generic fallback that works for many providers
                        tableQuery = "SELECT name AS TableName, 0 AS RowCount FROM sqlite_master WHERE type='table'";
                    }

                    try
                    {
                        var connection = _context.Database.GetDbConnection();
                        if (connection.State != System.Data.ConnectionState.Open)
                            connection.Open();
                            
                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText = tableQuery;
                            using (var reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var tableName = reader.GetString(0);
                                    var rowCount = reader.GetInt32(1);
                                    Tables[tableName] = rowCount;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // If we can't get table info, just continue
                        // Log the error but don't fail the entire test
                        Console.WriteLine($"Error getting table info: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                CanConnect = false;
                ErrorMessage = $"{ex.Message}{(ex.InnerException != null ? $" - {ex.InnerException.Message}" : string.Empty)}";
            }
        }
    }
}
