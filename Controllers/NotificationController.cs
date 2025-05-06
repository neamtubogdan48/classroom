using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using mvc.Models;
using mvc.Services;

namespace mvc.Controllers
{
    public class NotificationController : BaseController
    {
        private readonly INotificationService _notificationService;

        public NotificationController(UserManager<UserAccount> userManager, INotificationService notificationService) : base(userManager)
        {
                _notificationService = notificationService;
        }

        // GET: Notification
        public async Task<IActionResult> Index()
        {
            var notifications = await _notificationService.GetAllNotificationsAsync();
            return View(notifications);
        }

        // GET: Notification/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var notification = await _notificationService.GetNotificationByIdAsync(id);
            if (notification == null)
            {
                return NotFound();
            }
            return View(notification);
        }

        // GET: Notification/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Notification/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("name,description,timeSent,userId")] Notification notification)
        {
            if (ModelState.IsValid)
            {
                // Convert timeSent to UTC
                notification.timeSent = DateTime.SpecifyKind(notification.timeSent, DateTimeKind.Utc);

                await _notificationService.AddNotificationAsync(notification);
                return RedirectToAction(nameof(Index));
            }
            return View(notification);
        }


        // GET: Notification/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var notification = await _notificationService.GetNotificationByIdAsync(id);
            if (notification == null)
            {
                return NotFound();
            }
            return View(notification);
        }

        // POST: Notification/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("id,name,description,timeSent,userId")] Notification notification)
        {
            if (id != notification.id)
            {
                return BadRequest("Notification ID mismatch.");
            }

            if (ModelState.IsValid)
            {
                // Convert timeSent to UTC
                notification.timeSent = DateTime.SpecifyKind(notification.timeSent, DateTimeKind.Utc);
                await _notificationService.UpdateNotificationAsync(notification);
                return RedirectToAction(nameof(Index));
            }
            return View(notification);
        }

        // GET: Notification/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var notification = await _notificationService.GetNotificationByIdAsync(id);
            if (notification == null)
            {
                return NotFound();
            }
            return View(notification);
        }

        // POST: Notification/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _notificationService.DeleteNotificationAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
