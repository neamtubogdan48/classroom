using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mvc.Models;
using mvc.Services;
using mvc.ViewModels;

namespace mvc.Controllers
{
    public class ClassroomStudentsController : BaseController
    {
        private readonly IClassroomStudentsService _classroomStudentsService;
        private readonly IClassroomService _classroomService;

        public ClassroomStudentsController(UserManager<UserAccount> userManager, IClassroomStudentsService classroomStudentsService, IClassroomService classroomService) : base(userManager)
        {
            _classroomStudentsService = classroomStudentsService;
            _classroomService = classroomService;
        }

        // GET: ClassroomStudents
        public async Task<IActionResult> Index()
        {
            var classroomStudents = await _classroomStudentsService.GetAllClassroomStudentsAsync();
            return View(classroomStudents);
        }

        // GET: ClassroomStudents/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var classroomStudent = await _classroomStudentsService.GetClassroomStudentByIdAsync(id);
            if (classroomStudent == null)
            {
                return NotFound();
            }
            return View(classroomStudent);
        }

        // GET: ClassroomStudents/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ClassroomStudents/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("userId,classroomId")] ClassroomStudents classroomStudent)
        {
            if (ModelState.IsValid)
            {
                await _classroomStudentsService.AddClassroomStudentAsync(classroomStudent);
                return RedirectToAction(nameof(Index));
            }
            return View(classroomStudent);
        }

        // GET: ClassroomStudents/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var classroomStudent = await _classroomStudentsService.GetClassroomStudentByIdAsync(id);
            if (classroomStudent == null)
            {
                return NotFound();
            }
            return View(classroomStudent);
        }

        // POST: ClassroomStudents/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("id,userId,classroomId")] ClassroomStudents classroomStudent)
        {
            if (id != classroomStudent.id)
            {
                return BadRequest("ClassroomStudent ID mismatch.");
            }

            if (ModelState.IsValid)
            {
                await _classroomStudentsService.UpdateClassroomStudentAsync(classroomStudent);
                return RedirectToAction(nameof(Index));
            }
            return View(classroomStudent);
        }

        // GET: ClassroomStudents/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var classroomStudent = await _classroomStudentsService.GetClassroomStudentByIdAsync(id);
            if (classroomStudent == null)
            {
                return NotFound();
            }
            return View(classroomStudent);
        }

        // POST: ClassroomStudents/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _classroomStudentsService.DeleteClassroomStudentAsync(id);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LeaveClassroom(int id)
        {
            // Retrieve the current user's ID from the session
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User ID cannot be null or empty.");
            }

            // Check if the user is part of the classroom
            var classroomStudent = await _classroomStudentsService.GetClassroomStudentByIdAsync(id, userId);
            if (classroomStudent == null)
            {
                return NotFound("User is not part of this classroom.");
            }

            // Remove the user from the classroom
            await _classroomStudentsService.DeleteClassroomStudentAsync(classroomStudent.id);

            // Redirect back to the home page or classroom list
            return RedirectToAction("Home", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> JoinClassroom(int classroomCode)
        {
            // Logic to find the classroom by code and add the user to it
            var classroom = await _classroomService.GetClassroomByCodeAsync(classroomCode);
            if (classroom == null)
            {
                return NotFound("Classroom not found.");
            }

            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not logged in.");
            }

            // Check if the user is already part of the classroom
            var existingClassroomStudent = await _classroomStudentsService.GetClassroomStudentByUserAndClassroomAsync(userId, classroom.id);
            if (existingClassroomStudent != null)
            {
                return BadRequest("You are already part of this classroom.");
            }

            await _classroomStudentsService.AddClassroomStudentAsync(new ClassroomStudents
            {
                classroomId = classroom.id,
                userId = userId
            });

            return RedirectToAction("Home", "Home");
        }

        // GET: ClassroomStudents
        public async Task<IActionResult> ClassroomList(int id)
        {
            var classroom = await _classroomService.GetClassroomByIdAsync(id);
            if (classroom == null)
            {
                return NotFound("Classroom not found.");
            }

            var classroomStudents = await _classroomStudentsService.GetAllClassroomStudentsAsync();
            var users = await _userManager.Users.ToListAsync();

            var model = new ClassroomFluxViewModel
            {
                Classroom = classroom,
                ClassroomStudents = classroomStudents.Where(cs => cs.classroomId == id).ToList(),
                Users = users
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> KickStudent(int id)
        {
            // Retrieve the student record from the database
            var classroomStudent = await _classroomStudentsService.GetClassroomStudentByIdAsync(id);
            if (classroomStudent == null)
            {
                return NotFound("Student not found in the classroom.");
            }

            // Remove the student from the classroom
            await _classroomStudentsService.RemoveClassroomStudentAsync(id);

            // Redirect back to the classroom list with the correct classroomId
            return RedirectToAction("ClassroomList", new { id = classroomStudent.classroomId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> KickProfessor(string professorId, int classroomId)
        {
            // Retrieve the professor record from ClassroomStudents
            var classroomStudent = await _classroomStudentsService.GetClassroomStudentByIdAsync(classroomId, professorId);
            if (classroomStudent == null || classroomStudent.classroomId != classroomId)
            {
                return NotFound("Professor not found in the classroom.");
            }

            // Remove the professor from ClassroomStudents
            await _classroomStudentsService.RemoveClassroomStudentAsync(classroomStudent.id);

            // Update the professorId of the classroom to a default value (e.g., null or 0)
            var classroom = await _classroomService.GetClassroomByIdAsync(classroomId);
            if (classroom != null)
            {
                classroom.professorId = "97ad4176-8ba5-4ba1-ba37-83c01ef822ab"; // Set to null or a default value
                await _classroomService.UpdateClassroomAsync(classroom);
            }

            // Redirect back to the classroom list
            return RedirectToAction("ClassroomList", new { id = classroomId });
        }
    }
}
