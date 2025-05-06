using mvc.IRepository;
using mvc.Models;

namespace mvc.Services
{
    public class AssignmentChatService : IAssignmentChatService
    {
        private readonly IAssignmentChatRepository _assignmentChatRepository;

        public AssignmentChatService(IAssignmentChatRepository assignmentChatRepository)
        {
            _assignmentChatRepository = assignmentChatRepository;
        }

        public async Task<IEnumerable<AssignmentChat>> GetAllAssignmentChatsAsync()
        {
            return await _assignmentChatRepository.GetAllAssignmentChatsAsync();
        }

        public async Task<AssignmentChat?> GetAssignmentChatByIdAsync(int id)
        {
            return await _assignmentChatRepository.GetAssignmentChatByIdAsync(id);
        }

        public async Task AddAssignmentChatAsync(AssignmentChat assignmentChat)
        {
            await _assignmentChatRepository.AddAssignmentChatAsync(assignmentChat);
        }

        public async Task UpdateAssignmentChatAsync(AssignmentChat assignmentChat)
        {
            await _assignmentChatRepository.UpdateAssignmentChatAsync(assignmentChat);
        }

        public async Task DeleteAssignmentChatAsync(int id)
        {
            await _assignmentChatRepository.DeleteAssignmentChatAsync(id);
        }
    }
}
