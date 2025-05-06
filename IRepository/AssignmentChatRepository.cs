using Microsoft.EntityFrameworkCore;
using mvc.Data;
using mvc.Models;

namespace mvc.IRepository
{
    public class AssignmentChatRepository : IAssignmentChatRepository
    {
        private readonly AppDbContext _context;

        public AssignmentChatRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AssignmentChat>> GetAllAssignmentChatsAsync()
        {
            return await _context.AssignmentChat.ToListAsync();
        }

        public async Task<AssignmentChat?> GetAssignmentChatByIdAsync(int id)
        {
            return await _context.AssignmentChat.FindAsync(id);
        }

        public async Task AddAssignmentChatAsync(AssignmentChat assignmentChat)
        {
            _context.AssignmentChat.Add(assignmentChat);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAssignmentChatAsync(AssignmentChat assignmentChat)
        {
            _context.AssignmentChat.Update(assignmentChat);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAssignmentChatAsync(int id)
        {
            var assignmentChat = await _context.AssignmentChat.FindAsync(id);
            if (assignmentChat != null)
            {
                _context.AssignmentChat.Remove(assignmentChat);
                await _context.SaveChangesAsync();
            }
        }
    }
}
