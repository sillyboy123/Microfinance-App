using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinPlus.Models
{
    public class Payment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int LoanId { get; set; }

        [ForeignKey("LoanId")]
        public required Loan Loan { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        public DateTime PaymentDate { get; set; } = DateTime.Now;

        [Required]
        public PaymentStatus Status { get; set; }

        public required string TransactionReference { get; set; }
    }

    public enum PaymentStatus
    {
        Pending,
        Successful,
        Failed
    }
}
