using mvc.Models;

namespace mvc.Services
{
    public interface IAssignmentService
    {
        Task<IEnumerable<Assignment>> GetAllAssignmentsAsync();
        Task<Assignment?> GetAssignmentByIdAsync(int id);
        Task AddAssignmentAsync(Assignment assignment);
        Task UpdateAssignmentAsync(Assignment assignment);
        Task DeleteAssignmentAsync(int id);
    }
}
