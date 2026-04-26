using Waffy.Models;

namespace Waffy.Interfaces
{
    public interface IComplaintRepository
    {
        Task<int> CreateAsync(Complaint complaint);
        Task<IEnumerable<Complaint>> GetByUserIdAsync(int userId);
    }
}