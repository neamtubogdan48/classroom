using mvc.Models;

namespace mvc.Services
{
    public interface INotificationService
    {
        Task<IEnumerable<Notification>> GetAllNotificationsAsync();
        Task<Notification?> GetNotificationByIdAsync(int id);
        Task AddNotificationAsync(Notification notification);
        Task UpdateNotificationAsync(Notification notification);
        Task DeleteNotificationAsync(int id);
        Task<List<Notification>> GetNotificationsByUserIdAsync(string userId);
        Task<bool> ExistsAsync(string userId, string name, string description = null);
    }
}
