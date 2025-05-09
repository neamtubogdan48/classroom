using Microsoft.EntityFrameworkCore;
using mvc.Data;
using mvc.Models;

namespace mvc.IRepository
{
    public class DocumentRepository : IDocumentRepository
    {
        private readonly AppDbContext _context;

        public DocumentRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Document>> GetAllDocumentsAsync()
        {
            return await _context.Document.ToListAsync();
        }

        public async Task<Document?> GetDocumentByIdAsync(int id)
        {
            return await _context.Document.AsNoTracking().FirstOrDefaultAsync(d => d.id == id);
        }

        public async Task AddDocumentAsync(Document document)
        {
            _context.Document.Add(document);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateDocumentAsync(Document document)
        {
            _context.Document.Update(document);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteDocumentAsync(int id)
        {
            var document = await _context.Document.FindAsync(id);
            if (document != null)
            {
                _context.Document.Remove(document);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Document>> GetDocumentsByAssignmentIdAsync(int assignmentId)
        {
            return await _context.Document
                .AsNoTracking() // Ensures no tracking for better performance
                .Where(doc => doc.assignmentId == assignmentId)
                .ToListAsync();
        }

    }
}
