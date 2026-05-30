using Waffy.Models;

namespace Waffy.Interfaces
{
    public interface IClinicScheduleRepository
    {
        Task<IEnumerable<ClinicSchedule>> GetByClinicIdAsync(int clinicId);
        Task DeleteByClinicIdAsync(int clinicId);
        Task InsertAsync(ClinicSchedule schedule);
        Task<int> GetNextIdAsync();
    }
}