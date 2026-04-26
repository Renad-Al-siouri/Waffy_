using Waffy.Models;

namespace Waffy.Interfaces
{
    public interface IDangerousAnimalReportRepository
    {
        Task<int> CreateAsync(DangerousAnimalReport report);
        Task<IEnumerable<DangerousAnimalReport>> GetByUserIdAsync(int userId);
    }
}