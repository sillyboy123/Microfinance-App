using System;
using System.ComponentModel.DataAnnotations;

namespace MicrofinanceApp.Models
{
    public class Client
    {
        [Key]
        public int Id { get; set; }        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        public string Address { get; set; } = string.Empty;

        [Required]
        public DateTime DateOfBirth { get; set; }

        [Required]
        public DateTime RegistrationDate { get; set; } = DateTime.Now;        public string FullName => $"{FirstName} {LastName}";

        // Navigation property for loans
        public virtual ICollection<Loan> Loans { get; set; } = new List<Loan>();
    }
}
