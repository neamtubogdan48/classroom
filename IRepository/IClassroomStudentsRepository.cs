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
    }
}
