using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using mvc.Models;
using mvc.Services;
using System.Threading.Tasks;

namespace mvc.Controllers
{
    public class UsersController : BaseController
    {
        private readonly IUserService _userService;

        public UsersController(UserManager<UserAccount> userManager, IUserService userService) : base(userManager)
        {
            _userService = userService;
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            var users = await _userService.GetAllUsersAsync();
            return View(users);
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: Users/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserName,Email,PasswordHash,accountType,PhoneNumber,githubLink,notificationSettings")] UserAccount user, IFormFile profilePhotoFile)
        {
            if (profilePhotoFile != null && profilePhotoFile.Length > 0)
            {
                // Validate file size (max 5MB)
                const long maxFileSize = 5 * 1024 * 1024; // 5MB in bytes
                if (profilePhotoFile.Length > maxFileSize)
                {
                    ModelState.AddModelError("profilePhoto", "The file size cannot exceed 5MB.");
                }

                // Validate file type
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var fileExtension = Path.GetExtension(profilePhotoFile.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("profilePhoto", "Only JPG, JPEG and PNG files are allowed.");
                }

                if (ModelState.IsValid)
                {
                    // Define the path to save the uploaded file
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/users");
                    Directory.CreateDirectory(uploadsFolder); // Ensure the directory exists

                    var uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Save the file to the server
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await profilePhotoFile.CopyToAsync(stream);
                    }

                    // Save the file path in the profilePhoto property
                    user.profilePhoto = $"/uploads/users/{uniqueFileName}";
                }
            }
            else
            {
                ModelState.AddModelError("profilePhoto", "Please upload a profile photo.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Add the user and assign the accountType as a role
                    await _userService.AddUserAsync(user, user.accountType);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }

            return View(user);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(string id)
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



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
    string id,
    [Bind("Id,UserName,Email,PasswordHash,accountType,PhoneNumber,githubLink,notificationSettings")] UserAccount user, string? newPassword, IFormFile? profilePhotoFile)
        {
            Console.WriteLine("Edit method started.");

            if (string.IsNullOrEmpty(id) || id != user.Id)
            {
                Console.WriteLine("Failed: User ID mismatch or missing.");
                return BadRequest("User ID mismatch or missing.");
            }

            var existingUser = await _userService.GetUserByIdAsync(id);
            if (existingUser == null)
            {
                Console.WriteLine("Failed: User not found.");
                return NotFound("User not found.");
            }

            // Handle password change
            if (!string.IsNullOrEmpty(newPassword))
            {

                // Generate a password reset token
                var userManagerUser = await _userManager.FindByIdAsync(id);
                if (userManagerUser == null)
                {
                    return NotFound("User not found in UserManager.");
                }

                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(userManagerUser);

                // Reset the password using the token
                var passwordChangeResult = await _userManager.ResetPasswordAsync(userManagerUser, resetToken, newPassword);
                if (!passwordChangeResult.Succeeded)
                {
                    foreach (var error in passwordChangeResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return RedirectToAction("Edit", new { id });
                }
            }

            // Update other fields
            if (!string.IsNullOrEmpty(user.UserName) && user.UserName != existingUser.UserName)
            {
                existingUser.UserName = user.UserName;
                existingUser.NormalizedUserName = user.UserName.ToUpperInvariant();
            }

            if (!string.IsNullOrEmpty(user.Email) && user.Email != existingUser.Email)
            {
                existingUser.Email = user.Email;
                existingUser.NormalizedEmail = user.Email.ToUpperInvariant();
            }

            if (!string.IsNullOrEmpty(user.PhoneNumber) && user.PhoneNumber != existingUser.PhoneNumber)
                existingUser.PhoneNumber = user.PhoneNumber;

            if (!string.IsNullOrEmpty(user.githubLink) && user.githubLink != existingUser.githubLink)
                existingUser.githubLink = user.githubLink;

            if (user.notificationSettings != existingUser.notificationSettings)
                existingUser.notificationSettings = user.notificationSettings;

            if (!string.IsNullOrEmpty(user.accountType) && user.accountType != existingUser.accountType)
                existingUser.accountType = user.accountType;

            // Handle profile photo upload if needed
            if (profilePhotoFile != null && profilePhotoFile.Length > 0)
            {
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
                        ModelState.AddModelError("profilePhotoFile", "Only JPG, JPEG and PNG files are allowed.");
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

                        // Delete old photo if exists
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
            }
            else
            {
                // Retain the existing photo
                user.profilePhoto = existingUser.profilePhoto;
            }

            if (!ModelState.IsValid)
                return RedirectToAction("Edit", new { id });

            await _userService.UpdateUserAsync(existingUser);
            return RedirectToAction("Details", new { id });
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("User ID cannot be null or empty.");
            }

            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                {
                    return NotFound("User not found.");
                }

                // Delete profile photo from disk if not default
                if (!string.IsNullOrEmpty(user.profilePhoto) && user.profilePhoto != "/images/default.png")
                {
                    var photoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.profilePhoto.TrimStart('/'));
                    if (System.IO.File.Exists(photoPath))
                    {
                        System.IO.File.Delete(photoPath);
                    }
                }

                // Call the service to delete the user
                await _userService.DeleteUserAsync(user);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "An error occurred while deleting the user.");
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeAccSettings(string id, [Bind("Id,UserName,Email,PasswordHash,accountType,PhoneNumber,githubLink,notificationSettings")] UserAccount user, string? currentPassword, string? newPassword, string? confirmPassword)
        {
            Console.WriteLine("ChangeAccSettings method started.");

            if (string.IsNullOrEmpty(id) || id != user.Id)
            {
                return BadRequest("User ID mismatch or missing.");
            }

            var existingUser = await _userService.GetUserByIdAsync(id);
            if (existingUser == null)
            {
                return NotFound("User not found.");
            }

            // Handle password change
            if (!string.IsNullOrEmpty(currentPassword) && !string.IsNullOrEmpty(newPassword) && !string.IsNullOrEmpty(confirmPassword))
            {

                if (newPassword != confirmPassword)
                {
                    Console.WriteLine("Failed: New password and confirmation do not match.");
                    ModelState.AddModelError("confirmPassword", "The new password and confirmation do not match.");
                    return RedirectToAction("Edit", new { id }); // Redirect to the Edit view with the user ID
                }

                var userManagerUser = await _userManager.FindByIdAsync(id);
                if (userManagerUser == null)
                {
                    return NotFound("User not found in UserManager.");
                }

                if (!await _userManager.CheckPasswordAsync(userManagerUser, currentPassword))
                {
                    ModelState.AddModelError("currentPassword", "The current password is incorrect.");
                    return RedirectToAction("Edit", new { id });
                }

                var passwordChangeResult = await _userManager.ChangePasswordAsync(userManagerUser, currentPassword, newPassword);
                if (!passwordChangeResult.Succeeded)
                {
                    foreach (var error in passwordChangeResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return RedirectToAction("Edit", new { id }); // Redirect to the Edit view with the user ID
                }

                Console.WriteLine("Password change succeeded.");
            }

            // Update other fields
            if (!string.IsNullOrEmpty(user.UserName) && user.UserName != existingUser.UserName)
            {
                existingUser.UserName = user.UserName;
                existingUser.NormalizedUserName = user.UserName.ToUpperInvariant();
            }

            if (!string.IsNullOrEmpty(user.Email) && user.Email != existingUser.Email)
            {
                existingUser.Email = user.Email;
                existingUser.NormalizedEmail = user.Email.ToUpperInvariant();
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

            if (!ModelState.IsValid)
            {
                return RedirectToAction("Edit", new { id });
            }

            await _userService.UpdateUserAsync(existingUser);
            return RedirectToAction("UserProfile", "Home", new { id });

        }

        [HttpPost]
        public async Task<IActionResult> ResetProfilePhoto([FromBody] string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest(new { error = "User ID cannot be null or empty." });
            }

            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { error = "User not found." });
            }

            if (!string.IsNullOrEmpty(user.profilePhoto))
            {
                var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.profilePhoto.TrimStart('/'));
                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }
            }

            // Reset the profile photo to the default path
            user.profilePhoto = "/images/default.png";

            await _userService.UpdateUserAsync(user);

            return Ok(new { message = "Profile photo reset successfully." });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeProfilePhoto(string id, IFormFile profilePhotoFile)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("User ID cannot be null or empty.");
            }

            var existingUser = await _userService.GetUserByIdAsync(id);
            if (existingUser == null)
            {
                return NotFound("User not found.");
            }

            if (profilePhotoFile != null && profilePhotoFile.Length > 0)
            {
                const long maxFileSize = 5 * 1024 * 1024; // 5MB
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var fileExtension = Path.GetExtension(profilePhotoFile.FileName).ToLower();

                if (profilePhotoFile.Length > maxFileSize)
                {
                    ModelState.AddModelError("photoPathFile", "The file size cannot exceed 5MB.");
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

                    if (!string.IsNullOrEmpty(existingUser.profilePhoto) && existingUser.profilePhoto != "/images/default.png")
                    {
                        var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingUser.profilePhoto.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    existingUser.profilePhoto = $"/uploads/users/{uniqueFileName}";

                    await _userService.UpdateUserAsync(existingUser);
                    return RedirectToAction("UserProfile", "Home");
                }
            }

            return RedirectToAction("UserProfile", "Home");
        }
    }
}