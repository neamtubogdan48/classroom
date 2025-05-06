using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using mvc.Models;
using mvc.Services;

namespace mvc.Controllers
{
    public class AssignmentChatController : BaseController
    {
        private readonly IAssignmentChatService _assignmentChatService;

        public AssignmentChatController(UserManager<UserAccount> userManager, IAssignmentChatService assignmentChatService) : base(userManager)
        {
            _assignmentChatService = assignmentChatService;
        }

        // GET: AssignmentChat
        public async Task<IActionResult> Index()
        {
            var assignmentChats = await _assignmentChatService.GetAllAssignmentChatsAsync();
            return View(assignmentChats);
        }

        // GET: AssignmentChat/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var assignmentChat = await _assignmentChatService.GetAssignmentChatByIdAsync(id);
            if (assignmentChat == null)
            {
                return NotFound();
            }
            return View(assignmentChat);
        }

        // GET: AssignmentChat/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: AssignmentChat/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("message,userId,studentId,assignmentId")] AssignmentChat assignmentChat)
        {
            if (ModelState.IsValid)
            {
                await _assignmentChatService.AddAssignmentChatAsync(assignmentChat);
                return RedirectToAction(nameof(Index));
            }
            return View(assignmentChat);
        }

        // GET: AssignmentChat/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var assignmentChat = await _assignmentChatService.GetAssignmentChatByIdAsync(id);
            if (assignmentChat == null)
            {
                return NotFound();
            }
            return View(assignmentChat);
        }

        // POST: AssignmentChat/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("id,message,userId,studentId,assignmentId")] AssignmentChat assignmentChat)
        {
            if (id != assignmentChat.id)
            {
                return BadRequest("AssignmentChat ID mismatch.");
            }

            if (ModelState.IsValid)
            {
                await _assignmentChatService.UpdateAssignmentChatAsync(assignmentChat);
                return RedirectToAction(nameof(Index));
            }
            return View(assignmentChat);
        }

        // GET: AssignmentChat/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var assignmentChat = await _assignmentChatService.GetAssignmentChatByIdAsync(id);
            if (assignmentChat == null)
            {
                return NotFound();
            }
            return View(assignmentChat);
        }

        // POST: AssignmentChat/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _assignmentChatService.DeleteAssignmentChatAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
