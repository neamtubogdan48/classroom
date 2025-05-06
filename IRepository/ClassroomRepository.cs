using Microsoft.EntityFrameworkCore;
using mvc.Data;
using mvc.Models;

namespace mvc.IRepository
{
    public class ClassroomRepository : IClassroomRepository
    {
        private readonly AppDbContext _context;

        public ClassroomRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Classroom>> GetAllClassroomsAsync()
        {
            return await _context.Classroom.ToListAsync();
        }

        public async Task<Classroom?> GetClassroomByIdAsync(int id)
        {
            return await _context.Classroom.AsNoTracking().FirstOrDefaultAsync(d => d.id == id);
        }

        public async Task AddClassroomAsync(Classroom classroom)
        {
            _context.Classroom.Add(classroom);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateClassroomAsync(Classroom classroom)
        {
            _context.Classroom.Update(classroom);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteClassroomAsync(int id)
        {
            var classroom = await _context.Classroom.FindAsync(id);
            if (classroom != null)
            {
                _context.Classroom.Remove(classroom);
                await _context.SaveChangesAsync();
            }
        }
    }
}
