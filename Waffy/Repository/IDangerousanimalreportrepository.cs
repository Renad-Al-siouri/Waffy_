using Waffy.Models;

namespace Waffy.Interfaces
{
    public interface IDangerousAnimalReportRepository
    {
        // الموجودة أصلاً — لا تتغير
        Task<int> CreateAsync(DangerousAnimalReport report);
        Task<IEnumerable<DangerousAnimalReport>> GetAllAsync();
        Task<IEnumerable<DangerousAnimalReport>> GetByUserIdAsync(int userId);
        Task UpdateStatusAsync(int id, string status);

        // جديدة للخريطة
        Task<IEnumerable<DangerousAnimalReport>> GetActiveForMapAsync();
        Task<DangerousAnimalReport?> GetByIdAsync(int idDangReport);
        Task DeleteAsync(int idDangReport);

        Task AddImageAsync(DangerReportImage image);
        Task<IEnumerable<DangerReportImage>> GetImagesByReportIdAsync(int idDangReport);
    }
}