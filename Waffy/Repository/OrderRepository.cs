using Dapper;
using Microsoft.Data.SqlClient;
using Waffy.DTOs;
using Waffy.Interfaces;
using Waffy.Models;

namespace Waffy.Repository
{
    public class OrderRepository : IOrderRepository
    {
        private readonly string _connectionString;

        public OrderRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("WaffyConnection")
                ?? throw new Exception("Connection string not found");
        }

        public async Task<IEnumerable<Order>> GetByStoreIdAsync(int idStore)
        {
            var sql = @"SELECT * FROM [Order] 
                        WHERE IdStore = @IdStore 
                        ORDER BY CreatedAt DESC";
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<Order>(sql, new { IdStore = idStore });
        }

        public async Task<Order?> GetByIdAsync(int idOrder)
        {
            var sql = "SELECT * FROM [Order] WHERE IdOrder = @IdOrder";
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<Order>(sql, new { IdOrder = idOrder });
        }

        public async Task<IEnumerable<Order>> GetByStoreIdAndStatusAsync(int idStore, string status)
        {
            var sql = @"SELECT * FROM [Order] 
                        WHERE IdStore = @IdStore AND Status = @Status
                        ORDER BY CreatedAt DESC";
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<Order>(sql, new { IdStore = idStore, Status = status });
        }

        public async Task UpdateStatusAsync(int idOrder, string status)
        {
            var sql = "UPDATE [Order] SET Status = @Status WHERE IdOrder = @IdOrder";
            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(sql, new { IdOrder = idOrder, Status = status });
        }

        public async Task<int> GetOrdersCountByStoreAsync(int idStore)
        {
            var sql = "SELECT COUNT(*) FROM [Order] WHERE IdStore = @IdStore";
            using var connection = new SqlConnection(_connectionString);
            return await connection.ExecuteScalarAsync<int>(sql, new { IdStore = idStore });
        }

        public async Task<decimal> GetTotalRevenueByStoreAsync(int idStore)
        {
            var sql = @"
                SELECT ISNULL(SUM(oi.Quantity * ISNULL(oi.SalePrice, oi.UnitPrice)), 0)
                FROM [Order] o
                INNER JOIN OrderItem oi ON oi.IdOrder = o.IdOrder
                WHERE o.IdStore = @IdStore AND o.PaymentStatus = 'Paid'";
            using var connection = new SqlConnection(_connectionString);
            return await connection.ExecuteScalarAsync<decimal>(sql, new { IdStore = idStore });
        }

        // أضف هذه الـ method داخل class OrderRepository الموجودة

        public async Task<IEnumerable<CustomerStoreDto>> GetCustomersByStoreAsync(int idStore)
        {
            // ⚠️ تأكد من اسم الجدول — غيّر "Users" إذا كان اسمه مختلفاً
            var sql = @"
        SELECT
            u.UserId,
            u.FullName,
            u.Email,
            u.Phone,
            COUNT(DISTINCT o.IdOrder)                                    AS OrderCount,
            ISNULL(SUM(oi.Quantity * ISNULL(oi.SalePrice, oi.UnitPrice)), 0) AS TotalSpent,
            MAX(o.CreatedAt)                                             AS LastOrderDate
        FROM [Order] o
        INNER JOIN Users u  ON u.UserId = o.IdUser
        LEFT  JOIN OrderItem oi ON oi.IdOrder = o.IdOrder
        WHERE o.IdStore = @IdStore
        GROUP BY u.UserId, u.FullName, u.Email, u.Phone
        ORDER BY TotalSpent DESC";

            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<CustomerStoreDto>(sql, new { IdStore = idStore });
        }
    }
}