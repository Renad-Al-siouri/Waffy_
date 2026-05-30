using Waffy.Models;
using Waffy.Models.ViewModels;

namespace Waffy.Interfaces
{
    public interface IDoctorRepository
    {
        Task<IEnumerable<DoctorListItemViewModel>> GetByClinicIdAsync(int clinicId);
        Task<int> GetNextDoctorIdAsync();
        Task<bool> EmailExistsAsync(string email);
        Task<int> CreateUserAsync(string fullName, string email,
                                  string phone, string password);
        Task AddDoctorAsync(Doctor doctor);
        Task RemoveFromClinicAsync(int idDoctor);
    }
}