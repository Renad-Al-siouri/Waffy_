using Waffy.Models;
using Waffy.Models.DTOs;
using Waffy.Models.ViewModels;

namespace Waffy.Interfaces
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAllAsync();
        Task<IEnumerable<UserAdminDto>> GetAllForAdminAsync(
         string? search, string? status, string? roleName,
         string sortBy, int page, int pageSize);

        Task<UserAdminDto?> GetByIdForAdminAsync(int userId);

        Task<int> CountFilteredAsync(string? search, string? status, string? roleName);
        Task<int> CountByStatusAsync(string status);
        Task<int> CountNewThisMonthAsync();
        Task<int> CountTotalAsync();
        Task UpdateStatusAsync(int userId, string newStatus);
        Task<User?> GetByIdAsync(int userId);

        Task<User?> GetByEmailAsync(string email);

        Task AddNormalUserAsync(User user);

        Task AddClinicAsync(User user);

        Task AddStoreAsync(User user);

        Task AddDoctorAsync(User user);

        Task UpdateAsync(User user);

        Task DeleteAsync(int userId);

        Task<LoginUserResult?> LoginAsync(string email, string passwordHash);

        Task<string> GetRoleNameByIdAsync(int roleId);
        Task<IEnumerable<int>> GetUserIdsByCityAsync(string city);

        Task UpdateRenewalDiscountAsync(int userId);
        Task<IEnumerable<UserCityDto>> GetUsersByCityAsync(string city, string? area = null);
        Task IncrementWarningCountAsync(int userId);
    }
}