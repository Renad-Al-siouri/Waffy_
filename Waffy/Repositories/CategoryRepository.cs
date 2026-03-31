namespace Waffy.Repositories
{
    using Dapper;
    using Microsoft.Data.SqlClient;
    using System.Data;
    using Waffy.Models;
    using Waffy.Models.Waffy.Models;

    public class CategoryRepository
    {
        private readonly IConfiguration _config;

        public CategoryRepository(IConfiguration config)
        {
            _config = config;
        }

        private IDbConnection CreateConnection()
        {
            return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
        }

        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            using var db = CreateConnection();
            return await db.QueryAsync<Category>("SELECT * FROM Category ORDER BY Id DESC");
        }

        public async Task<Category?> GetByIdAsync(int id)
        {
            using var db = CreateConnection();
            return await db.QueryFirstOrDefaultAsync<Category>("SELECT * FROM Category WHERE Id = @Id", new { Id = id });
        }

        public async Task<int> CreateAsync(Category item)
        {
            using var db = CreateConnection();
            string sql = @"INSERT INTO Category (StoreId, Name, Icon, CreatedAt)
                       VALUES (@StoreId, @Name, @Icon, GETDATE())";
            return await db.ExecuteAsync(sql, item);
        }

        public async Task<int> UpdateAsync(Category item)
        {
            using var db = CreateConnection();
            string sql = @"UPDATE Category SET StoreId = @StoreId, Name = @Name, Icon = @Icon WHERE Id = @Id";
            return await db.ExecuteAsync(sql, item);
        }

        public async Task<int> DeleteAsync(int id)
        {
            using var db = CreateConnection();
            return await db.ExecuteAsync("DELETE FROM Category WHERE Id = @Id", new { Id = id });
        }
    }
}
