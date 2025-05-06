using mvc.Models;

namespace mvc.IRepository
{
    public interface IAssignmentChatRepository
    {
        Task<IEnumerable<AssignmentChat>> GetAllAssignmentChatsAsync();
        Task<AssignmentChat?> GetAssignmentChatByIdAsync(int id);
        Task AddAssignmentChatAsync(AssignmentChat assignmentChat);
        Task UpdateAssignmentChatAsync(AssignmentChat assignmentChat);
        Task DeleteAssignmentChatAsync(int id);
    }
}
