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
        private readonly IClassroomService _classroomService;
        private readonly INotificationService _notificationService;

        public HomeController(
        IUserService userService, 
        UserManager<UserAccount> userManager, 
        IAccountService accountService, 
        IClassroomStudentsService classroomStudentsService,
        IAssignmentService assignmentService,
        IDocumentService documentService,
        IClassroomService classroomService,
        INotificationService notificationService) : base(userManager)
        {   
            _userService = userService;
            _userManager = userManager;
            _accountService = accountService;
            _classroomStudentsService = classroomStudentsService;
            _assignmentService = assignmentService;
            _documentService = documentService;
            _classroomService = classroomService;
            _notificationService = notificationService;
        }   

        public IActionResult AccessDenied()
        {
            ViewData["Title"] = "AccessDenied";
            return View();
        }
        public async Task<IActionResult> Controllers()
        {
            ViewData["Title"] = "Controllers";
            var user = await _userManager.GetUserAsync(HttpContext.User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

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
                    HttpContext.Session.SetString("AccountType", user.accountType);
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
            var classroomViewModels = await _classroomService.MapClassroomsToViewModelsAsync(classrooms);

            var allClassrooms = await _classroomService.GetAllClassroomsAsync();
            var allClassroomViewModels = await _classroomService.MapClassroomsToViewModelsAsync(allClassrooms);

            var userClassroomsViewModel = new UserClassroomsViewModel
            {
                UserAccount = existingUser,
                Classrooms = classroomViewModels,
                AllClassrooms = allClassroomViewModels
            };

            return View(userClassroomsViewModel);
        }

        public async Task<IActionResult> ClassroomFlux(int id)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            string userId = user?.Id ?? HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User ID cannot be null or empty.");
            }

            var userAccount = await _userService.GetUserByIdAsync(userId);
            if (userAccount == null)
            {
                return NotFound("User not found.");
            }

            var classroom = await _classroomStudentsService.GetClassroomByIdAsync(id);
            if (classroom == null)
            {
                return NotFound("Classroom not found.");
            }

            var assignments = await _assignmentService.GetAssignmentsByClassroomIdAsync(id);

            var assignmentChats = new List<AssignmentChat>();
            foreach (var assignment in assignments)
            {
                var chats = await _assignmentService.GetAssignmentChatsByAssignmentIdAsync(assignment.id);
                assignmentChats.AddRange(chats);
            }

            var documents = new List<Document>();
            foreach (var assignment in assignments)
            {
                var assignmentDocuments = await _documentService.GetDocumentsByAssignmentIdAsync(assignment.id);
                documents.AddRange(assignmentDocuments);
            }

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

        public async Task<IActionResult> UserProfile()
        {
            ViewData["Title"] = "UserProfile";
            var user = await _userManager.GetUserAsync(HttpContext.User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return View(user);
        }

        public async Task<IActionResult> Notifications()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var notifications = (await _notificationService.GetNotificationsByUserIdAsync(user.Id)).ToList();

            var viewModel = new UserNotificationsViewModel
            {
                UserAccount = user,
                Notifications = notifications
            };

            return View(viewModel);
        }
    }
}