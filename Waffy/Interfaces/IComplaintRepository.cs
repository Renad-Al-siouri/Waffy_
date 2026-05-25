using Waffy.Models;

namespace Waffy.Interfaces
{
    public interface IComplaintRepository
    {
        Task<int> CreateAsync(Complaint complaint);
        Task<IEnumerable<Complaint>> GetByUserIdAsync(int userId);
        Task<IEnumerable<Complaint>> GetAllByTargetTypeAsync(string targetType); 
        Task UpdateStatusAsync(int id, string status);

        Task<IEnumerable<Complaint>> GetByStoreIdAsync(int storeId);
    }
}