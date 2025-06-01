using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MicrofinanceApp.Data;
using MicrofinanceApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace MicrofinanceApp.Controllers
{
    [Authorize]
    public class TransactionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TransactionsController> _logger;

        public TransactionsController(ApplicationDbContext context, ILogger<TransactionsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Transactions
        public async Task<IActionResult> Index()
        {
            try 
            {
                var transactions = await _context.Transactions
                    .Include(t => t.Client)
                    .OrderByDescending(t => t.TransactionDate)
                    .ToListAsync();

                _logger.LogInformation($"Retrieved {transactions.Count} transactions");

                var summary = new
                {
                    TotalAmount = transactions.Sum(t => t.Amount),
                    CompletedCount = transactions.Count(t => t.Status == TransactionStatus.Completed),
                    PendingAmount = transactions.Where(t => t.Status == TransactionStatus.Pending)
                        .Sum(t => t.Amount),
                    TodayCount = transactions.Count(t => t.TransactionDate.Date == DateTime.Today)
                };

                ViewBag.Summary = summary;
                return View(transactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transactions");
                TempData["Error"] = "An error occurred while loading transactions.";
                return View(new List<Transaction>());
            }
        }        // GET: Transactions/Create
        public async Task<IActionResult> Create()
        {
            _logger.LogInformation("Transactions/Create GET action accessed");
            
            try {
                var clients = await _context.Clients
                    .OrderBy(c => c.FirstName)
                    .ThenBy(c => c.LastName)
                    .Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = $"{c.FirstName} {c.LastName} ({c.Email})"
                    })
                    .ToListAsync();
                
                _logger.LogInformation($"Found {clients.Count} clients for dropdown");
                ViewData["Clients"] = clients;
                
                return View(new Transaction());
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Error in Transactions/Create GET action");
                TempData["Error"] = "An error occurred while loading the create transaction page.";
                return RedirectToAction(nameof(Index));
            }
        }        // POST: Transactions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ClientId,Amount,Type,Description")] Transaction transaction)
        {
            try
            {
                _logger.LogInformation($"Attempting to create transaction for client {transaction.ClientId}");

                // Force model state to be valid for these properties since we'll set them ourselves
                ModelState.Remove("Reference");
                ModelState.Remove("TransactionDate");
                ModelState.Remove("Status");
                
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Transaction model state is invalid");
                    foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                    {
                        _logger.LogWarning($"Validation error: {error.ErrorMessage}");
                    }
                    
                    ViewData["Clients"] = await GetClientSelectList();
                    return View(transaction);
                }
                
                // Validate that required fields are provided
                if (transaction.ClientId <= 0)
                {
                    ModelState.AddModelError("ClientId", "Please select a valid client.");
                    ViewData["Clients"] = await GetClientSelectList();
                    return View(transaction);
                }
                
                if (transaction.Amount <= 0)
                {
                    ModelState.AddModelError("Amount", "Amount must be greater than zero.");
                    ViewData["Clients"] = await GetClientSelectList();
                    return View(transaction);
                }

                var client = await _context.Clients.FindAsync(transaction.ClientId);
                if (client == null)
                {
                    _logger.LogError($"Client with ID {transaction.ClientId} not found");
                    ModelState.AddModelError("ClientId", "Selected client not found.");
                    ViewData["Clients"] = await GetClientSelectList();
                    return View(transaction);
                }

                transaction.Reference = GenerateTransactionReference();
                transaction.Status = TransactionStatus.Completed;
                transaction.TransactionDate = DateTime.Now;

                _logger.LogInformation($"Adding transaction {transaction.Reference} to database");
                _context.Add(transaction);
                
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Transaction {transaction.Reference} saved successfully");

                TempData["Success"] = "Transaction recorded successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating transaction for client {transaction.ClientId}");
                ModelState.AddModelError(string.Empty, "An error occurred while processing your request. Please try again.");
                ViewData["Clients"] = await GetClientSelectList();
                return View(transaction);
            }        }

        private async Task<IEnumerable<SelectListItem>> GetClientSelectList()
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

        private string GenerateTransactionReference()
        {
            return $"TRX-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
        }
    }
}
