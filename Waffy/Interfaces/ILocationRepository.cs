using Waffy.Models;

namespace Waffy.Interfaces
{
    public interface ILocationRepository
    {
        Task<IEnumerable<Governorate>> GetAllGovernoratesAsync();
        Task<IEnumerable<District>> GetAllDistrictsAsync();
    }
}