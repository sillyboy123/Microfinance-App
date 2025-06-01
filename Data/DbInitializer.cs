using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using FinPlus.Data.Identity;
using FinPlus.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace FinPlus.Data
{    public static class DbInitializer
    {
        public static async Task Initialize(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILogger logger)
        {
            try
            {
                // Check if using SQLite and initialize properly
                if (context.Database.ProviderName?.Contains("Sqlite") == true)
                {
                    await InitializeSQLite(context, userManager, logger);
                    return;
                }
                
                // Standard initialization for SQL Server
                logger.LogInformation("Ensuring database is created...");
                await context.Database.EnsureCreatedAsync();
                logger.LogInformation("Database created or already exists.");

                // Check if database has any data
                logger.LogInformation("Checking if database needs seeding...");
                if (!context.Users.Any())
                {
                    logger.LogInformation("Seeding initial data...");
                    Console.WriteLine("Seeding initial data...");

                    // Create admin user
                    var adminUser = new ApplicationUser
                    {
                        UserName = "admin@microfinance.app",
                        Email = "admin@microfinance.app",
                        EmailConfirmed = true
                    };

                    var result = await userManager.CreateAsync(adminUser, "Admin123!");
                    if (result.Succeeded)
                    {
                        logger.LogInformation("Admin user created successfully");
                        
                        // Check if the Administrator role exists before adding the user to it
                        if (await context.Roles.AnyAsync(r => r.Name == "Administrator"))
                        {
                            await userManager.AddToRoleAsync(adminUser, "Administrator");
                            logger.LogInformation("Added admin user to Administrator role");
                        }
                        else
                        {
                            logger.LogWarning("Administrator role does not exist, skipping role assignment");
                        }
                    }
                    else
                    {
                        logger.LogError("Failed to create admin user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                    }

                    try
                    {
                        // Create sample client
                        var client = new Client
                        {
                            FirstName = "John",
                            LastName = "Doe",
                            Email = "john.doe@example.com",
                            PhoneNumber = "555-123-4567",
                            Address = "123 Main St, Anytown, USA",
                            DateOfBirth = new DateTime(1985, 6, 15),
                            RegistrationDate = DateTime.Now
                        };

                        context.Clients.Add(client);
                        await context.SaveChangesAsync();
                        logger.LogInformation("Sample client created successfully");
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error creating sample client");
                    }
                    
                    logger.LogInformation("Database seeding completed");
                    Console.WriteLine("Database seeding completed.");
                }
                else
                {
                    logger.LogInformation("Database already contains data, skipping seed");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while initializing the database");
                Console.WriteLine($"Database initialization error: {ex.Message}");
            }
        }

        private static async Task InitializeSQLite(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILogger logger)
        {
            logger.LogInformation("Initializing SQLite database...");
            
            try
            {
                // Ensure database is created - for SQLite this creates the file if it doesn't exist
                await context.Database.EnsureCreatedAsync();
                logger.LogInformation("SQLite database file created or already exists");
                
                // Try to create schema by applying migrations
                try
                {
                    logger.LogInformation("Applying migrations to SQLite database...");
                    await context.Database.MigrateAsync();
                    logger.LogInformation("Migrations applied successfully");
                }
                catch (Exception migrationEx)
                {
                    logger.LogWarning(migrationEx, "Error applying migrations to SQLite. Will try with EnsureCreated instead");
                    
                    // If migrations fail, try to recreate the database
                    await context.Database.EnsureDeletedAsync();
                    await context.Database.EnsureCreatedAsync();
                    logger.LogInformation("SQLite database recreated with schema");
                }
                
                // Continue with data seeding if needed
                if (!context.Users.Any())
                {
                    logger.LogInformation("Seeding initial data in SQLite...");
                    
                    // Create admin user (same as in Initialize method)
                    var adminUser = new ApplicationUser
                    {
                        UserName = "admin@microfinance.app",
                        Email = "admin@microfinance.app",
                        EmailConfirmed = true
                    };

                    var result = await userManager.CreateAsync(adminUser, "Admin123!");
                    if (result.Succeeded)
                    {
                        logger.LogInformation("Admin user created successfully in SQLite");
                        
                        // Add sample client data
                        await SeedSampleDataForSQLite(context, logger);
                    }
                    else
                    {
                        logger.LogError("Failed to create admin user in SQLite: {Errors}", 
                            string.Join(", ", result.Errors.Select(e => e.Description)));
                    }
                    
                    logger.LogInformation("SQLite database seeding completed");
                }
                else
                {
                    logger.LogInformation("SQLite database already contains data, skipping seed");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error initializing SQLite database");
            }
        }
        
        private static async Task SeedSampleDataForSQLite(ApplicationDbContext context, ILogger logger)
        {
            try
            {
                // Create sample client
                var client = new Client
                {
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "john.doe@example.com",
                    PhoneNumber = "555-123-4567",
                    Address = "123 Main St, Anytown, USA",
                    DateOfBirth = new DateTime(1985, 6, 15),
                    RegistrationDate = DateTime.Now
                };

                context.Clients.Add(client);
                await context.SaveChangesAsync();
                logger.LogInformation("Sample client created successfully in SQLite");
                  // Create a sample loan
                var loan = new Loan
                {
                    ClientId = client.Id,
                    Amount = 1000,
                    Term = 12,
                    Purpose = "Home renovation",
                    InterestRate = 5.0m,
                    Status = LoanStatus.Active,
                    ApplicationDate = DateTime.Now.AddDays(-30),
                    ApprovalDate = DateTime.Now.AddDays(-25),
                    MonthlyPayment = 85.61m
                };
                
                context.Loans.Add(loan);
                await context.SaveChangesAsync();
                logger.LogInformation("Sample loan created successfully in SQLite");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error seeding sample data in SQLite");
            }
        }
    }
}
