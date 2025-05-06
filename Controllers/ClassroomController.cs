using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using mvc.Models;
using mvc.Services;

namespace mvc.Controllers
{
    public class ClassroomController : BaseController
    {
        private readonly IClassroomService _classroomService;

        public ClassroomController(UserManager<UserAccount> userManager, IClassroomService classroomService) : base(userManager)
        {
            _classroomService = classroomService;
        }

        // GET: Classroom
        public async Task<IActionResult> Index()
        {
            var classrooms = await _classroomService.GetAllClassroomsAsync();
            return View(classrooms);
        }

        // GET: Classroom/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var classroom = await _classroomService.GetClassroomByIdAsync(id);
            if (classroom == null)
            {
                return NotFound();
            }
            return View(classroom);
        }

        // GET: Classroom/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Classroom/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("name,code,professorId")] Classroom classroom, IFormFile photoFile)
        {
            if (photoFile != null && photoFile.Length > 0)
            {
                // Validate file size (max 5MB)
                const long maxFileSize = 5 * 1024 * 1024; // 5MB in bytes
                if (photoFile.Length > maxFileSize)
                {
                    ModelState.AddModelError("photo", "The file size cannot exceed 5MB.");
                }

                // Validate file type
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(photoFile.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("photo", "Only JPG, JPEG, PNG, and GIF files are allowed.");
                }

                if (ModelState.IsValid)
                {
                    // Define the path to save the uploaded file
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/classrooms");
                    Directory.CreateDirectory(uploadsFolder); // Ensure the directory exists

                    var uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Save the file to the server
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await photoFile.CopyToAsync(stream);
                    }

                    // Save the file path in the photo property
                    classroom.photo = $"/uploads/classrooms/{uniqueFileName}";
                }
            }
            else
            {
                ModelState.AddModelError("photo", "Please upload a photo.");
            }

            if (ModelState.IsValid)
            {
                await _classroomService.AddClassroomAsync(classroom);
                return RedirectToAction(nameof(Index));
            }

            return View(classroom);
        }


        // GET: Classroom/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var classroom = await _classroomService.GetClassroomByIdAsync(id);
            if (classroom == null)
            {
                return NotFound();
            }
            return View(classroom);
        }

        // POST: Classroom/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("id,name,code,professorId")] Classroom classroom, IFormFile photoFile)
        {
            if (id != classroom.id)
            {
                return BadRequest("Classroom ID mismatch.");
            }

            // Retrieve the existing classroom without tracking
            var existingClassroom = await _classroomService.GetClassroomByIdAsync(id);
            if (existingClassroom == null)
            {
                return NotFound();
            }

            if (photoFile != null && photoFile.Length > 0)
            {
                // Validate file size (max 5MB)
                const long maxFileSize = 5 * 1024 * 1024; // 5MB in bytes
                if (photoFile.Length > maxFileSize)
                {
                    ModelState.AddModelError("photo", "The file size cannot exceed 5MB.");
                }

                // Validate file type
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(photoFile.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("photo", "Only JPG, JPEG, PNG, and GIF files are allowed.");
                }

                if (ModelState.IsValid)
                {
                    // Define the path to save the uploaded file
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/classrooms");
                    Directory.CreateDirectory(uploadsFolder); // Ensure the directory exists

                    var uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Save the new file to the server
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await photoFile.CopyToAsync(stream);
                    }

                    // Delete the old file if it exists
                    if (!string.IsNullOrEmpty(existingClassroom.photo))
                    {
                        var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingClassroom.photo.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    // Update the file path in the photo property
                    classroom.photo = $"/uploads/classrooms/{uniqueFileName}";
                }
            }
            else
            {
                // Retain the existing file path if no new file is uploaded
                classroom.photo = existingClassroom.photo;
            }

            if (ModelState.IsValid)
            {
                await _classroomService.UpdateClassroomAsync(classroom);
                return RedirectToAction(nameof(Index));
            }

            return View(classroom);
        }

        // GET: Classroom/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var classroom = await _classroomService.GetClassroomByIdAsync(id);
            if (classroom == null)
            {
                return NotFound();
            }
            return View(classroom);
        }

        // POST: Classroom/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Retrieve the classroom to access the photo path
            var classroom = await _classroomService.GetClassroomByIdAsync(id);
            if (classroom == null)
            {
                ModelState.AddModelError(string.Empty, "Classroom not found.");
                return RedirectToAction(nameof(Index));
            }

            // Delete the photo from the server if it exists
            if (!string.IsNullOrEmpty(classroom.photo))
            {
                var photoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", classroom.photo.TrimStart('/'));
                if (System.IO.File.Exists(photoPath))
                {
                    System.IO.File.Delete(photoPath);
                }
            }

            // Proceed to delete the classroom
            await _classroomService.DeleteClassroomAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
