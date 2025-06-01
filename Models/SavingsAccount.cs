using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinPlus.Models
{
    public class SavingsAccount
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ClientId { get; set; }

        [ForeignKey("ClientId")]
        public Client? Client { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Current Balance")]
        public decimal Balance { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Interest Rate (%)")]
        [Range(0.1, 100, ErrorMessage = "Interest rate must be between 0.1% and 100%")]
        public decimal InterestRate { get; set; } = 3.5m; // Default interest rate

        [Required]
        [Display(Name = "Account Number")]
        public string AccountNumber { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Opening Date")]
        public DateTime OpeningDate { get; set; } = DateTime.Now;

        [Required]
        public AccountStatus Status { get; set; } = AccountStatus.Active;
    }

    public enum AccountStatus
    {
        Active,
        Dormant,
        Closed
    }
}
