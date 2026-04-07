using Waffy.Models;

namespace Waffy.Interfaces
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAllAsync();

        Task<User?> GetByIdAsync(int userId);

        Task<User?> GetByEmailAsync(string email);

        Task AddNormalUserAsync(User user);

        Task AddClinicAsync(User user);

        Task AddStoreAsync(User user);

        Task AddDoctorAsync(User user);

        Task UpdateAsync(User user);

        Task DeleteAsync(int userId);

        Task<LoginUserResult?> LoginAsync(string email, string passwordHash);
    }
}