using mvc.Models;

namespace mvc.Services
{
    public interface IAssignmentChatService
    {
        Task<IEnumerable<AssignmentChat>> GetAllAssignmentChatsAsync();
        Task<AssignmentChat?> GetAssignmentChatByIdAsync(int id);
        Task AddAssignmentChatAsync(AssignmentChat assignmentChat);
        Task UpdateAssignmentChatAsync(AssignmentChat assignmentChat);
        Task DeleteAssignmentChatAsync(int id);
    }
}
