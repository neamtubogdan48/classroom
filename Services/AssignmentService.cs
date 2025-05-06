using mvc.IRepository;
using mvc.Models;

namespace mvc.Services
{
    public class AssignmentService : IAssignmentService
    {
        private readonly IAssignmentRepository _assignmentRepository;

        public AssignmentService(IAssignmentRepository assignmentRepository)
        {
            _assignmentRepository = assignmentRepository;
        }

        public async Task<IEnumerable<Assignment>> GetAllAssignmentsAsync()
        {
            return await _assignmentRepository.GetAllAssignmentsAsync();
        }

        public async Task<Assignment?> GetAssignmentByIdAsync(int id)
        {
            return await _assignmentRepository.GetAssignmentByIdAsync(id);
        }

        public async Task AddAssignmentAsync(Assignment assignment)
        {
            await _assignmentRepository.AddAssignmentAsync(assignment);
        }

        public async Task UpdateAssignmentAsync(Assignment assignment)
        {
            await _assignmentRepository.UpdateAssignmentAsync(assignment);
        }

        public async Task DeleteAssignmentAsync(int id)
        {
            await _assignmentRepository.DeleteAssignmentAsync(id);
        }
    }
}
