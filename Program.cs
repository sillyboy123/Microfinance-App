using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using FinPlus.Data;
using FinPlus.Data.Identity;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Add MVC and Razor Pages services
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Add database context
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Create a logger factory for database configuration
var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var dbLogger = loggerFactory.CreateLogger("DatabaseConfiguration");

builder.Services.AddDbContext<ApplicationDbContext>(options => {
    // Use our enhanced database configuration helper with automatic fallback
    DatabaseConfigHelper.ConfigureWithFallback(
        options, 
        connectionString ?? "Server=localhost\\SQLEXPRESS;Database=MicrofinanceApp;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True",
        "FinPlus.db",
        dbLogger);
});

var identityBuilder = builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireDigit = true;
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure cookie settings
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Ensure database is created and apply migrations
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var logger = services.GetRequiredService<ILogger<Program>>();
        
        logger.LogInformation("Attempting to connect to the database and apply migrations...");
        Console.WriteLine("Checking database connection...");
        
        // First check database connection
        bool canConnect = false;
        try {
            canConnect = context.Database.CanConnect();
            Console.WriteLine($"Database connection test result: {(canConnect ? "Success" : "Failed")}");
            logger.LogInformation($"Database connection test result: {(canConnect ? "Success" : "Failed")}");
        }
        catch (Exception connEx) {
            Console.WriteLine($"Error testing database connection: {connEx.Message}");
            logger.LogError(connEx, "Error testing database connection");
        }
          if (canConnect) {
            context.Database.Migrate();
            logger.LogInformation("Database migrated successfully.");
            Console.WriteLine("Database migrated successfully.");
            
            // Initialize database with sample data if needed
            await DbInitializer.Initialize(context, userManager, logger);
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating or initializing the database.");
        Console.WriteLine($"Database migration error: {ex.Message}");
        
        if (ex.InnerException != null) {
            Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
            logger.LogError(ex.InnerException, "Inner exception details");
        }
    }
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

// Create special route for database connection testing
app.MapGet("/db-test", context => {
    context.Response.Redirect("/DbTestWelcome");
    return Task.CompletedTask;
});

app.Run();
