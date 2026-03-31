namespace Waffy.Repositories
{
    using Dapper;
    using Microsoft.Data.SqlClient;
    using System.Data;
    using Waffy.Models;
    using Waffy.Models.Waffy.Models;

    public class OrderItemRepository
    {
        private readonly IConfiguration _config;

        public OrderItemRepository(IConfiguration config)
        {
            _config = config;
        }

        private IDbConnection CreateConnection()
        {
            return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
        }

        public async Task<IEnumerable<OrderItem>> GetAllAsync()
        {
            using var db = CreateConnection();
            return await db.QueryAsync<OrderItem>("SELECT * FROM OrderItem ORDER BY Id DESC");
        }

        public async Task<OrderItem?> GetByIdAsync(int id)
        {
            using var db = CreateConnection();
            return await db.QueryFirstOrDefaultAsync<OrderItem>("SELECT * FROM OrderItem WHERE Id = @Id", new { Id = id });
        }

        public async Task<int> CreateAsync(OrderItem item)
        {
            using var db = CreateConnection();
            string sql = @"
        INSERT INTO OrderItem (OrderId, ProductId, ProductNameSnapshot, Quantity, UnitPrice, SalePrice)
        VALUES (@OrderId, @ProductId, @ProductNameSnapshot, @Quantity, @UnitPrice, @SalePrice)";
            return await db.ExecuteAsync(sql, item);
        }

        public async Task<int> UpdateAsync(OrderItem item)
        {
            using var db = CreateConnection();
            string sql = @"
        UPDATE OrderItem SET
            OrderId = @OrderId,
            ProductId = @ProductId,
            ProductNameSnapshot = @ProductNameSnapshot,
            Quantity = @Quantity,
            UnitPrice = @UnitPrice,
            SalePrice = @SalePrice
        WHERE Id = @Id";
            return await db.ExecuteAsync(sql, item);
        }

        public async Task<int> DeleteAsync(int id)
        {
            using var db = CreateConnection();
            return await db.ExecuteAsync("DELETE FROM OrderItem WHERE Id = @Id", new { Id = id });
        }
    }
}
