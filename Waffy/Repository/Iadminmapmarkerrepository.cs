using Waffy.Models;

namespace Waffy.Interfaces
{
    public interface IAdminMapMarkerRepository
    {
        Task<IEnumerable<AdminMapMarker>> GetAllAsync();
        Task<IEnumerable<AdminMapMarker>> GetActiveAsync();
        Task<IEnumerable<AdminMapMarker>> GetByTypeAsync(string markerType);
        Task<AdminMapMarker?> GetByIdAsync(int id);
        Task<int> CreateAsync(AdminMapMarker marker);
        Task UpdateAsync(AdminMapMarker marker);
        Task UpdateStatusAsync(int id, string status);
        Task DeleteAsync(int id);

    }
}