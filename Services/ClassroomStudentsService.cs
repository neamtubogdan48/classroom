using mvc.IRepository;
using mvc.Models;

namespace mvc.Services
{
    public class ClassroomStudentsService : IClassroomStudentsService
    {
        private readonly IClassroomStudentsRepository _classroomStudentsRepository;

        public ClassroomStudentsService(IClassroomStudentsRepository classroomStudentsRepository)
        {
            _classroomStudentsRepository = classroomStudentsRepository;
        }

        public async Task<IEnumerable<ClassroomStudents>> GetAllClassroomStudentsAsync()
        {
            return await _classroomStudentsRepository.GetAllClassroomStudentsAsync();
        }

        public async Task<ClassroomStudents?> GetClassroomStudentByIdAsync(int id)
        {
            return await _classroomStudentsRepository.GetClassroomStudentByIdAsync(id);
        }

        public async Task AddClassroomStudentAsync(ClassroomStudents classroomStudent)
        {
            await _classroomStudentsRepository.AddClassroomStudentAsync(classroomStudent);
        }

        public async Task UpdateClassroomStudentAsync(ClassroomStudents classroomStudent)
        {
            await _classroomStudentsRepository.UpdateClassroomStudentAsync(classroomStudent);
        }

        public async Task DeleteClassroomStudentAsync(int id)
        {
            await _classroomStudentsRepository.DeleteClassroomStudentAsync(id);
        }
    }
}
