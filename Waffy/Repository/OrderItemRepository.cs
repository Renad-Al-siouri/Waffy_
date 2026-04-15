using Dapper;
using Microsoft.Data.SqlClient;
using Waffy.Interfaces;
using Waffy.Models;

namespace Waffy.Repository
{
    public class OrderItemRepository : IOrderItemRepository
    {
        private readonly string _connectionString;

        public OrderItemRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("WaffyConnection")
                ?? throw new Exception("Connection string not found");
        }

        public async Task<IEnumerable<OrderItem>> GetByOrderIdAsync(int idOrder)
        {
            var sql = "SELECT * FROM OrderItem WHERE IdOrder = @IdOrder";
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<OrderItem>(sql, new { IdOrder = idOrder });
        }

        public async Task AddRangeAsync(IEnumerable<OrderItem> items)
        {
            var sql = @"
                INSERT INTO OrderItem
                    (IdOrder, IdProduct, ProductNameSnapshot, Quantity, UnitPrice, SalePrice)
                VALUES
                    (@IdOrder, @IdProduct, @ProductNameSnapshot, @Quantity, @UnitPrice, @SalePrice)";
            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(sql, items);
        }
    }
}