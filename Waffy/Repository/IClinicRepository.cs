using Waffy.Models;

namespace Waffy.Interfaces
{
    public interface IClinicRepository
    {
        Task<int> AddAsync(Clinic clinic);
        Task<Clinic?> GetByIdAsync(int idClinic);
        Task<Clinic?> GetByOwnerUserIdAsync(int ownerUserId);
        Task UpdateAsync(Clinic clinic);
        Task DeleteAsync(int idClinic);
    }
}