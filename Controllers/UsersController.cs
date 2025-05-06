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

        

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,UserName,Email,accountType,notificationSettings,githubLink,PhoneNumber")] UserAccount user, IFormFile profilePhotoFile)
        {
            if (id != user.Id)
            {
                return BadRequest("User ID mismatch.");
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
                    ModelState.AddModelError("photoPathFile", "Only JPG, JPEG and PNG files are allowed.");
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

                    user.profilePhoto = $"/uploads/users/{uniqueFileName}";
                }
            }
            else
            {
                user.profilePhoto = existingUser.profilePhoto;
            }

            if (!ModelState.IsValid)
            {
                return View(user);
            }

            try
            {
                await _userService.UpdateUserAsync(user);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(user);
            }
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

                // Call the service to delete the user
                await _userService.DeleteUserAsync(user);

                Console.WriteLine($"User with ID: {id} deleted successfully.");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting user with ID: {id}. Exception: {ex.Message}");
                ModelState.AddModelError(string.Empty, "An error occurred while deleting the user.");
                return View();
            }
        }
    }
}
