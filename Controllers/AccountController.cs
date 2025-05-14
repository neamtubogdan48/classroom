// Controllers/AccountController.cs
using Microsoft.AspNetCore.Mvc;
using mvc.Models;
using mvc.Services;
using mvc.ViewModels;

namespace mvc.Controllers;
public class AccountController : Controller
{
    private readonly IAccountService _accountService;
    private readonly INotificationService _notificationService;

    public AccountController(IAccountService accountService, INotificationService notificationService)
    {
        _accountService = accountService;
        _notificationService = notificationService;
    }

    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _accountService.AuthenticateUserAsync(model.Email, model.Password);
        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(model);
        }

        return RedirectToAction("Home", "Home");
    }

    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _accountService.RegisterUserAsync(model);
        if (result.Succeeded)
        {
            await _accountService.AddUserRoleAsync(model.Email, model.accountType);
            return RedirectToAction("Login", "Account");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);
    }

    public async Task<IActionResult> Logout()
    {
        HttpContext.Session.Clear();
        await _accountService.LogoutAsync();
        return RedirectToAction("Home", "Home");
    }

    public IActionResult RequestReset()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RequestReset(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            ModelState.AddModelError(string.Empty, "Email is required.");
            return View();
        }

        // Get all admin users
        var adminUsers = await _accountService.GetUsersByAccountTypeAsync("Admin");

        // Create a notification for each admin
        foreach (var admin in adminUsers)
        {
            var notification = new Notification
            {
                userId = admin.Id,
                name = "Password Reset Requested",
                description = $"A password reset was requested for {email}.",
                timeSent = DateTime.UtcNow.AddHours(3)
            };
            await _notificationService.AddNotificationAsync(notification);
        }

        ViewData["Message"] = "Your request has been sent to the administrators.";
        return RedirectToAction("Login", "Account");
    }
}
