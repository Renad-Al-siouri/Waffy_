using Waffy.Models;

namespace Waffy.Interfaces
{
    public interface IFoundPetClaimRepository
    {
        Task<int> AddAsync(FoundPetClaim claim);
        Task<IEnumerable<FoundPetClaim>> GetByReportIdAsync(int reportId);
        Task<IEnumerable<FoundPetClaim>> GetByUserIdAsync(int userId);
        Task<FoundPetClaim?> GetByIdAsync(int claimId);
        Task ConfirmAsync(int claimId);
        Task RejectAsync(int claimId);
    }
}