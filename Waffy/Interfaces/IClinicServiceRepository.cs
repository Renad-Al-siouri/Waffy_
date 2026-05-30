using Waffy.Models;

namespace Waffy.Interfaces
{
    public interface IClinicServiceRepository
    {
        Task<IEnumerable<ClinicService>> GetByClinicIdAsync(int clinicId);
        Task DeleteByClinicIdAsync(int clinicId);
        Task InsertAsync(ClinicService service);
        Task<int> GetNextIdAsync();
    }
}