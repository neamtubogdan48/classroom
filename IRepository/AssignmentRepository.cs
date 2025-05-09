using Microsoft.EntityFrameworkCore;
using mvc.Data;
using mvc.Models;

namespace mvc.IRepository
{
    public class AssignmentRepository : IAssignmentRepository
    {
        private readonly AppDbContext _context;

        public AssignmentRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Assignment>> GetAllAssignmentsAsync()
        {
            return await _context.Assignment.ToListAsync();
        }

        public async Task<Assignment?> GetAssignmentByIdAsync(int id)
        {
            return await _context.Assignment.AsNoTracking().FirstOrDefaultAsync(d => d.id == id);
        }

        public async Task AddAssignmentAsync(Assignment assignment)
        {
            _context.Assignment.Add(assignment);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAssignmentAsync(Assignment assignment)
        {
            _context.Assignment.Update(assignment);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAssignmentAsync(int id)
        {
            var assignment = await _context.Assignment.FindAsync(id);
            if (assignment != null)
            {
                _context.Assignment.Remove(assignment);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Assignment>> GetAssignmentsByClassroomIdAsync(int classroomId)
        {
            return await _context.Assignment
                .Where(a => a.classroomId == classroomId)
                .ToListAsync();
        }

        public async Task<IEnumerable<AssignmentChat>> GetAssignmentChatsByAssignmentIdAsync(int assignmentId)
        {
            return await _context.AssignmentChat
                .Where(ac => ac.assignmentId == assignmentId)
                .ToListAsync();
        }

    }
}
