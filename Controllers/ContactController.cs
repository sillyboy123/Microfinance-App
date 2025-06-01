using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MicrofinanceApp.Data;

namespace MicrofinanceApp.Controllers
{
    public class ContactController : Controller
    {
        private readonly ILogger<ContactController> _logger;
        private readonly ApplicationDbContext _context;

        public ContactController(ILogger<ContactController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(string name, string email, string subject, string message)
        {
            // Here you would typically process the contact form submission
            // For example, sending an email or saving to database
            
            // Log the form submission for now
            _logger.LogInformation($"Contact form submitted by {name} ({email}): {subject}");
            
            // Add a success message
            TempData["SuccessMessage"] = "Thank you for your message! We will get back to you soon.";
            
            return RedirectToAction("Index");
        }
    }
}
