using mvc.Models;

namespace mvc.IRepository
{
    public interface IClassroomRepository
    {
        Task<IEnumerable<Classroom>> GetAllClassroomsAsync();
        Task<Classroom?> GetClassroomByIdAsync(int id);
        Task AddClassroomAsync(Classroom classroom);
        Task UpdateClassroomAsync(Classroom classroom);
        Task DeleteClassroomAsync(int id);
    }
}
