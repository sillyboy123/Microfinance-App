using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinPlus.Models
{
    public class Transaction
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ClientId { get; set; }

        [ForeignKey("ClientId")]
        public Client? Client { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Transaction type is required")]
        [EnumDataType(typeof(TransactionType), ErrorMessage = "Please select a valid transaction type")]
        public TransactionType Type { get; set; }        // Reference is generated automatically in the controller
        public string Reference { get; set; } = string.Empty;

        public DateTime TransactionDate { get; set; } = DateTime.Now;

        public TransactionStatus Status { get; set; } = TransactionStatus.Pending;

        [StringLength(500)]
        public string? Description { get; set; }
    }

    public enum TransactionType
    {
        [Display(Name = "Loan Disbursement")]
        LoanDisbursement,
        [Display(Name = "Loan Repayment")]
        LoanRepayment,
        [Display(Name = "Savings Deposit")]
        SavingsDeposit,
        [Display(Name = "Savings Withdrawal")]
        SavingsWithdrawal,
        [Display(Name = "Fee")]
        Fee,
        [Display(Name = "Other")]
        Other
    }

    public enum TransactionStatus
    {
        Pending,
        Completed,
        Failed,
        Reversed
    }
}
