using Waffy.Models;
using Waffy.Models.ViewModels;

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
        Task<IEnumerable<string>> GetAllImagesAsync(int reportId);

        Task<IEnumerable<LostPetReport>> GetActivePagedAsync(int page, int pageSize);
        Task<int> GetActiveTotalCountAsync();

        Task CloseReportAsync(int reportId);

        Task<IEnumerable<LostPetAdminDto>> GetAllAsync();
        Task<int> GetUserPostCountAsync(int userId);
        Task ExpireOldReportsAsync();
        Task<IEnumerable<LostPetReport>> GetExpiringSoonAsync();
        Task MarkExpiryNotifiedAsync(int reportId);
        Task UpdatePaymentStatusAsync(int reportId, bool isPaid, int paymentId);
        Task UpdateAmountPaidAsync(int reportId, decimal amount);
        Task<int> GetFreePostsCountAsync(int userId);
        Task<int> GetFreeNoRewardPostsCountAsync(int userId);
        Task RenewReportAsync(int reportId, DateTime newExpiryDate);
        Task<bool> CanRenewAsync(int reportId);
        Task<bool> HasRenewedExpiredPostAsync(int userId);
        Task MarkRewardReservedAsync(int reportId);
        Task UpdateRewardStatusAsync(int reportId, string status);
    }
}
