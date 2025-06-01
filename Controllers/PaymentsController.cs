using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FinPlus.Data;
using FinPlus.Models;

namespace FinPlus.Controllers
{
    public class PaymentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PaymentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Payments
        public async Task<IActionResult> Index()
        {
            return View(await _context.Payments
                .Include(p => p.Loan)
                .ThenInclude(l => l.Client)
                .ToListAsync());
        }

        // GET: Payments/Create
        public IActionResult Create()
        {
            var activeLoans = _context.Loans
                .Include(l => l.Client)
                .Where(l => l.Status == LoanStatus.Active)
                .Select(l => new
                {
                    l.Id,
                    DisplayText = $"Loan #{l.Id} - {(l.Client != null ? l.Client.FullName : "Unknown Client")} - ${l.MonthlyPayment}"
                })
                .ToList();

            ViewData["LoanId"] = new SelectList(activeLoans, "Id", "DisplayText");
            return View();
        }

        // POST: Payments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("LoanId,Amount")] Payment payment)
        {
            if (ModelState.IsValid)
            {
                payment.Status = PaymentStatus.Successful;
                payment.TransactionReference = GenerateTransactionReference();
                
                _context.Add(payment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }            var activeLoans = _context.Loans
                .Include(l => l.Client)
                .Where(l => l.Status == LoanStatus.Active)
                .Select(l => new
                {
                    l.Id,
                    DisplayText = $"Loan #{l.Id} - {(l.Client != null ? l.Client.FullName : "Unknown Client")} - ${l.MonthlyPayment}"
                })
                .ToList();

            ViewData["LoanId"] = new SelectList(activeLoans, "Id", "DisplayText", payment.LoanId);
            return View(payment);
        }

        private string GenerateTransactionReference()
        {
            return $"TXN-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8)}";
        }
    }
}
