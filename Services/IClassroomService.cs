using mvc.Models;
using mvc.ViewModels;

namespace mvc.Services
{
    public interface IClassroomService
    {
        Task<IEnumerable<Classroom>> GetAllClassroomsAsync();
        Task<Classroom?> GetClassroomByIdAsync(int id);
        Task AddClassroomAsync(Classroom classroom);
        Task UpdateClassroomAsync(Classroom classroom);
        Task DeleteClassroomAsync(int id);
        Task<Classroom?> GetClassroomByCodeAsync(int code);
        Task<List<ClassroomViewModel>> MapClassroomsToViewModelsAsync(IEnumerable<Classroom> classrooms);
    }
}
