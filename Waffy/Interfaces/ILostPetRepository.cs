using Waffy.Models;

namespace Waffy.Interfaces
{
    public interface ILostPetRepository
    {
        // Public listings (Active reports)
        Task<IEnumerable<LostPetReport>> GetActiveAsync();

        // User's own reports
        Task<IEnumerable<LostPetReport>> GetByUserIdAsync(int userId);

        // Single report
        Task<LostPetReport?> GetByIdAsync(int idLostReport);
        Task<LostPetReport?> GetByCodeAsync(string reportCode);

        // CRUD
        Task<int> AddAsync(LostPetReport report);
        Task UpdateAsync(LostPetReport report);
        Task UpdateStatusAsync(int idLostReport, string status);
        Task DeleteAsync(int idLostReport);

        // Images
        Task AddImageAsync(int idLostReport, string imageUrl);
        Task<IEnumerable<LostPetImage>> GetImagesByReportIdAsync(int idLostReport);
        Task<string?> GetFirstImageAsync(int idLostReport);

        Task<IEnumerable<LostPetReport>> GetActivePagedAsync(int page, int pageSize);
        Task<int> GetActiveTotalCountAsync();

        Task CloseReportAsync(int reportId);
    }
}
