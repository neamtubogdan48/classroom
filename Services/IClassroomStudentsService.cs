using mvc.Models;

namespace mvc.Services
{
    public interface IClassroomStudentsService
    {
        Task<IEnumerable<ClassroomStudents>> GetAllClassroomStudentsAsync();
        Task<ClassroomStudents?> GetClassroomStudentByIdAsync(int id);
        Task AddClassroomStudentAsync(ClassroomStudents classroomStudent);
        Task UpdateClassroomStudentAsync(ClassroomStudents classroomStudent);
        Task DeleteClassroomStudentAsync(int id);
        Task<List<Classroom>> GetClassroomsByUserIdAsync(string userId);
        Task<Classroom?> GetClassroomByIdAsync(int id);
        Task<ClassroomStudents?> GetClassroomStudentByIdAsync(int classroomId, string userId);
        Task RemoveClassroomStudentAsync(int id);
        Task<List<ClassroomStudents>> GetClassroomStudentsByAssignmentIdAsync(int assignmentId);
        Task<ClassroomStudents?> GetClassroomStudentByUserAndClassroomAsync(string userId, int classroomId);
        Task<IEnumerable<ClassroomStudents>> GetClassroomStudentsByClassroomIdAsync(int classroomId);
    }
}
