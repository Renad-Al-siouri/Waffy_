namespace Waffy.Repositories
{
    using Dapper;
    using Microsoft.Data.SqlClient;
    using System.Data;
    using Waffy.Models;

    public class RoleRepository
    {
        private readonly IConfiguration _config;

        public RoleRepository(IConfiguration config)
        {
            _config = config;
        }

        private IDbConnection CreateConnection()
        {
            return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
        }

        // ✅ Get All
        public async Task<IEnumerable<Role>> GetAllAsync()
        {
            using var db = CreateConnection();
            return await db.QueryAsync<Role>("SELECT * FROM Roles");
        }

        // ✅ Get By Id
        public async Task<Role> GetByIdAsync(int id)
        {
            using var db = CreateConnection();
            return await db.QueryFirstOrDefaultAsync<Role>(
                "SELECT * FROM Roles WHERE Id = @Id",
                new { Id = id });
        }

        // ✅ Create
        public async Task<int> CreateAsync(Role role)
        {
            using var db = CreateConnection();
            var sql = "INSERT INTO Roles (Name) VALUES (@Name)";
            return await db.ExecuteAsync(sql, role);
        }

        // ✅ Update
        public async Task<int> UpdateAsync(Role role)
        {
            using var db = CreateConnection();
            var sql = "UPDATE Roles SET Name = @Name WHERE Id = @Id";
            return await db.ExecuteAsync(sql, role);
        }

        // ✅ Delete
        public async Task<int> DeleteAsync(int id)
        {
            using var db = CreateConnection();
            var sql = "DELETE FROM Roles WHERE Id = @Id";
            return await db.ExecuteAsync(sql, new { Id = id });
        }
    }
}
