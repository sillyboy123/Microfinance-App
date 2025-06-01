using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FinPlus.Data;
using FinPlus.Models;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FinPlus.Controllers
{
    [Authorize] // Ensure only logged-in users can apply for loans
    public class LoansController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LoansController(ApplicationDbContext context)
        {
            _context = context;
        }

        private async Task<List<SelectListItem>> GetClientSelectList()
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

        // GET: Loans
        public async Task<IActionResult> Index()
        {
            var loans = await _context.Loans
                .Include(l => l.Client)
                .OrderByDescending(l => l.ApplicationDate)
                .ToListAsync();
            return View(loans);        }        

        // GET: Loans/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            try
            {
                // Check if there are any clients in the system
                var clientsExist = await _context.Clients.AnyAsync();
                if (!clientsExist)
                {
                    TempData["Error"] = "Please add a client before creating a loan application.";
                    return RedirectToAction("Create", "Clients");
                }

                ViewData["Clients"] = await GetClientSelectList();
                return View(new Loan { InterestRate = 12.0m });
            }            catch (Exception)
            {
                TempData["Error"] = "An error occurred while loading the loan application form.";
                return RedirectToAction(nameof(Index));
            }
        }        

        // POST: Loans/Create        
        [HttpPost]
        [ActionName("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePost([Bind("Amount,Term,Purpose,ClientId")] Loan loan)
        {
            try
            {
                // Check if the client exists
                var client = await _context.Clients.FindAsync(loan.ClientId);
                if (client == null)
                {
                    ModelState.AddModelError("ClientId", "Please select a valid client.");
                    ViewData["Clients"] = await GetClientSelectList();
                    return View(loan);
                }

                // Validate required fields and ranges (backup validation in case client-side validation fails)
                if (loan.Amount < 100 || loan.Amount > 1000000)
                {
                    ModelState.AddModelError("Amount", "Amount must be between $100 and $1,000,000");
                }

                if (loan.Term < 1 || loan.Term > 60)
                {
                    ModelState.AddModelError("Term", "Term must be between 1 and 60 months");
                }

                if (string.IsNullOrWhiteSpace(loan.Purpose) || loan.Purpose.Length < 10 || loan.Purpose.Length > 200)
                {
                    ModelState.AddModelError("Purpose", "Purpose must be between 10 and 200 characters");
                }

                if (ModelState.IsValid && client != null)
                {
                    // Set default values
                    loan.ApplicationDate = DateTime.Now;
                    loan.Status = LoanStatus.Pending;
                    loan.InterestRate = 12.0m; // 12% APR default
                    loan.Client = client;

                    // Calculate monthly payment
                    decimal monthlyRate = loan.InterestRate / 12 / 100;
                    loan.MonthlyPayment = loan.Amount * 
                        (monthlyRate * (decimal)Math.Pow(1 + (double)monthlyRate, loan.Term)) / 
                        ((decimal)Math.Pow(1 + (double)monthlyRate, loan.Term) - 1);

                    _context.Add(loan);
                    await _context.SaveChangesAsync();
                    
                    TempData["Success"] = "Loan application submitted successfully!";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "An error occurred while processing your request. Please try again.");
            }

            // If we got this far, something failed, redisplay form
            ViewData["Clients"] = await GetClientSelectList();
            return View(loan);
        }
    }
}
