using System.Diagnostics;
using System.Security.Claims;
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
        private readonly IClassroomStudentsService _classroomStudentsService;
        private readonly IAssignmentService _assignmentService;
        private readonly IDocumentService _documentService;

        public HomeController(
        IUserService userService, 
        UserManager<UserAccount> userManager, 
        IAccountService accountService, 
        IClassroomStudentsService classroomStudentsService,
        IAssignmentService assignmentService,
        IDocumentService documentService) : base(userManager)
        {   
            _userService = userService;
            _userManager = userManager;
            _accountService = accountService;
            _classroomStudentsService = classroomStudentsService;
            _assignmentService = assignmentService;
            _documentService = documentService;
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

            // Reset the profile photo to the default path
            user.profilePhoto = "/images/default.png";

            await _userService.UpdateUserAsync(user);
            return RedirectToAction(nameof(User));
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
            else if (profilePhotoFile != null && profilePhotoFile.Length > 0)
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
                    ModelState.AddModelError("profilePhotoFile", "Only JPG, JPEG, and PNG files are allowed.");
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

        // Store the User ID in the session when the user logs in or is authenticated
        public async Task<IActionResult> User()
        {
            ViewData["Title"] = "User";
            var user = await _userManager.GetUserAsync(HttpContext.User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Store the User ID in the session
            HttpContext.Session.SetString("UserId", user.Id);

            return View(user);
        }

        public async Task<IActionResult> Home()
        {
            var id = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(id))
            {
                var user = await _userManager.GetUserAsync(HttpContext.User);
                if (user != null)
                {
                    id = user.Id;
                    HttpContext.Session.SetString("UserId", id);
                }
                else
                {
                    return RedirectToAction("Login", "Account");
                }
            }

            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("User ID cannot be null or empty.");
            }

            var existingUser = await _userService.GetUserByIdAsync(id);
            if (existingUser == null)
            {
                return NotFound("User not found.");
            }

            var classrooms = await _classroomStudentsService.GetClassroomsByUserIdAsync(id);
            var classroomViewModels = new List<ClassroomViewModel>();

            foreach (var classroom in classrooms)
            {
                if (!string.IsNullOrEmpty(classroom.professorId))
                {
                    var professor = await _userService.GetUserByIdAsync(classroom.professorId);
                    classroomViewModels.Add(new ClassroomViewModel
                    {
                        Classroom = classroom,
                        ProfessorName = professor?.UserName,
                        ProfessorPhoto = professor?.profilePhoto
                    });
                }
            }

            var viewModel = new UserClassroomsViewModel
            {
                UserAccount = existingUser,
                Classrooms = classroomViewModels // Pass the populated classroomViewModels
            };

            return View(viewModel);
        }

        public async Task<IActionResult> ClassroomFlux(int id)
        {
            // Retrieve the user from the current context or session
            var user = await _userManager.GetUserAsync(HttpContext.User);
            string userId = user?.Id ?? HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User ID cannot be null or empty.");
            }

            // Retrieve the current user using the user service
            var userAccount = await _userService.GetUserByIdAsync(userId);
            if (userAccount == null)
            {
                return NotFound("User not found.");
            }

            // Retrieve the classroom with the given ID
            var classroom = await _classroomStudentsService.GetClassroomByIdAsync(id);
            if (classroom == null)
            {
                return NotFound("Classroom not found.");
            }

            // Retrieve assignments for the classroom
            var assignments = await _assignmentService.GetAssignmentsByClassroomIdAsync(id);

            // Retrieve assignment chats for each assignment in the classroom
            var assignmentChats = new List<AssignmentChat>();
            foreach (var assignment in assignments)
            {
                var chats = await _assignmentService.GetAssignmentChatsByAssignmentIdAsync(assignment.id);
                assignmentChats.AddRange(chats);
            }

            // Retrieve documents for each assignment in the classroom
            var documents = new List<Document>();
            foreach (var assignment in assignments)
            {
                var assignmentDocuments = await _documentService.GetDocumentsByAssignmentIdAsync(assignment.id);
                documents.AddRange(assignmentDocuments);
            }

            // Populate the ClassroomFluxViewModel
            var viewModel = new ClassroomFluxViewModel
            {
                Classroom = classroom,
                Assignments = assignments.ToList(),
                AssignmentChats = assignmentChats.ToList(),
                Documents = documents.ToList(),
                UserAccount = userAccount,
                Users = (await _userService.GetAllUsersAsync()).ToList()
            };

            return View(viewModel);
        }

    }
}