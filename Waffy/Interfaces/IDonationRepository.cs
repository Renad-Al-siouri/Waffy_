using Waffy.Models;

namespace Waffy.Interfaces
{
    public interface IDonationRepository
    {
        Task<int> CreateAsync(Donation donation);
        Task<IEnumerable<Donation>> GetByUserIdAsync(int userId);
        Task<IEnumerable<Donation>> GetAllAsync();
        Task<IEnumerable<Donation>> GetAllWithDonorAsync();
        Task UpdateStatusAsync(int id, string status);
        Task<Donation?> GetByIdAsync(int id);
    }
}