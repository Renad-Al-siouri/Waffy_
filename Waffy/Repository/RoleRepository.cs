using Dapper;
using Microsoft.Data.SqlClient;
using Waffy.Interfaces;
using Waffy.Models;

namespace Waffy.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        // Connection string is injected from appsettings.json
        private readonly string _connectionString;

        public RoleRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("WaffyConnection");
        }

        // -----------------------------------------------
        // Get all roles
        // -----------------------------------------------
        public async Task<IEnumerable<Role>> GetAllAsync()
        {
            var sql = "SELECT RoleId, RoleName FROM Roles";

            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<Role>(sql);
        }

        // -----------------------------------------------
        // Get a role by ID
        // -----------------------------------------------
        public async Task<Role> GetByIdAsync(int roleId)
        {
            var sql = "SELECT RoleId, RoleName FROM Roles WHERE RoleId = @RoleId";

            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<Role>(sql, new { RoleId = roleId });
        }

        // -----------------------------------------------
        // Get RoleId by role name
        // Used internally before inserting a new user
        // -----------------------------------------------
        public async Task<int> GetRoleIdByNameAsync(string roleName)
        {
            var sql = "SELECT RoleId FROM Roles WHERE RoleName = @RoleName";

            using var connection = new SqlConnection(_connectionString);
            return await connection.ExecuteScalarAsync<int>(sql, new { RoleName = roleName });
        }
    }
}