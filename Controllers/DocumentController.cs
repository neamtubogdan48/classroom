using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using mvc.Models;
using mvc.Services;

namespace mvc.Controllers
{
    public class DocumentController : BaseController
    {
        private readonly IDocumentService _documentService;

        public DocumentController(UserManager<UserAccount> userManager, IDocumentService documentService) : base(userManager)
        {
            _documentService = documentService;
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
            }

            if (ModelState.IsValid)
            {
                await _documentService.AddDocumentAsync(document);
                return RedirectToAction(nameof(Index));
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

        // POST: Document/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("id,grade,assignmentId,userId")] Document document, IFormFile studentDocFile)
        {
            if (id != document.id)
            {
                return BadRequest("Document ID mismatch.");
            }

            // Retrieve the existing document without tracking
            var existingDocument = await _documentService.GetDocumentByIdAsync(id);
            if (existingDocument == null)
            {
                return NotFound();
            }

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

                    // Save the new file to the server
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await studentDocFile.CopyToAsync(stream);
                    }

                    // Delete the old file if it exists
                    if (!string.IsNullOrEmpty(existingDocument.studentDoc))
                    {
                        var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingDocument.studentDoc.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    // Update the file path in the studentDoc property
                    document.studentDoc = $"/uploads/documents/{uniqueFileName}";
                }
            }
            else
            {
                // Retain the existing file path if no new file is uploaded
                document.studentDoc = existingDocument.studentDoc;
            }

            if (ModelState.IsValid)
            {
                // Update the document in the database
                await _documentService.UpdateDocumentAsync(document);
                return RedirectToAction(nameof(Index));
            }

            return View(document);
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

            // Proceed to delete the document
            await _documentService.DeleteDocumentAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
