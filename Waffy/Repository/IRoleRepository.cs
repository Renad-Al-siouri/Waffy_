using Waffy.Models;

namespace Waffy.Interfaces
{
    public interface IRoleRepository
    {
        // Get all roles from the database
        Task<IEnumerable<Role>> GetAllAsync();

        // Get a single role by its ID
        Task<Role> GetByIdAsync(int roleId);

        // Get a RoleId using the role name (e.g. "User", "Doctor")
        Task<int> GetRoleIdByNameAsync(string roleName);
    }
}