using mvc.Models;

namespace mvc.IRepository
{
    public interface IAssignmentRepository
    {
        Task<IEnumerable<Assignment>> GetAllAssignmentsAsync();
        Task<Assignment?> GetAssignmentByIdAsync(int id);
        Task AddAssignmentAsync(Assignment assignment);
        Task UpdateAssignmentAsync(Assignment assignment);
        Task DeleteAssignmentAsync(int id);
        Task<IEnumerable<Assignment>> GetAssignmentsByClassroomIdAsync(int classroomId);
        Task<IEnumerable<AssignmentChat>> GetAssignmentChatsByAssignmentIdAsync(int assignmentId);
    }
}
