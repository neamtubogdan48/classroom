using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using mvc.Models;
using mvc.Services;

namespace mvc.Controllers
{
    public class AssignmentController : BaseController
    {
        private readonly IAssignmentService _assignmentService;

        public AssignmentController(UserManager<UserAccount> userManager, IAssignmentService assignmentService) : base(userManager)
        {
            _assignmentService = assignmentService;
        }

        // GET: Assignment
        public async Task<IActionResult> Index()
        {
            var assignments = await _assignmentService.GetAllAssignmentsAsync();
            return View(assignments);
        }

        // GET: Assignment/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var assignment = await _assignmentService.GetAssignmentByIdAsync(id);
            if (assignment == null)
            {
                return NotFound();
            }
            return View(assignment);
        }

        // GET: Assignment/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Assignment/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("name,description,deadline,lateTurnInOption,noDeadlineOption,classroomId")] Assignment assignment, IFormFile requirementsDocFile)
        {
            if (requirementsDocFile != null && requirementsDocFile.Length > 0)
            {
                // Validate file size (max 100MB)
                const long maxFileSize = 100 * 1024 * 1024; // 100MB in bytes
                if (requirementsDocFile.Length > maxFileSize)
                {
                    ModelState.AddModelError("requirementsDoc", "The file size cannot exceed 100MB.");
                }

                // Validate file type
                var allowedExtensions = new[] { ".pdf", ".docx", ".txt", ".zip", ".rar" };
                var fileExtension = Path.GetExtension(requirementsDocFile.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("requirementsDoc", "Only PDF, DOCX, TXT, and archive files are allowed.");
                }

                if (ModelState.IsValid)
                {
                    // Define the path to save the uploaded file
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/assignments");
                    Directory.CreateDirectory(uploadsFolder); // Ensure the directory exists

                    var uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Save the file to the server
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await requirementsDocFile.CopyToAsync(stream);
                    }

                    // Save the file path in the requirementsDoc property
                    assignment.requirementsDoc = $"/uploads/assignments/{uniqueFileName}";
                }
            }
            else
            {
                ModelState.AddModelError("requirementsDoc", "Please upload a requirements document.");
            }

            if (ModelState.IsValid)
            {
                // Convert deadline to UTC
                assignment.deadline = DateTime.SpecifyKind(assignment.deadline, DateTimeKind.Utc);

                await _assignmentService.AddAssignmentAsync(assignment);
                return RedirectToAction(nameof(Index));
            }

            return View(assignment);
        }

        // GET: Assignment/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var assignment = await _assignmentService.GetAssignmentByIdAsync(id);
            if (assignment == null)
            {
                return NotFound();
            }
            return View(assignment);
        }

        // POST: Assignment/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("id,name,description,deadline,lateTurnInOption,noDeadlineOption,classroomId")] Assignment assignment, IFormFile requirementsDocFile)
        {
            if (id != assignment.id)
            {
                return BadRequest("Assignment ID mismatch.");
            }

            // Retrieve the existing assignment without tracking
            var existingAssignment = await _assignmentService.GetAssignmentByIdAsync(id);
            if (existingAssignment == null)
            {
                return NotFound();
            }

            if (requirementsDocFile != null && requirementsDocFile.Length > 0)
            {
                // Validate file size (max 100MB)
                const long maxFileSize = 100 * 1024 * 1024; // 100MB in bytes
                if (requirementsDocFile.Length > maxFileSize)
                {
                    ModelState.AddModelError("requirementsDoc", "The file size cannot exceed 100MB.");
                }

                // Validate file type
                var allowedExtensions = new[] { ".pdf", ".docx", ".txt", ".zip", ".rar" };
                var fileExtension = Path.GetExtension(requirementsDocFile.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("requirementsDoc", "Only PDF, DOCX, TXT, and archive files are allowed.");
                }

                if (ModelState.IsValid)
                {
                    // Define the path to save the uploaded file
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/assignments");
                    Directory.CreateDirectory(uploadsFolder); // Ensure the directory exists

                    var uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Save the new file to the server
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await requirementsDocFile.CopyToAsync(stream);
                    }

                    // Delete the old file if it exists
                    if (!string.IsNullOrEmpty(existingAssignment.requirementsDoc))
                    {
                        var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingAssignment.requirementsDoc.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    // Update the file path in the requirementsDoc property
                    assignment.requirementsDoc = $"/uploads/assignments/{uniqueFileName}";
                }
            }
            else
            {
                // Retain the existing file path if no new file is uploaded
                assignment.requirementsDoc = existingAssignment.requirementsDoc;
            }

            if (ModelState.IsValid)
            {
                // Convert deadline to UTC
                assignment.deadline = DateTime.SpecifyKind(assignment.deadline, DateTimeKind.Utc);

                // Update the assignment in the database
                await _assignmentService.UpdateAssignmentAsync(assignment);
                return RedirectToAction(nameof(Index));
            }

            return View(assignment);
        }

        // GET: Assignment/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var assignment = await _assignmentService.GetAssignmentByIdAsync(id);
            if (assignment == null)
            {
                return NotFound();
            }
            return View(assignment);
        }

        // POST: Assignment/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Retrieve the assignment to access the requirementsDoc path
            var assignment = await _assignmentService.GetAssignmentByIdAsync(id);
            if (assignment == null)
            {
                ModelState.AddModelError(string.Empty, "Assignment not found.");
                return RedirectToAction(nameof(Index));
            }

            // Delete the requirements document from the server if it exists
            if (!string.IsNullOrEmpty(assignment.requirementsDoc))
            {
                var docPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", assignment.requirementsDoc.TrimStart('/'));
                if (System.IO.File.Exists(docPath))
                {
                    System.IO.File.Delete(docPath);
                }
            }

            // Proceed to delete the assignment
            await _assignmentService.DeleteAssignmentAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
