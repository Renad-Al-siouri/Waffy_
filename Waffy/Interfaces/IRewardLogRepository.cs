using Waffy.Models;
using Waffy.Models.ViewModels;

namespace Waffy.Interfaces
{
    public interface IRewardLogRepository
    {
        Task<int> AddAsync(RewardLog log);
        Task<IEnumerable<RewardLogDto>> GetAllAsync();
        Task<IEnumerable<RewardLogDto>> GetByUserIdAsync(int userId);
        Task<bool> HasRecentRewardAsync(int userId, int days = 30);
        Task<HashSet<int>> GetRewardedClaimIdsAsync();

    }
}