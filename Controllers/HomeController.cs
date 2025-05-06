using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mvc.data.models;
using mvc.Data;
using mvc.Models;
using mvc.Services;
using mvc.ViewModels;

namespace mvc.Controllers
{
    public class HomeController : BaseController
    {
        private readonly IUserService _userService;
        private readonly UserManager<UserAccount> _userManager;
        private readonly IAccountService _accountService;

        public HomeController(IUserService userService, UserManager<UserAccount> userManager, IAccountService accountService) : base(userManager)
        {
            _userService = userService;
            _userManager = userManager;
            _accountService = accountService;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Index"; // Set the ViewData["Title"]
            return View();
        }

        public IActionResult AccessDenied()
        {
            ViewData["Title"] = "AccessDenied";
            return View();
        }
        public async Task<IActionResult> User()
        {
            ViewData["Title"] = "User";
            var user = await _userManager.GetUserAsync(HttpContext.User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return View(user);
        }

        // GET: UserSettings/5
        public async Task<IActionResult> UserSettings(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("User ID cannot be null or empty.");
            }

            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            return View(user);
        }

        // POST: UserSettings/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UserSettings(string id, [Bind("Id,UserName,Email,PasswordHash,accountType,PhoneNumber,githubLink,notificationSettings")] UserAccount user, IFormFile? profilePhotoFile)
        {
            if (string.IsNullOrEmpty(id) || id != user.Id)
            {
                return BadRequest("User ID mismatch or missing.");
            }

            var existingUser = await _userService.GetUserByIdAsync(id);
            if (existingUser == null)
            {
                return NotFound("User not found.");
            }

            // Update only the fields that are changed
            if (!string.IsNullOrEmpty(user.UserName) && user.UserName != existingUser.UserName)
            {
                existingUser.UserName = user.UserName;
            }

            if (!string.IsNullOrEmpty(user.Email) && user.Email != existingUser.Email)
            {
                existingUser.Email = user.Email;
            }

            if (!string.IsNullOrEmpty(user.PasswordHash) && user.PasswordHash != existingUser.PasswordHash)
            {
                existingUser.PasswordHash = user.PasswordHash;
            }

            if (!string.IsNullOrEmpty(user.PhoneNumber) && user.PhoneNumber != existingUser.PhoneNumber)
            {
                existingUser.PhoneNumber = user.PhoneNumber;
            }

            if (!string.IsNullOrEmpty(user.githubLink) && user.githubLink != existingUser.githubLink)
            {
                existingUser.githubLink = user.githubLink;
            }

            if (user.notificationSettings != existingUser.notificationSettings)
            {
                existingUser.notificationSettings = user.notificationSettings;
            }

            if (!string.IsNullOrEmpty(user.accountType) && user.accountType != existingUser.accountType)
            {
                existingUser.accountType = user.accountType;
            }

            // Handle profile photo upload if a new file is provided
            if (profilePhotoFile != null && profilePhotoFile.Length > 0)
            {
                const long maxFileSize = 5 * 1024 * 1024; // 5MB
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var fileExtension = Path.GetExtension(profilePhotoFile.FileName).ToLower();

                if (profilePhotoFile.Length > maxFileSize)
                {
                    ModelState.AddModelError("profilePhotoFile", "The file size cannot exceed 5MB.");
                }

                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("photoPathFile", "Only JPG, JPEG, and PNG files are allowed.");
                }

                if (ModelState.IsValid)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/users");
                    Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await profilePhotoFile.CopyToAsync(stream);
                    }

                    if (!string.IsNullOrEmpty(existingUser.profilePhoto))
                    {
                        var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingUser.profilePhoto.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    existingUser.profilePhoto = $"/uploads/users/{uniqueFileName}";
                }
            }

            if (!ModelState.IsValid)
            {
                return View(existingUser);
            }

            try
            {
                await _userService.UpdateUserAsync(existingUser);
                return RedirectToAction(nameof(User));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(existingUser);
            }
        }
    }
}