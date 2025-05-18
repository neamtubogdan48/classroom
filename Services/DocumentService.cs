using mvc.IRepository;
using mvc.Models;

namespace mvc.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly IDocumentRepository _documentRepository;

        public DocumentService(IDocumentRepository documentRepository)
        {
            _documentRepository = documentRepository;
        }

        public async Task<IEnumerable<Document>> GetAllDocumentsAsync()
        {
            return await _documentRepository.GetAllDocumentsAsync();
        }

        public async Task<Document?> GetDocumentByIdAsync(int id)
        {
            return await _documentRepository.GetDocumentByIdAsync(id);
        }

        public async Task AddDocumentAsync(Document document)
        {
            await _documentRepository.AddDocumentAsync(document);
        }

        public async Task UpdateDocumentAsync(Document document)
        {
            await _documentRepository.UpdateDocumentAsync(document);
        }

        public async Task DeleteDocumentAsync(int id)
        {
            await _documentRepository.DeleteDocumentAsync(id);
        }

        public async Task<IEnumerable<Document>> GetDocumentsByAssignmentIdAsync(int assignmentId)
        {
            return await _documentRepository.GetDocumentsByAssignmentIdAsync(assignmentId);
        }

        public async Task<bool> HasNotificationWithDescriptionAsync(string userId, string description)
        {
            return await _documentRepository.HasNotificationWithDescriptionAsync(userId, description);
        }
    }
}
