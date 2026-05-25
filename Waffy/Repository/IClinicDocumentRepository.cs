using Waffy.Models;

namespace Waffy.Interfaces
{
    public interface IClinicDocumentRepository
    {
        Task AddAsync(ClinicDocument document);
        Task<IEnumerable<ClinicDocument>> GetByClinicIdAsync(int idClinic);
        Task DeleteAsync(int id);
        Task UpdateStatusAsync(int docId, string status, string? adminNote, int reviewedByAdminId);
    }
}