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
    }
}
