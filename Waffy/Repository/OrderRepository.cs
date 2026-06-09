using Dapper;
using Microsoft.Data.SqlClient;
using Waffy.Interfaces;
using Waffy.Models;

namespace Waffy.Repository
{
    public class OrderRepository : IOrderRepository
    {
        private readonly string _conn;

        public OrderRepository(IConfiguration cfg)
            => _conn = cfg.GetConnectionString("WaffyConnection")!;

        // ─────────────────────────────────────────────────────────
        public async Task<int> CreateAsync(Order order)
        {
            using var conn = new SqlConnection(_conn);

            var nextNumber = await conn.ExecuteScalarAsync<int>(@"
        SELECT ISNULL(MAX(StoreOrderNumber), 0) + 1
        FROM [Order]
        WHERE IdStore = @IdStore",
                new { order.IdStore });

            order.StoreOrderNumber = nextNumber;

            return await conn.ExecuteScalarAsync<int>(@"
        INSERT INTO [Order]
            (IdUser, IdStore, OrderCode, Status, PaymentMethod, PaymentStatus,
             RecipientName, RecipientPhone, DeliveryAddress, DeliveryFee,
             Notes, CreatedAt, StoreOrderNumber,
             CliQVerificationCode, CliQVerified)
        OUTPUT INSERTED.IdOrder
        VALUES
            (@IdUser, @IdStore, @OrderCode, @Status, @PaymentMethod, @PaymentStatus,
             @RecipientName, @RecipientPhone, @DeliveryAddress, @DeliveryFee,
             @Notes, @CreatedAt, @StoreOrderNumber,
             @CliQVerificationCode, @CliQVerified)",
                order);
        }

        public async Task AddItemAsync(OrderItem item)
        {
            using var conn = new SqlConnection(_conn);
            await conn.ExecuteAsync(@"
                INSERT INTO OrderItem
                    (IdOrder, IdProduct, ProductNameSnapshot, Quantity, UnitPrice, SalePrice)
                VALUES
                    (@IdOrder, @IdProduct, @ProductNameSnapshot, @Quantity, @UnitPrice, @SalePrice)",
                item);
        }

        public async Task<IEnumerable<Order>> GetByUserIdAsync(int userId)
        {
            using var conn = new SqlConnection(_conn);
            return await conn.QueryAsync<Order>(@"
                SELECT o.*, s.Name AS StoreName
                FROM   [Order] o
                JOIN   Store   s ON s.IdStore = o.IdStore
                WHERE  o.IdUser = @UserId
                ORDER  BY o.CreatedAt DESC",
                new { UserId = userId });
        }

        public async Task<IEnumerable<OrderItem>> GetItemsByOrderIdAsync(int idOrder)
        {
            using var conn = new SqlConnection(_conn);
            return await conn.QueryAsync<OrderItem>(
                "SELECT * FROM OrderItem WHERE IdOrder = @IdOrder",
                new { IdOrder = idOrder });
        }

        public async Task<Order?> GetByIdAsync(int orderId)
        {
            using var conn = new SqlConnection(_conn);
            return await conn.QueryFirstOrDefaultAsync<Order>(@"
                SELECT o.*, s.Name AS StoreName
                FROM   [Order] o
                LEFT JOIN Store s ON s.IdStore = o.IdStore
                WHERE  o.IdOrder = @OrderId",
                new { OrderId = orderId });
        }

        public async Task<IEnumerable<Order>> GetByStoreIdAsync(int storeId)
        {
            using var conn = new SqlConnection(_conn);
            return await conn.QueryAsync<Order>(@"
                SELECT o.*, s.Name AS StoreName
                FROM   [Order] o
                LEFT JOIN Store s ON s.IdStore = o.IdStore
                WHERE  o.IdStore = @StoreId
                ORDER  BY o.CreatedAt DESC",
                new { StoreId = storeId });
        }

        public async Task<(decimal Items, decimal Delivery)> GetTotalRevenueByStoreAsync(int storeId)
        {
            using var conn = new SqlConnection(_conn);
            var result = await conn.QueryFirstOrDefaultAsync(@"
        SELECT
            ISNULL(SUM(sub.ItemsTotal), 0) AS Items,
            ISNULL(SUM(sub.DeliveryFee),  0) AS Delivery
        FROM (
            SELECT
                o.IdOrder,
                SUM(oi.Quantity * ISNULL(oi.SalePrice, oi.UnitPrice)) AS ItemsTotal,
                o.DeliveryFee
            FROM [Order] o
            INNER JOIN OrderItem oi ON oi.IdOrder = o.IdOrder
            WHERE o.IdStore = @StoreId
              AND o.Status  = 'Delivered'
            GROUP BY o.IdOrder, o.DeliveryFee
        ) sub",
                new { StoreId = storeId });

            return (
                Items: (decimal)(result?.Items ?? 0),
                Delivery: (decimal)(result?.Delivery ?? 0)
            );
        }

        public async Task UpdateStatusAsync(int orderId, string status)
        {
            using var conn = new SqlConnection(_conn);
            await conn.ExecuteAsync(
                "UPDATE [Order] SET Status = @Status WHERE IdOrder = @OrderId",
                new { OrderId = orderId, Status = status });
        }

        /// <summary>
        /// Sets Status = 'Shipped' and saves the delivery-person details.
        /// </summary>
        public async Task UpdateDeliveryPersonAsync(int orderId, string name, string phone)
        {
            using var conn = new SqlConnection(_conn);
            await conn.ExecuteAsync(@"
                UPDATE [Order]
                SET    Status              = 'Shipped',
                       DeliveryPersonName  = @Name,
                       DeliveryPersonPhone = @Phone
                WHERE  IdOrder = @OrderId",
                new { OrderId = orderId, Name = name, Phone = phone });
        }

        public async Task<IEnumerable<CustomerSummary>> GetCustomersByStoreAsync(int storeId)
        {
            using var conn = new SqlConnection(_conn);
            return await conn.QueryAsync<CustomerSummary>(@"
                SELECT
                    u.FullName,
                    u.Email,
                    u.Phone,
                    COUNT(DISTINCT o.IdOrder)                                        AS OrderCount,
                    ISNULL(SUM(oi.Quantity * ISNULL(oi.SalePrice, oi.UnitPrice)), 0) AS TotalSpent,
                    MAX(o.CreatedAt)                                                 AS LastOrderDate
                FROM   [Order]   o
                INNER JOIN Users     u  ON u.UserId   = o.IdUser
                INNER JOIN OrderItem oi ON oi.IdOrder = o.IdOrder
                WHERE  o.IdStore = @StoreId
                GROUP  BY u.FullName, u.Email, u.Phone
                ORDER  BY TotalSpent DESC",
                new { StoreId = storeId });
        }

        public async Task UpdateCliQVerifiedAsync(int idOrder, bool verified)
        {
            using var conn = new SqlConnection(_conn);
            await conn.ExecuteAsync(
                "UPDATE [Order] SET CliQVerified=@v WHERE IdOrder=@id",
                new { v = verified, id = idOrder });
        }
        public async Task<Order?> GetByCodeAsync(string orderCode)
        {
            using var conn = new SqlConnection(_conn);
            return await conn.QueryFirstOrDefaultAsync<Order>(
                "SELECT * FROM [Order] WHERE OrderCode = @OrderCode",
                new { OrderCode = orderCode });
        }
    }
}