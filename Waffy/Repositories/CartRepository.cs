namespace Waffy.Repositories
{
    using Dapper;
    using Microsoft.Data.SqlClient;
    using System.Data;
    using Waffy.Models;
    using Waffy.Models.Waffy.Models;

    public class CartRepository
    {
        private readonly IConfiguration _config;

        public CartRepository(IConfiguration config)
        {
            _config = config;
        }

        private IDbConnection CreateConnection()
        {
            return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
        }

        public async Task<IEnumerable<Cart>> GetAllAsync()
        {
            using var db = CreateConnection();
            return await db.QueryAsync<Cart>("SELECT * FROM Cart ORDER BY Id DESC");
        }

        public async Task<Cart?> GetByIdAsync(int id)
        {
            using var db = CreateConnection();
            return await db.QueryFirstOrDefaultAsync<Cart>("SELECT * FROM Cart WHERE Id = @Id", new { Id = id });
        }

        public async Task<Cart?> GetByUserIdAsync(int userId)
        {
            using var db = CreateConnection();
            return await db.QueryFirstOrDefaultAsync<Cart>("SELECT * FROM Cart WHERE UserId = @UserId", new { UserId = userId });
        }

        public async Task<int> CreateAsync(Cart item)
        {
            using var db = CreateConnection();
            string sql = "INSERT INTO Cart (UserId, CreatedAt) VALUES (@UserId, GETDATE())";
            return await db.ExecuteAsync(sql, item);
        }

        public async Task<int> UpdateAsync(Cart item)
        {
            using var db = CreateConnection();
            string sql = "UPDATE Cart SET UserId = @UserId WHERE Id = @Id";
            return await db.ExecuteAsync(sql, item);
        }

        public async Task<int> DeleteAsync(int id)
        {
            using var db = CreateConnection();
            return await db.ExecuteAsync("DELETE FROM Cart WHERE Id = @Id", new { Id = id });
        }
    }
}
