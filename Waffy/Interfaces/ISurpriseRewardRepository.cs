using Waffy.Models;

namespace Waffy.Interfaces
{
    public interface ISurpriseRewardRepository
    {
        Task<SurpriseRewardPool?> GetAvailableAsync();
        Task<SurpriseRewardPool?> GetByIdAsync(int rewardId); 
        Task MarkAsUsedAsync(int rewardId, int userId);
        Task AddAsync(SurpriseRewardPool reward);
        Task<IEnumerable<SurpriseRewardPool>> GetAllAsync();
        Task DeleteAsync(int rewardId);
        Task UpdateAsync(SurpriseRewardPool reward);

    }
}