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

        // GET: Assignment/Create/{classroomId}
        public IActionResult Create(int id) // 'id' here represents the classroomId
        {
            var model = new Assignment
            {
                classroomId = id // Assign the classroomId from the URL
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("name,description,deadline,lateTurnInOption,noDeadlineOption,classroomId")] Assignment assignment, IFormFile? requirementsDocFile)
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
                    try
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
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("requirementsDoc", "An error occurred while saving the file.");
                    }
                }
            }
            else
            {
                // If no file is uploaded, set the requirementsDoc property to null or an empty string
                assignment.requirementsDoc = null;
            }

            if (ModelState.IsValid)
            {
                // Convert deadline to UTC
                assignment.deadline = DateTime.SpecifyKind(assignment.deadline, DateTimeKind.Utc);
                if (assignment.noDeadlineOption)
                {
                    assignment.deadline = DateTime.MaxValue; // Set to a far future date if no deadline option is selected
                }

                await _assignmentService.AddAssignmentAsync(assignment);

                // Notify all students in the classroom
                var classroomStudentsService = HttpContext.RequestServices.GetService(typeof(IClassroomStudentsService)) as IClassroomStudentsService;
                var notificationService = HttpContext.RequestServices.GetService(typeof(INotificationService)) as INotificationService;
                if (classroomStudentsService != null && notificationService != null)
                {
                    var students = await classroomStudentsService.GetClassroomStudentsByClassroomIdAsync(assignment.classroomId);
                    foreach (var student in students)
                    {
                        var notification = new Notification
                        {
                            userId = student.userId,
                            name = "New Assignment",
                            description = $"A new assignment '{assignment.name}' has been created in your classroom.",
                            timeSent = DateTime.UtcNow.AddHours(3)
                        };
                        await notificationService.AddNotificationAsync(notification);
                    }
                }

                return RedirectToAction("ClassroomFlux", "Home", new { id = assignment.classroomId });

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
        public async Task<IActionResult> Edit(int id, [Bind("id,name,description,deadline,lateTurnInOption,noDeadlineOption,classroomId")] Assignment assignment, IFormFile? requirementsDocFile)
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

            // Update only the fields that are provided or changed
            if (!string.IsNullOrEmpty(assignment.name) && assignment.name != existingAssignment.name)
            {
                existingAssignment.name = assignment.name;
            }

            if (!string.IsNullOrEmpty(assignment.description) && assignment.description != existingAssignment.description)
            {
                existingAssignment.description = assignment.description;
            }

            if (assignment.deadline != default && assignment.deadline != existingAssignment.deadline)
            {
                existingAssignment.deadline = DateTime.SpecifyKind(assignment.deadline, DateTimeKind.Utc);
            }

            existingAssignment.lateTurnInOption = assignment.lateTurnInOption;
            existingAssignment.noDeadlineOption = assignment.noDeadlineOption;

            if (assignment.classroomId != existingAssignment.classroomId)
            {
                existingAssignment.classroomId = assignment.classroomId;
            }

            // Handle file upload only if a new file is provided
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
                    existingAssignment.requirementsDoc = $"/uploads/assignments/{uniqueFileName}";
                }
            }

            // If no new file is uploaded, retain the existing file path
            if (requirementsDocFile == null)
            {
                assignment.requirementsDoc = existingAssignment.requirementsDoc;
            }

            if (ModelState.IsValid)
            {
                // Update the assignment in the database
                await _assignmentService.UpdateAssignmentAsync(existingAssignment);
                return RedirectToAction("ClassroomFlux", "Home", new { id = assignment.classroomId });
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
            return RedirectToAction("ClassroomFlux", "Home", new { id = assignment.classroomId });
        }
    }
    }

