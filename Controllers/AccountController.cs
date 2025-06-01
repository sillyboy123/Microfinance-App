using Microsoft.AspNetCore.Mvc;
using MicrofinanceApp.ViewModels;
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;
using MicrofinanceApp.Data.Identity;

namespace MicrofinanceApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            return View(new LoginViewModel { ReturnUrl = returnUrl ?? "/" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(
                    model.Email,
                    model.Password,
                    model.RememberMe,
                    lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    {
                        return Redirect(model.ReturnUrl);
                    }
                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!model.AcceptTerms)
            {
                ModelState.AddModelError("AcceptTerms", "You must accept the terms and conditions");
            }

            if (ModelState.IsValid)
            {
                var user = new ApplicationUser 
                { 
                    UserName = model.Email, 
                    Email = model.Email,
                    RegisteredAt = DateTime.UtcNow
                };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: true);
                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            var viewModel = new ProfileViewModel
            {
                Email = user.Email ?? string.Empty,
                UserName = user.UserName ?? string.Empty,
                EmailConfirmed = user.EmailConfirmed,
                PhoneNumber = user.PhoneNumber,
                MemberSince = user.RegisteredAt
            };

            return View(viewModel);
        }        [HttpGet]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> Settings()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToAction("Login", new { returnUrl = "/Account/Settings" });
                }

                var viewModel = new SettingsViewModel
                {
                    Email = user.Email ?? string.Empty,
                    PhoneNumber = user.PhoneNumber,
                    // Default settings that would typically come from the user's preferences in the database
                    ReceiveEmailNotifications = true,
                    ReceiveSmsNotifications = user.PhoneNumber != null
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error in Settings GET: {ex.Message}");
                ModelState.AddModelError(string.Empty, "An error occurred while loading settings.");
                return View(new SettingsViewModel { Email = User.Identity?.Name ?? string.Empty });
            }
        }        [HttpPost]
        [ValidateAntiForgeryToken]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> Settings(SettingsViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToAction("Login", new { returnUrl = "/Account/Settings" });
                }

                var changesMade = false;

                // Update phone number if it has changed
                if (model.PhoneNumber != user.PhoneNumber)
                {
                    var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, model.PhoneNumber);
                    if (!setPhoneResult.Succeeded)
                    {
                        foreach (var error in setPhoneResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                        return View(model);
                    }
                    changesMade = true;
                }

                // Change password if a new one was provided
                if (!string.IsNullOrEmpty(model.CurrentPassword) && !string.IsNullOrEmpty(model.NewPassword))
                {
                    var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
                    if (!changePasswordResult.Succeeded)
                    {
                        foreach (var error in changePasswordResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                        return View(model);
                    }
                    changesMade = true;
                }

                // In a real app, save notification preferences to user's settings in database
                // For this demo, we'll just acknowledge the settings were saved
                
                if (changesMade)
                {
                    await _signInManager.RefreshSignInAsync(user);
                    model.StatusMessage = "Your account settings have been updated";
                }
                else
                {
                    model.StatusMessage = "No changes were made";
                }

                return View(model);
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error in Settings POST: {ex.Message}");
                ModelState.AddModelError(string.Empty, "An error occurred while saving your settings.");
                return View(model);
            }
        }
    }
}
