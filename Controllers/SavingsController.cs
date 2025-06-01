using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FinPlus.Data;
using FinPlus.Models;
using Microsoft.AspNetCore.Authorization;

namespace FinPlus.Controllers
{
    [Authorize]
    public class SavingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SavingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Savings
        public async Task<IActionResult> Index()
        {
            var accounts = await _context.SavingsAccounts
                .Include(s => s.Client)
                .OrderByDescending(s => s.OpeningDate)
                .ToListAsync();
            return View(accounts);
        }        // GET: Savings/Create
        public async Task<IActionResult> Create()
        {
            var clients = await _context.Clients
                .OrderBy(c => c.FirstName)
                .ThenBy(c => c.LastName)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.FirstName} {c.LastName} ({c.Email})"
                })
                .ToListAsync();

            ViewData["Clients"] = clients;
            return View(new SavingsAccount());
        }
        
        // POST: Savings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ClientId,Balance,InterestRate")] SavingsAccount account)
        {
            // Generate account number and set default values before validation
            account.AccountNumber = GenerateAccountNumber();
            account.OpeningDate = DateTime.Now;
            account.Status = AccountStatus.Active;

            // Log the model state for debugging
            Console.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");
            Console.WriteLine($"Form data received - ClientId: {account.ClientId}, Balance: {account.Balance}, InterestRate: {account.InterestRate}, AccountNumber: {account.AccountNumber}");
              
            // Remove validation error for AccountNumber since we're generating it
            ModelState.Remove("AccountNumber");
            
            // Log individual validation errors if any
            foreach (var key in ModelState.Keys)
            {
                var state = ModelState[key];
                if (state != null && state.Errors.Count > 0)
                {
                    foreach (var error in state.Errors)
                    {
                        Console.WriteLine($"Validation error for {key}: {error.ErrorMessage ?? "Unknown error"}");
                    }
                }
            }

            // Validate ClientId is provided
            if (account.ClientId <= 0)
            {
                ModelState.AddModelError("ClientId", "Please select a valid client");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var client = await _context.Clients.FindAsync(account.ClientId);
                    if (client == null)
                    {
                        ModelState.AddModelError("ClientId", "Selected client not found.");
                        var clients = await GetClientsSelectListAsync();
                        ViewData["Clients"] = clients;
                        return View(account);
                    }                    // Account number, opening date and status are already set at the beginning of the method

                    Console.WriteLine($"Adding new account: {account.AccountNumber} for client {client.FirstName} {client.LastName}");
                    _context.SavingsAccounts.Add(account);
                    await _context.SaveChangesAsync();
                    
                    Console.WriteLine("Account saved successfully!");
                    TempData["Success"] = "Savings account created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception: {ex.Message}");
                    Console.WriteLine($"StackTrace: {ex.StackTrace}");
                    
                    ModelState.AddModelError("", $"An error occurred while saving the account: {ex.Message}");
                    var clients = await GetClientsSelectListAsync();
                    ViewData["Clients"] = clients;
                    return View(account);
                }
            }
            else
            {
                Console.WriteLine("ModelState is invalid. Returning to view with errors.");
            }

            // If we got this far, something failed; redisplay form
            var clientsList = await GetClientsSelectListAsync();
            ViewData["Clients"] = clientsList;
            return View(account);
        }
        
        // Helper method to get clients for dropdown
        private async Task<List<SelectListItem>> GetClientsSelectListAsync()
        {
            return await _context.Clients
                .OrderBy(c => c.FirstName)
                .ThenBy(c => c.LastName)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.FirstName} {c.LastName} ({c.Email})"
                })
                .ToListAsync();
        }

        private string GenerateAccountNumber()
        {
            // Generate a unique 10-digit account number starting with "SA"
            var random = new Random();
            var number = random.Next(100000, 999999).ToString();
            return $"SA{number}";
        }
    }
}