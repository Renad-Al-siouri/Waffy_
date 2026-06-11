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
        Task<IEnumerable<DoctorAdminRowDto>> GetAllForAdminAsync(string? statusFilter);
        Task UpdateVerificationStatusAsync(int idDoctor, string status);
        Task<DoctorDetailViewModel?> GetDetailByIdAsync(int idDoctor);


        Task<DoctorFullDto?> GetFullInfoByUserIdAsync(int userId);
        Task UpdateProfessionalInfoAsync(int userId, Doctor data);
        Task UpdateWorkScheduleAsync(int userId, Doctor data);
        Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
        Task<IEnumerable<DoctorDocument>> GetDocumentsByDoctorIdAsync(int idDoctor);
        Task AddDocumentAsync(DoctorDocument doc);
        Task<int?> GetDoctorIdByUserIdAsync(int userId);
        Task UpdatePersonalInfoAsync(int userId, string fullName,string? phone, string? bio, string? profileImageUrl = null);
        Task<DoctorFullDto?> GetFullInfoByDoctorIdAsync(int idDoctor);
        Task UpdateDocumentStatusAsync(int docId, string status);

      

    }
}