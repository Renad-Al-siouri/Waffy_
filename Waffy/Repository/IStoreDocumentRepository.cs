using Waffy.Models;
namespace Waffy.Interfaces
{
    public interface IStoreDocumentRepository
    {
        Task<IEnumerable<StoreDocument>> GetByStoreIdAsync(int idStore);
        Task<StoreDocument?> GetByIdAsync(int id);
        Task AddAsync(StoreDocument document);
        // ← محدّث: أضفنا adminNote و adminId
        Task UpdateStatusAsync(int id, string status, string? adminNote, int adminId);
        Task DeleteAsync(int id);
    }
}