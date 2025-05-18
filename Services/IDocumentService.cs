using mvc.Models;

namespace mvc.Services
{
    public interface IDocumentService
    {
        Task<IEnumerable<Document>> GetAllDocumentsAsync();
        Task<Document?> GetDocumentByIdAsync(int id);
        Task AddDocumentAsync(Document document);
        Task UpdateDocumentAsync(Document document);
        Task DeleteDocumentAsync(int id);
        Task<IEnumerable<Document>> GetDocumentsByAssignmentIdAsync(int assignmentId);
        Task<bool> HasNotificationWithDescriptionAsync(string userId, string description);
    }
}
