namespace Waffy.Repositories
{
    using Dapper;
    using Microsoft.Data.SqlClient;
    using System.Data;
    using Waffy.Models;
    using Waffy.Models.Waffy.Models;

    public class OrderRepository
    {
        private readonly IConfiguration _config;

        public OrderRepository(IConfiguration config)
        {
            _config = config;
        }

        private IDbConnection CreateConnection()
        {
            return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
        }

        public async Task<IEnumerable<Order>> GetAllAsync()
        {
            using var db = CreateConnection();
            return await db.QueryAsync<Order>("SELECT * FROM [Order] ORDER BY Id DESC");
        }

        public async Task<Order?> GetByIdAsync(int id)
        {
            using var db = CreateConnection();
            return await db.QueryFirstOrDefaultAsync<Order>("SELECT * FROM [Order] WHERE Id = @Id", new { Id = id });
        }

        public async Task<int> CreateAsync(Order item)
        {
            using var db = CreateConnection();
            string sql = @"
        INSERT INTO [Order]
        (UserId, StoreId, OrderCode, Status, PaymentMethod, PaymentStatus, RecipientName,
         RecipientPhone, DeliveryAddress, DeliveryFee, Notes, CancellationReason, CreatedAt)
        VALUES
        (@UserId, @StoreId, @OrderCode, @Status, @PaymentMethod, @PaymentStatus, @RecipientName,
         @RecipientPhone, @DeliveryAddress, @DeliveryFee, @Notes, @CancellationReason, GETDATE())";
            return await db.ExecuteAsync(sql, item);
        }

        public async Task<int> UpdateAsync(Order item)
        {
            using var db = CreateConnection();
            string sql = @"
        UPDATE [Order] SET
            UserId = @UserId,
            StoreId = @StoreId,
            OrderCode = @OrderCode,
            Status = @Status,
            PaymentMethod = @PaymentMethod,
            PaymentStatus = @PaymentStatus,
            RecipientName = @RecipientName,
            RecipientPhone = @RecipientPhone,
            DeliveryAddress = @DeliveryAddress,
            DeliveryFee = @DeliveryFee,
            Notes = @Notes,
            CancellationReason = @CancellationReason
        WHERE Id = @Id";
            return await db.ExecuteAsync(sql, item);
        }

        public async Task<int> DeleteAsync(int id)
        {
            using var db = CreateConnection();
            return await db.ExecuteAsync("DELETE FROM [Order] WHERE Id = @Id", new { Id = id });
        }
    }
}
