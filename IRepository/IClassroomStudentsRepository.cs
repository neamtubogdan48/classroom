using mvc.Models;

namespace mvc.IRepository
{
    public interface IClassroomStudentsRepository
    {
        Task<IEnumerable<ClassroomStudents>> GetAllClassroomStudentsAsync();
        Task<ClassroomStudents?> GetClassroomStudentByIdAsync(int id);
        Task AddClassroomStudentAsync(ClassroomStudents classroomStudent);
        Task UpdateClassroomStudentAsync(ClassroomStudents classroomStudent);
        Task DeleteClassroomStudentAsync(int id);
        Task<IEnumerable<Classroom>> GetClassroomsByUserIdAsync(string userId);
        Task<Classroom?> GetClassroomByIdAsync(int id);
        Task<ClassroomStudents?> GetClassroomStudentByIdAsync(int classroomId, string userId);
    }
}
