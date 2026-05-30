using Waffy.Models;

namespace Waffy.Interfaces
{
    public interface IClinicImageRepository
    {
        Task<IEnumerable<ClinicImage>> GetByClinicIdAsync(int clinicId);
        Task AddAsync(ClinicImage image);
        Task DeleteAsync(int id);
        Task<int> CountByClinicIdAsync(int clinicId);
    }
}