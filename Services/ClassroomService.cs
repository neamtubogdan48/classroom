using mvc.IRepository;
using mvc.Models;

namespace mvc.Services
{
    public class ClassroomService : IClassroomService
    {
        private readonly IClassroomRepository _classroomRepository;

        public ClassroomService(IClassroomRepository classroomRepository)
        {
            _classroomRepository = classroomRepository;
        }

        public async Task<IEnumerable<Classroom>> GetAllClassroomsAsync()
        {
            return await _classroomRepository.GetAllClassroomsAsync();
        }

        public async Task<Classroom?> GetClassroomByIdAsync(int id)
        {
            return await _classroomRepository.GetClassroomByIdAsync(id);
        }

        public async Task AddClassroomAsync(Classroom classroom)
        {
            await _classroomRepository.AddClassroomAsync(classroom);
        }

        public async Task UpdateClassroomAsync(Classroom classroom)
        {
            await _classroomRepository.UpdateClassroomAsync(classroom);
        }

        public async Task DeleteClassroomAsync(int id)
        {
            await _classroomRepository.DeleteClassroomAsync(id);
        }
    }
}
