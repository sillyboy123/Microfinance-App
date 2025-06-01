using Microsoft.AspNetCore.Mvc.RazorPages;
using MicrofinanceApp.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MicrofinanceApp.Pages
{
    public class DatabaseStatusModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        
        public bool CanConnect { get; private set; }
        public string ProviderName { get; private set; } = string.Empty;
        public string ErrorMessage { get; private set; } = string.Empty;
        public string InnerErrorMessage { get; private set; } = string.Empty;
        public bool IsSQLite => ProviderName.Contains("Sqlite", StringComparison.OrdinalIgnoreCase);
        public string DatabasePath { get; private set; } = string.Empty;
        public string FileSize { get; private set; } = "N/A";
        public string LastModified { get; private set; } = "N/A";
        public int PendingMigrationCount { get; private set; }
        public Dictionary<string, int> TableCounts { get; private set; } = new Dictionary<string, int>();
        
        public DatabaseStatusModel(ApplicationDbContext context)
        {
            _context = context;
        }
        
        public async Task OnGetAsync()
        {
            // Get basic connection status
            var status = DatabaseConfigHelper.CheckConnection(_context);
            CanConnect = status.CanConnect;
            ProviderName = status.ProviderName;
            ErrorMessage = status.ErrorMessage;
            InnerErrorMessage = status.InnerErrorMessage;
            
            if (status.PendingMigrations != null)
            {
                PendingMigrationCount = status.PendingMigrations.Count();
            }
            
            // Get SQLite specific information
            if (IsSQLite && status.ConnectionDetails.TryGetValue("DatabasePath", out var dbPath))
            {
                DatabasePath = dbPath;
                
                if (status.ConnectionDetails.TryGetValue("FileSize", out var fileSize))
                    FileSize = fileSize;
                    
                if (status.ConnectionDetails.TryGetValue("LastModified", out var lastModified))
                    LastModified = lastModified;
            }
            
            // Get table counts if connected
            if (CanConnect)
            {
                try
                {
                    // Clients count
                    TableCounts["Clients"] = await _context.Clients.CountAsync();
                    
                    // Loans count
                    TableCounts["Loans"] = await _context.Loans.CountAsync();
                    
                    // Transactions count
                    TableCounts["Transactions"] = await _context.Transactions.CountAsync();
                    
                    // Users count
                    TableCounts["Users"] = await _context.Users.CountAsync();
                    
                    // Roles count 
                    TableCounts["Roles"] = await _context.Roles.CountAsync();
                    
                    try
                    {
                        // Savings accounts count
                        TableCounts["Savings Accounts"] = await _context.SavingsAccounts.CountAsync();
                    }
                    catch { /* table might not exist yet */ }
                    
                    try
                    {
                        // Payments count
                        TableCounts["Payments"] = await _context.Payments.CountAsync();
                    }
                    catch { /* table might not exist yet */ }
                }
                catch (Exception ex)
                {
                    ErrorMessage = $"Error getting table counts: {ex.Message}";
                }
            }
        }
    }
}
