using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using mvc.Models;
using mvc.Services;
using mvc.ViewModels;

namespace mvc.Controllers
{
    public class DocumentController : BaseController
    {
        private readonly IDocumentService _documentService;
        private readonly IClassroomStudentsService _classroomStudentsService;
        private readonly IUserService _userService;
        private readonly IAssignmentService _assignmentService;

        public DocumentController(UserManager<UserAccount> userManager, IDocumentService documentService, IClassroomStudentsService classroomStudentsService, IUserService userService, IAssignmentService assignmentService) : base(userManager)
        {
            _documentService = documentService;
            _classroomStudentsService = classroomStudentsService;
            _userService = userService;
            _assignmentService = assignmentService;
        }

        // GET: Document
        public async Task<IActionResult> Index()
        {
            var documents = await _documentService.GetAllDocumentsAsync();
            return View(documents);
        }

        // GET: Document/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var document = await _documentService.GetDocumentByIdAsync(id);
            if (document == null)
            {
                return NotFound();
            }
            return View(document);
        }

        // GET: Document/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Document/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("grade,assignmentId,userId")] Document document, IFormFile studentDocFile)
        {
            if (studentDocFile != null && studentDocFile.Length > 0)
            {
                // Validate file size (max 100MB)
                const long maxFileSize = 100 * 1024 * 1024; // 100MB in bytes
                if (studentDocFile.Length > maxFileSize)
                {
                    ModelState.AddModelError("studentDoc", "The file size cannot exceed 100MB.");
                }

                // Validate file type
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".pdf", ".docx", ".zip", ".rar" };
                var fileExtension = Path.GetExtension(studentDocFile.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("studentDoc", "Only photos, PDFs, DOCX, and archive files are allowed.");
                }

                if (ModelState.IsValid)
                {
                    // Define the path to save the uploaded file
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/documents");
                    Directory.CreateDirectory(uploadsFolder); // Ensure the directory exists

                    var uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Save the file to the server
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await studentDocFile.CopyToAsync(stream);
                    }

                    // Save the file path in the studentDoc property
                    document.studentDoc = $"/uploads/documents/{uniqueFileName}";
                }
            }
            else
            {
                ModelState.AddModelError("studentDoc", "Please upload a document.");
                Console.WriteLine("No file uploaded.");
            }

            if (ModelState.IsValid)
            {
                await _documentService.AddDocumentAsync(document);

                // Send notification to classroom professor
                var assignment = await _assignmentService.GetAssignmentByIdAsync(document.assignmentId);
                if (assignment != null)
                {
                    var classroom = await _classroomStudentsService.GetClassroomByIdAsync(assignment.classroomId);
                    if (classroom != null)
                    {
                        var professorId = classroom.professorId;
                        var userId = HttpContext.Session.GetString("UserId");
                        var user = await _userService.GetUserByIdAsync(userId);

                        var notificationService = HttpContext.RequestServices.GetService(typeof(INotificationService)) as INotificationService;
                        if (notificationService != null && !string.IsNullOrEmpty(professorId))
                        {
                            var notification = new Notification
                            {
                                userId = professorId,
                                name = "Document Uploaded",
                                description = $"{user?.UserName ?? "A user"} uploaded a document for assignment '{assignment.name}'.",
                                timeSent = DateTime.UtcNow.AddHours(3)
                            };
                            await notificationService.AddNotificationAsync(notification);
                        }
                    }
                }

                return RedirectToAction("ClassroomFlux", "Home", new { id = assignment.classroomId });
            }

            return View(document);
        }

        // GET: Document/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var document = await _documentService.GetDocumentByIdAsync(id);
            if (document == null)
            {
                return NotFound();
            }
            return View(document);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("id,grade,assignmentId,userId")] Document document, IFormFile studentDocFile)
        {
            if (id != document.id)
            {
                return BadRequest("Document ID mismatch.");
            }

            var existingDocument = await _documentService.GetDocumentByIdAsync(id);
            if (existingDocument == null)
            {
                return NotFound();
            }

            // Update only fields that have changed
            if (document.grade != existingDocument.grade)
                existingDocument.grade = document.grade;

            if (document.assignmentId != existingDocument.assignmentId)
                existingDocument.assignmentId = document.assignmentId;

            if (!string.IsNullOrEmpty(document.userId) && document.userId != existingDocument.userId)
                existingDocument.userId = document.userId;

            // Handle file upload
            if (studentDocFile != null && studentDocFile.Length > 0)
            {
                const long maxFileSize = 100 * 1024 * 1024; // 100MB
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".pdf", ".docx", ".zip", ".rar" };
                var fileExtension = Path.GetExtension(studentDocFile.FileName).ToLower();

                if (studentDocFile.Length > maxFileSize)
                {
                    ModelState.AddModelError("studentDoc", "The file size cannot exceed 100MB.");
                }

                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("studentDoc", "Only photos, PDFs, DOCX, and archive files are allowed.");
                }

                if (ModelState.IsValid)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/documents");
                    Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await studentDocFile.CopyToAsync(stream);
                    }

                    // Delete old file if exists
                    if (!string.IsNullOrEmpty(existingDocument.studentDoc))
                    {
                        var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingDocument.studentDoc.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    existingDocument.studentDoc = $"/uploads/documents/{uniqueFileName}";
                }
            }

            if (!ModelState.IsValid)
                return View(existingDocument);

            await _documentService.UpdateDocumentAsync(existingDocument);
            return RedirectToAction(nameof(Index));
        }





        // GET: Document/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var document = await _documentService.GetDocumentByIdAsync(id);
            if (document == null)
            {
                return NotFound();
            }
            return View(document);
        }

        // POST: Document/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Retrieve the document to access the studentDoc path
            var document = await _documentService.GetDocumentByIdAsync(id);
            if (document == null)
            {
                ModelState.AddModelError(string.Empty, "Document not found.");
                return RedirectToAction(nameof(Index));
            }

            // Delete the student document from the server if it exists
            if (!string.IsNullOrEmpty(document.studentDoc))
            {
                var docPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", document.studentDoc.TrimStart('/'));
                if (System.IO.File.Exists(docPath))
                {
                    System.IO.File.Delete(docPath);
                }
            }

            // Retrieve the assignment to access classroomId
            var assignment = await _assignmentService.GetAssignmentByIdAsync(document.assignmentId);
            if (assignment == null)
            {
                ModelState.AddModelError(string.Empty, "Assignment not found.");
                return RedirectToAction(nameof(Index));
            }

            // Proceed to delete the document
            await _documentService.DeleteDocumentAsync(id);

            return RedirectToAction("ClassroomFlux", "Home", new { id = assignment.classroomId });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReturnDocument(int id)
        {
            var document = await _documentService.GetDocumentByIdAsync(id);
            if (document == null)
            {
                ModelState.AddModelError(string.Empty, "Document not found.");
                return RedirectToAction(nameof(Index));
            }

            // Notify student that professor has returned the homework
            var assignment = await _assignmentService.GetAssignmentByIdAsync(document.assignmentId);
            var notificationService = HttpContext.RequestServices.GetService(typeof(INotificationService)) as INotificationService;
            if (notificationService != null && assignment != null && !string.IsNullOrEmpty(document.userId))
            {
                var studentNotification = new Notification
                {
                    userId = document.userId,
                    name = "Homework Returned",
                    description = $"In assignment '{assignment.name}', the professor has returned your homework.",
                    timeSent = DateTime.UtcNow.AddHours(3)
                };
                await notificationService.AddNotificationAsync(studentNotification);
            }

            return await DeleteConfirmed(id);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GradeDocument(int id, int grade)
        {
            // Retrieve the document by ID
            var document = await _documentService.GetDocumentByIdAsync(id);
            if (document == null)
            {
                return NotFound();
            }

            // Update the grade
            document.grade = grade;
            await _documentService.UpdateDocumentAsync(document);

            // Retrieve assignment for notification
            var assignment = await _assignmentService.GetAssignmentByIdAsync(document.assignmentId);

            // Send notification to the user
            var notificationService = HttpContext.RequestServices.GetService(typeof(INotificationService)) as INotificationService;
            if (notificationService != null && assignment != null && !string.IsNullOrEmpty(document.userId))
            {
                var notification = new Notification
                {
                    userId = document.userId,
                    name = "Grade Received",
                    description = $"Your grade is {document.grade} in assignment '{assignment.name}'.",
                    timeSent = DateTime.UtcNow.AddHours(3)
                };
                await notificationService.AddNotificationAsync(notification);
            }

            return RedirectToAction("UploadList", new { id = document.assignmentId });
        }

        public async Task<IActionResult> UploadList(int id)
        {
            ViewData["AssignmentId"] = id;

            // Retrieve the classroom students and documents for the given assignment
            var classroomStudents = await _classroomStudentsService.GetClassroomStudentsByAssignmentIdAsync(id);
            var documents = (await _documentService.GetDocumentsByAssignmentIdAsync(id)).ToList();
            var users = await _userService.GetAllUsersAsync();

            var ClassroomFluxViewModel = new ClassroomFluxViewModel
            {
                ClassroomStudents = classroomStudents,
                Documents = documents,
                Users = users
            };

            return View(ClassroomFluxViewModel);
        }
    }
}
