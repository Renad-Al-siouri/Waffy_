namespace Waffy.Repositories
{
    using Dapper;
    using Microsoft.Data.SqlClient;
    using System.Data;
    using Waffy.Models;
    using Waffy.Models.Waffy.Models;

    public class CartItemRepository
    {
        private readonly IConfiguration _config;

        public CartItemRepository(IConfiguration config)
        {
            _config = config;
        }

        private IDbConnection CreateConnection()
        {
            return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
        }

        public async Task<IEnumerable<CartItem>> GetAllAsync()
        {
            using var db = CreateConnection();
            return await db.QueryAsync<CartItem>("SELECT * FROM CartItem ORDER BY Id DESC");
        }

        public async Task<CartItem?> GetByIdAsync(int id)
        {
            using var db = CreateConnection();
            return await db.QueryFirstOrDefaultAsync<CartItem>("SELECT * FROM CartItem WHERE Id = @Id", new { Id = id });
        }

        public async Task<int> CreateAsync(CartItem item)
        {
            using var db = CreateConnection();
            string sql = "INSERT INTO CartItem (CartId, ProductId, Quantity) VALUES (@CartId, @ProductId, @Quantity)";
            return await db.ExecuteAsync(sql, item);
        }

        public async Task<int> UpdateAsync(CartItem item)
        {
            using var db = CreateConnection();
            string sql = @"
        UPDATE CartItem SET
            CartId = @CartId,
            ProductId = @ProductId,
            Quantity = @Quantity
        WHERE Id = @Id";
            return await db.ExecuteAsync(sql, item);
        }

        public async Task<int> DeleteAsync(int id)
        {
            using var db = CreateConnection();
            return await db.ExecuteAsync("DELETE FROM CartItem WHERE Id = @Id", new { Id = id });
        }
    }
}
