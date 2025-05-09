using mvc.Models;

namespace mvc.IRepository
{
    public interface IDocumentRepository
    {
        Task<IEnumerable<Document>> GetAllDocumentsAsync();
        Task<Document?> GetDocumentByIdAsync(int id);
        Task AddDocumentAsync(Document document);
        Task UpdateDocumentAsync(Document document);
        Task DeleteDocumentAsync(int id);
        Task<IEnumerable<Document>> GetDocumentsByAssignmentIdAsync(int assignmentId);
    }
}
