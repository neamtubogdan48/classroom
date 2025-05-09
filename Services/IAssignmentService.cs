using Microsoft.DotNet.Scaffolding.Shared.CodeModifier.CodeChange;
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
        Task<IEnumerable<Assignment>> GetAssignmentsByClassroomIdAsync(int classroomId);
        Task<IEnumerable<AssignmentChat>> GetAssignmentChatsByAssignmentIdAsync(int assignmentId);
    }
}
