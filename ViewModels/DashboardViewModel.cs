using FinPlus.Models;
using System.Collections.Generic;

namespace FinPlus.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalClients { get; set; }
        public int ActiveLoans { get; set; }
        public decimal TotalLoanAmount { get; set; }
        public decimal OutstandingBalance { get; set; }
        public decimal TotalSavings { get; set; }
        public int OverdueLoans { get; set; }
        public List<RecentActivity> RecentActivities { get; set; } = new List<RecentActivity>();
        public List<UpcomingPayment> UpcomingPayments { get; set; } = new List<UpcomingPayment>();
        public PerformanceMetrics Performance { get; set; } = new PerformanceMetrics();

        public class RecentActivity
        {
            public string Type { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public decimal? Amount { get; set; }
            public DateTime Timestamp { get; set; }
            public string ClientName { get; set; } = string.Empty;
        }

        public class UpcomingPayment
        {
            public string ClientName { get; set; } = string.Empty;
            public decimal Amount { get; set; }
            public DateTime DueDate { get; set; }
            public string LoanReference { get; set; } = string.Empty;
        }

        public class PerformanceMetrics
        {
            public decimal RepaymentRate { get; set; }
            public int NewClientsThisMonth { get; set; }
            public decimal RevenueThisMonth { get; set; }
            public decimal GrowthRate { get; set; }
        }
    }
}
