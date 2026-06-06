using Waffy.Models;

namespace Waffy.Interfaces
{
    public interface IComplaintRepository
    {
        Task<int> CreateAsync(Complaint complaint);
        Task<IEnumerable<Complaint>> GetByUserIdAsync(int userId);
        Task<IEnumerable<Complaint>> GetAllByTargetTypeAsync(string targetType); 
        Task UpdateStatusAsync(int id, string status);
        Task<IEnumerable<Complaint>> GetByClinicIdAsync(int clinicId);
        Task WarnAsync(string targetType, int targetId, string warnType, string message);
        Task SuspendAsync(string targetType, int targetId, string reason, string duration, string message);
        Task<IEnumerable<Complaint>> GetByStoreIdAsync(int storeId);
    }
}