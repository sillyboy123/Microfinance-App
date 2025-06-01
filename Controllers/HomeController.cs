using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MicrofinanceApp.Models;
using MicrofinanceApp.ViewModels;
using MicrofinanceApp.Data;

namespace MicrofinanceApp.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public IActionResult Index()
    {
        return View();
    }        public IActionResult Privacy()
    {
        return View();
    }
      [Route("connection-test")]
    public IActionResult ConnectionTest()
    {
        var testResult = DatabaseConfigHelper.CheckConnection(_context);
        
        var viewModel = new Dictionary<string, object>
        {
            ["CanConnect"] = testResult.CanConnect,
            ["ProviderName"] = testResult.ProviderName,
            ["ErrorMessage"] = testResult.ErrorMessage,
            ["InnerErrorMessage"] = testResult.InnerErrorMessage,
            ["PendingMigrations"] = testResult.PendingMigrations.Any() ? 
                string.Join(", ", testResult.PendingMigrations) : "None",
            ["ConnectionDetails"] = testResult.ConnectionDetails
        };
        
        return Json(viewModel);
    }
      [Route("diagnostics")]
    public IActionResult Diagnostics()
    {
        var diagnosticModel = new Dictionary<string, object>();
        
        try
        {
            // Use our enhanced connection test
            var testResult = DatabaseConfigHelper.CheckConnection(_context);
            diagnosticModel["DatabaseConnection"] = testResult.CanConnect ? "Success" : "Failed";
            diagnosticModel["DatabaseProvider"] = testResult.ProviderName;
            
            if (!string.IsNullOrEmpty(testResult.ErrorMessage))
            {
                diagnosticModel["Error"] = testResult.ErrorMessage;
                if (!string.IsNullOrEmpty(testResult.InnerErrorMessage))
                {
                    diagnosticModel["InnerError"] = testResult.InnerErrorMessage;
                }
            }
            
            if (testResult.CanConnect)
            {
                // Include connection details for SQLite
                foreach (var detail in testResult.ConnectionDetails)
                {
                    diagnosticModel[detail.Key] = detail.Value;
                }
                
                // Get pending migrations
                var pendingMigrations = testResult.PendingMigrations.ToList();
                diagnosticModel["PendingMigrations"] = pendingMigrations.Count > 0 ? 
                    string.Join(", ", pendingMigrations) : "None";
                
                // Get tables count
                try
                {
                    var connection = _context.Database.GetDbConnection();
                    var command = connection.CreateCommand();
                    string tableQuery;
                    
                    // Adjust query based on provider
                    if (testResult.ProviderName.Contains("Sqlite"))
                    {
                        tableQuery = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%'";
                    }
                    else
                    {
                        tableQuery = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE'";
                    }
                    
                    if (connection.State != System.Data.ConnectionState.Open)
                    {
                        connection.Open();
                    }
                    
                    command.CommandText = tableQuery;
                    var tableCount = command.ExecuteScalar();
                    diagnosticModel["TableCount"] = tableCount?.ToString() ?? "0";
                }
                catch (Exception ex)
                {
                    diagnosticModel["TableQueryError"] = ex.Message;
                }
                
                // Check for clients
                var clientCount = _context.Clients.Count();
                diagnosticModel["ClientCount"] = clientCount;
            }
        }
        catch (Exception ex)
        {
            diagnosticModel["Error"] = ex.Message;
            if (ex.InnerException != null)
            {
                diagnosticModel["InnerError"] = ex.InnerException.Message;
            }
        }
        
        return Json(diagnosticModel);
    }
    
    [Microsoft.AspNetCore.Authorization.Authorize]
    public IActionResult Reports()
    {
        var reportsViewModel = new ReportsViewModel();
        
        try
        {
            // Get data from database for reports
            var clients = _context.Clients.ToList();
            var loans = _context.Loans.Include(l => l.Client).ToList();
            var savings = _context.SavingsAccounts.Include(s => s.Client).ToList();
            var transactions = _context.Transactions.Include(t => t.Client)
                .OrderByDescending(t => t.TransactionDate)
                .Take(10)
                .ToList();
            
            // Populate summary statistics
            reportsViewModel.TotalClients = clients.Count;
            reportsViewModel.ActiveLoans = loans.Count(l => l.Status == LoanStatus.Active);
            reportsViewModel.ActiveSavingsAccounts = savings.Count;
            reportsViewModel.TotalLoanAmount = loans.Where(l => l.Status == LoanStatus.Active)
                .Sum(l => l.Amount);
            reportsViewModel.TotalSavingsAmount = savings.Sum(s => s.Balance);
            
            // Group loans by status for charts
            foreach (LoanStatus status in Enum.GetValues(typeof(LoanStatus)))
            {
                var statusLoans = loans.Where(l => l.Status == status);
                reportsViewModel.LoansByStatusCount[status.ToString()] = statusLoans.Count();
                reportsViewModel.LoansByStatusAmount[status.ToString()] = statusLoans.Sum(l => l.Amount);
            }
            
            // Get client registrations by month
            var groupedClients = clients.GroupBy(c => new { c.RegistrationDate.Year, c.RegistrationDate.Month })
                .Select(g => new
                {
                    YearMonth = $"{g.Key.Year}-{g.Key.Month:D2}",
                    Count = g.Count()
                })
                .OrderBy(x => x.YearMonth)
                .Take(12);
                
            foreach (var group in groupedClients)
            {
                reportsViewModel.ClientRegistrationsByMonth[group.YearMonth] = group.Count;
            }
            
            // Recent transactions and loans for the report
            reportsViewModel.RecentTransactions = transactions;
            reportsViewModel.RecentLoans = loans.OrderByDescending(l => l.ApplicationDate).Take(5).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading reports data");
            
            // Provide some mock data if database query fails
            reportsViewModel.TotalClients = 248;
            reportsViewModel.ActiveLoans = 185;
            reportsViewModel.ActiveSavingsAccounts = 142;
            reportsViewModel.TotalLoanAmount = 856000.00m;
            reportsViewModel.TotalSavingsAmount = 345200.00m;
        }
        
        return View(reportsViewModel);
    }
    
    [Microsoft.AspNetCore.Authorization.Authorize]
    public IActionResult Dashboard()
    {
        var dashboardViewModel = new DashboardViewModel();
        
        try
        {
            // For demo purposes, we'll use mock data instead of database queries
            dashboardViewModel.TotalClients = 248;
            dashboardViewModel.ActiveLoans = 185;
            dashboardViewModel.TotalLoanAmount = 856000.00m;
            dashboardViewModel.OutstandingBalance = 542700.00m;
            dashboardViewModel.OverdueLoans = 12;
            dashboardViewModel.TotalSavings = 345200.00m;
            
            // Sample recent activities
            dashboardViewModel.RecentActivities = new List<DashboardViewModel.RecentActivity>
            {
                new DashboardViewModel.RecentActivity 
                { 
                    Type = "Loan",
                    Description = "New loan approved", 
                    Amount = 5000.00m,
                    Timestamp = DateTime.Now.AddHours(-2),
                    ClientName = "John Smith"
                },
                new DashboardViewModel.RecentActivity 
                { 
                    Type = "Payment", 
                    Description = "Loan payment received", 
                    Amount = 350.00m,
                    Timestamp = DateTime.Now.AddHours(-5),
                    ClientName = "Maria Garcia"
                },
                new DashboardViewModel.RecentActivity 
                { 
                    Type = "Client", 
                    Description = "New client registered", 
                    Amount = null,
                    Timestamp = DateTime.Now.AddDays(-1),
                    ClientName = "David Johnson"
                },
                new DashboardViewModel.RecentActivity 
                { 
                    Type = "Savings", 
                    Description = "Deposit to savings account", 
                    Amount = 1200.00m,
                    Timestamp = DateTime.Now.AddDays(-1).AddHours(-3),
                    ClientName = "Anna Brown"
                }
            };
            
            // Sample upcoming payments
            dashboardViewModel.UpcomingPayments = new List<DashboardViewModel.UpcomingPayment>
            {
                new DashboardViewModel.UpcomingPayment
                {
                    ClientName = "Maria Garcia",
                    Amount = 350.00m,
                    DueDate = DateTime.Now.AddDays(3),
                    LoanReference = "L-2025-0042"
                },
                new DashboardViewModel.UpcomingPayment
                {
                    ClientName = "John Smith",
                    Amount = 420.00m,
                    DueDate = DateTime.Now.AddDays(5),
                    LoanReference = "L-2025-0036"
                },
                new DashboardViewModel.UpcomingPayment
                {
                    ClientName = "Lisa Wong",
                    Amount = 560.00m,
                    DueDate = DateTime.Now.AddDays(7),
                    LoanReference = "L-2025-0039"
                }
            };
            
            // Sample performance metrics
            dashboardViewModel.Performance = new DashboardViewModel.PerformanceMetrics
            {
                RepaymentRate = 94.5m,
                NewClientsThisMonth = 12,
                RevenueThisMonth = 15600.00m,
                GrowthRate = 8.2m
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading dashboard data");
        }
        
        return View(dashboardViewModel);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
