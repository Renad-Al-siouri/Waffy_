using Dapper;
using Microsoft.Data.SqlClient;
using Waffy.Interfaces;
using Waffy.Models;

namespace Waffy.Repository
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly string _connectionString;

        public CategoryRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("WaffyConnection")
                ?? throw new Exception("WaffyConnection not found");
        }

        public async Task<IEnumerable<Category>> GetByStoreIdAsync(int storeId)
        {
            var sql = "SELECT * FROM Category WHERE IdStore = @StoreId ORDER BY Name ASC";
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<Category>(sql, new { StoreId = storeId });
        }

        public async Task<Category?> GetByIdAsync(int id)
        {
            var sql = "SELECT * FROM Category WHERE IdCategory = @Id";
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<Category>(sql, new { Id = id });
        }

        public async Task<int> AddAsync(Category category)
        {
            var sql = @"DECLARE @NewId INT;
                        SELECT @NewId = ISNULL(MAX(IdCategory), 0) + 1 FROM Category;
                        INSERT INTO Category (IdCategory, IdStore, Name, Icon, CreatedAt)
                        VALUES (@NewId, @IdStore, @Name, @Icon, @CreatedAt);
                        SELECT @NewId;";
            using var connection = new SqlConnection(_connectionString);
            return await connection.ExecuteScalarAsync<int>(sql, new
            {
                category.IdStore,
                category.Name,
                Icon = string.IsNullOrEmpty(category.Icon) ? "📦" : category.Icon,
                CreatedAt = DateTime.UtcNow
            });
        }

        public async Task UpdateAsync(Category category)
        {
            var sql = "UPDATE Category SET Name = @Name, Icon = @Icon WHERE IdCategory = @IdCategory";
            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(sql, category);
        }

        public async Task DeleteAsync(int id)
        {
            var sql = "DELETE FROM Category WHERE IdCategory = @Id";
            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(sql, new { Id = id });
        }
    }
}