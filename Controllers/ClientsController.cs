using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MicrofinanceApp.Data;
using MicrofinanceApp.Models;

namespace MicrofinanceApp.Controllers
{
    public class ClientsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ClientsController(ApplicationDbContext context)
        {
            _context = context;
        }        // GET: Clients
        public async Task<IActionResult> Index()
        {
            return View(await _context.Clients
                .Include(c => c.Loans)
                .OrderByDescending(c => c.RegistrationDate)
                .ToListAsync());
        }

        // GET: Clients/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var client = await _context.Clients
                .FirstOrDefaultAsync(m => m.Id == id);
            if (client == null)
            {
                return NotFound();
            }

            return View(client);
        }

        // GET: Clients/Create
        public IActionResult Create()
        {
            return View();
        }        // POST: Clients/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FirstName,LastName,Email,PhoneNumber,Address,DateOfBirth")] Client client)
        {
            try
            {
                Console.WriteLine($"Received client data: FirstName={client.FirstName}, LastName={client.LastName}, Email={client.Email}");
                
                // Add manual validation since we're using required keyword modifiers                // Check if client is null
                if (client == null)
                {
                    ModelState.AddModelError(string.Empty, "No client data was received.");
                    return View(new Client());
                }
                
                if (string.IsNullOrEmpty(client.FirstName) || 
                    string.IsNullOrEmpty(client.LastName) || 
                    string.IsNullOrEmpty(client.Email) || 
                    string.IsNullOrEmpty(client.PhoneNumber) || 
                    string.IsNullOrEmpty(client.Address))
                {
                    Console.WriteLine("Validation failed: One or more required fields are empty");
                    foreach (var key in ModelState.Keys)
                    {
                        var state = ModelState[key];
                        if (state.Errors.Count > 0)
                        {
                            Console.WriteLine($"Error in {key}: {string.Join(", ", state.Errors.Select(e => e.ErrorMessage))}");
                        }
                    }
                    ModelState.AddModelError(string.Empty, "All fields are required.");
                    return View(client);
                }
                
                if (ModelState.IsValid)
                {
                    Console.WriteLine("Model is valid, proceeding with client creation");
                    
                    // Ensure RegistrationDate is set
                    client.RegistrationDate = DateTime.Now;
                    
                    // Add to context
                    _context.Clients.Add(client);
                    
                    // Save changes to database
                    await _context.SaveChangesAsync();
                    Console.WriteLine($"Client saved successfully with ID: {client.Id}");
                    
                    // Redirect to the index page
                    TempData["SuccessMessage"] = $"Client {client.FirstName} {client.LastName} registered successfully.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    Console.WriteLine("ModelState is invalid:");
                    foreach (var key in ModelState.Keys)
                    {
                        var state = ModelState[key];
                        if (state.Errors.Count > 0)
                        {
                            Console.WriteLine($"Error in {key}: {string.Join(", ", state.Errors.Select(e => e.ErrorMessage))}");
                        }
                    }
                }
                
                // If we got this far, something failed, redisplay form
                return View(client);
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Exception in Create action: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                
                ModelState.AddModelError(string.Empty, $"Error creating client: {ex.Message}");
                return View(client);
            }
        }

        // GET: Clients/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var client = await _context.Clients.FindAsync(id);
            if (client == null)
            {
                return NotFound();
            }
            return View(client);
        }

        // POST: Clients/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FirstName,LastName,Email,PhoneNumber,Address,DateOfBirth")] Client client)
        {
            if (id != client.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(client);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClientExists(client.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(client);
        }

        private bool ClientExists(int id)
        {
            return _context.Clients.Any(e => e.Id == id);
        }
    }
}
