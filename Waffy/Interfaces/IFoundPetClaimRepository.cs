using Waffy.Models;
using Waffy.Models.ViewModels;

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
        Task<IEnumerable<FoundClaimAdminDto>> GetAllWithDetailsAsync();
        Task<IEnumerable<TopFinderDto>> GetTopFindersAsync();
        Task CancelRewardAsync(int claimId);
        Task ConfirmClaimAsync(int claimId);
        Task RejectClaimAsync(int claimId);
    }
}