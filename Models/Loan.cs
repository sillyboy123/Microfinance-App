using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MicrofinanceApp.Models
{
    public class Loan
    {
        [Key]
        public int Id { get; set; }

        [Required]        public int ClientId { get; set; }

        [ForeignKey("ClientId")]
        public Client? Client { get; set; }

        [Required(ErrorMessage = "Please enter the loan amount")]
        [Range(100, 1000000, ErrorMessage = "Amount must be between $100 and $1,000,000")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Loan Amount")]
        public decimal Amount { get; set; }

        [Required]
        [Range(1, 60, ErrorMessage = "Term must be between 1 and 60 months")]
        [Display(Name = "Loan Term (Months)")]
        public int Term { get; set; }

        [Required(ErrorMessage = "Please enter the loan purpose")]
        [StringLength(200, MinimumLength = 10, ErrorMessage = "Purpose must be between 10 and 200 characters")]
        [Display(Name = "Loan Purpose")]
        public string Purpose { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(1, 100, ErrorMessage = "Interest rate must be between 1% and 100%")]
        public decimal InterestRate { get; set; } = 12.0m;

        [Required]
        public DateTime ApplicationDate { get; set; } = DateTime.Now;

        public DateTime? ApprovalDate { get; set; }

        [Required]
        public LoanStatus Status { get; set; } = LoanStatus.Pending;

        [Column(TypeName = "decimal(18,2)")]
        public decimal MonthlyPayment { get; set; }
    }

    public enum LoanStatus
    {
        Pending,
        Approved,
        Rejected,
        Active,
        Completed,
        Defaulted
    }
}
