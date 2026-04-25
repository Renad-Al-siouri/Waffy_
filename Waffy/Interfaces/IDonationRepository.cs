using Waffy.Models;

namespace Waffy.Interfaces
{
    public interface IDonationRepository
    {
        Task<int> CreateAsync(Donation donation);
        Task<IEnumerable<Donation>> GetByUserIdAsync(int userId);
    }
}