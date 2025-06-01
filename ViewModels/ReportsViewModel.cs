using System;
using System.Collections.Generic;
using FinPlus.Models;

namespace FinPlus.ViewModels
{
    public class ReportsViewModel
    {
        // Summary statistics
        public int TotalClients { get; set; }
        public int ActiveLoans { get; set; }
        public int ActiveSavingsAccounts { get; set; }
        public decimal TotalLoanAmount { get; set; }
        public decimal TotalSavingsAmount { get; set; }
        
        // Collection data for charts
        public Dictionary<string, int> LoansByStatusCount { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, decimal> LoansByStatusAmount { get; set; } = new Dictionary<string, decimal>();
        public Dictionary<string, int> ClientRegistrationsByMonth { get; set; } = new Dictionary<string, int>();
        
        // Recent activity
        public List<Transaction> RecentTransactions { get; set; } = new List<Transaction>();
        public List<Loan> RecentLoans { get; set; } = new List<Loan>();
        
        // Custom date range filters
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        
        // Report type
        public string ReportType { get; set; } = "Summary";
    }
}
