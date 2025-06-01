using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using FinPlus.Data.Identity;
using FinPlus.Models;

namespace FinPlus.Data
{    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        
        public DbSet<Transaction> Transactions { get; set; }

        public DbSet<Client> Clients { get; set; } = null!;
        public DbSet<Loan> Loans { get; set; } = null!;
        public DbSet<Payment> Payments { get; set; } = null!;
        public DbSet<SavingsAccount> SavingsAccounts { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure cascade delete for Loans
            modelBuilder.Entity<Loan>()
                .HasOne(l => l.Client)
                .WithMany(c => c.Loans)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure cascade delete for Payments
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Loan)
                .WithMany()
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
