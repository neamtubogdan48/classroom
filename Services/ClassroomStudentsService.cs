using mvc.IRepository;
using mvc.Models;
using NuGet.Protocol.Core.Types;

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

        public async Task<List<Classroom>> GetClassroomsByUserIdAsync(string userId)
        {
            return (await _classroomStudentsRepository.GetClassroomsByUserIdAsync(userId)).ToList();
        }

        public async Task<Classroom?> GetClassroomByIdAsync(int id)
        {
            return await _classroomStudentsRepository.GetClassroomByIdAsync(id);
        }

        public async Task RemoveClassroomStudentAsync(int id)
        {
            await _classroomStudentsRepository.RemoveClassroomStudentAsync(id);
        }

        public async Task<ClassroomStudents?> GetClassroomStudentByIdAsync(int classroomId, string userId)
        {
            return await _classroomStudentsRepository.GetClassroomStudentByIdAsync(userId, classroomId);
        }

        public async Task<List<ClassroomStudents>> GetClassroomStudentsByAssignmentIdAsync(int assignmentId)
        {
            return await _classroomStudentsRepository.GetClassroomStudentsByAssignmentIdAsync(assignmentId);
        }

        public async Task<ClassroomStudents?> GetClassroomStudentByUserAndClassroomAsync(string userId, int classroomId)
        {
            return await _classroomStudentsRepository.GetClassroomStudentByUserAndClassroomAsync(userId, classroomId);
        }

        public async Task<IEnumerable<ClassroomStudents>> GetClassroomStudentsByClassroomIdAsync(int classroomId)
        {
            return await _classroomStudentsRepository.GetByClassroomIdAsync(classroomId);
        }
    }
}
