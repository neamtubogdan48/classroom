using Microsoft.EntityFrameworkCore;
using mvc.Data;
using mvc.Models;

namespace mvc.IRepository
{
    public class ClassroomStudentsRepository : IClassroomStudentsRepository
    {
        private readonly AppDbContext _context;

        public ClassroomStudentsRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ClassroomStudents>> GetAllClassroomStudentsAsync()
        {
            return await _context.ClassroomStudents.ToListAsync();
        }

        public async Task<ClassroomStudents?> GetClassroomStudentByIdAsync(int id)
        {
            return await _context.ClassroomStudents.FindAsync(id);
        }

        public async Task AddClassroomStudentAsync(ClassroomStudents classroomStudent)
        {
            _context.ClassroomStudents.Add(classroomStudent);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateClassroomStudentAsync(ClassroomStudents classroomStudent)
        {
            _context.ClassroomStudents.Update(classroomStudent);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteClassroomStudentAsync(int id)
        {
            var classroomStudent = await _context.ClassroomStudents.FindAsync(id);
            if (classroomStudent != null)
            {
                _context.ClassroomStudents.Remove(classroomStudent);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Classroom>> GetClassroomsByUserIdAsync(string userId)
        {
            return await _context.ClassroomStudents
                .Where(cs => cs.userId == userId)
                .Join(_context.Classroom,
                      cs => cs.classroomId,
                      c => c.id,
                      (cs, c) => c)
                .ToListAsync();
        }

        public async Task<Classroom?> GetClassroomByIdAsync(int id)
        {
            return await _context.Classroom.FirstOrDefaultAsync(c => c.id == id);
        }

        public async Task RemoveClassroomStudentAsync(int id)
        {
            var classroomStudent = await _context.ClassroomStudents.FindAsync(id);
            if (classroomStudent != null)
            {
                _context.ClassroomStudents.Remove(classroomStudent);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<ClassroomStudents?> GetClassroomStudentByIdAsync(string userId, int classroomId)
        {
            return await _context.ClassroomStudents
                .FirstOrDefaultAsync(cs => cs.userId == userId && cs.classroomId == classroomId);
        }

        public async Task<List<ClassroomStudents>> GetClassroomStudentsByAssignmentIdAsync(int assignmentId)
        {
            // Retrieve the classroomId associated with the given assignmentId
            var classroomId = await _context.Assignment
                .Where(a => a.id == assignmentId)
                .Select(a => a.classroomId)
                .FirstOrDefaultAsync();

            // If no classroomId is found, return an empty list
            if (classroomId == 0)
            {
                return new List<ClassroomStudents>();
            }

            // Retrieve the students in the classroom
            return await _context.ClassroomStudents
                .Where(cs => cs.classroomId == classroomId)
                .ToListAsync();
        }

        public async Task<ClassroomStudents?> GetClassroomStudentByUserAndClassroomAsync(string userId, int classroomId)
        {
            return await _context.ClassroomStudents
                .FirstOrDefaultAsync(cs => cs.userId == userId && cs.classroomId == classroomId);
        }
    }
}
