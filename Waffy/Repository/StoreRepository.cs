using Dapper;
using Microsoft.Data.SqlClient;
using Waffy.Interfaces;
using Waffy.Models;

namespace Waffy.Repository
{
    public class StoreRepository : IStoreRepository
    {
        private readonly string _connectionString;

        public StoreRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("WaffyConnection")
                ?? throw new Exception("Connection string not found");
        }

        // ── جلب جميع المتاجر ─────────────────────────────────
        public async Task<IEnumerable<Store>> GetAllAsync()
        {
            const string sql = "SELECT * FROM Store";
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<Store>(sql);
        }

        // ── جلب متجر بالـ ID ──────────────────────────────────
        public async Task<Store?> GetByIdAsync(int idStore)
        {
            const string sql = "SELECT * FROM Store WHERE IdStore = @IdStore";
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<Store>(sql, new { IdStore = idStore });
        }

        // ── جلب متجر عبر OwnerUserId ─────────────────────────
        public async Task<Store?> GetByOwnerUserIdAsync(int ownerUserId)
        {
            const string sql = "SELECT * FROM Store WHERE OwnerUserId = @OwnerUserId";
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<Store>(sql, new { OwnerUserId = ownerUserId });
        }

        // ── إضافة متجر جديد ──────────────────────────────────
        public async Task<int> AddAsync(Store store)
        {
            const string sql = @"
                INSERT INTO Store
                (
                    OwnerUserId, Name, StoreType, Description, LogoUrl,
                    CoverImageUrl, Email, Phone, WhatsApp, Country,
                    City, Area, Address, Latitude, Longitude,
                    DeliveryAvailable, DeliveryAreas, OpenTime, CloseTime,
                    WorkingDays, AcceptedPayments, VerificationStatus, CreatedAt
                )
                VALUES
                (
                    @OwnerUserId, @Name, @StoreType, @Description, @LogoUrl,
                    @CoverImageUrl, @Email, @Phone, @WhatsApp, @Country,
                    @City, @Area, @Address, @Latitude, @Longitude,
                    @DeliveryAvailable, @DeliveryAreas, @OpenTime, @CloseTime,
                    @WorkingDays, @AcceptedPayments, @VerificationStatus, @CreatedAt
                );
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using var connection = new SqlConnection(_connectionString);
            return await connection.ExecuteScalarAsync<int>(sql, store);
        }

        // ── تحديث بيانات المتجر ───────────────────────────────
        public async Task UpdateAsync(Store store)
        {
            const string sql = @"
                UPDATE Store SET
                    Name                = @Name,
                    StoreType           = @StoreType,
                    Description         = @Description,
                    LogoUrl             = @LogoUrl,
                    CoverImageUrl       = @CoverImageUrl,
                    Email               = @Email,
                    Phone               = @Phone,
                    WhatsApp            = @WhatsApp,
                    Country             = @Country,
                    City                = @City,
                    Area                = @Area,
                    Address             = @Address,
                    Latitude            = @Latitude,
                    Longitude           = @Longitude,
                    DeliveryAvailable   = @DeliveryAvailable,
                    DeliveryAreas       = @DeliveryAreas,
                    DeliveryFee         = @DeliveryFee,
                    OpenTime            = @OpenTime,
                    CloseTime           = @CloseTime,
                    WorkingDays         = @WorkingDays,
                    AcceptedPayments    = @AcceptedPayments,
                    VerificationStatus  = @VerificationStatus
                WHERE IdStore = @IdStore";

            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(sql, store);
        }

        // ── حذف متجر ─────────────────────────────────────────
        public async Task DeleteAsync(int idStore)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();

            try
            {
                // 1. صور المنتجات (child of Product)
                await connection.ExecuteAsync(
                    @"DELETE pi FROM ProductImage pi
              INNER JOIN Product p ON pi.IdProduct = p.Id
              WHERE p.IdStore = @IdStore",
                    new { IdStore = idStore }, transaction);

                // 2. عناصر الأوردرات (child of Order)
                await connection.ExecuteAsync(
                    @"DELETE oi FROM OrderItem oi
              INNER JOIN [Order] o ON oi.IdOrder = o.IdOrder
              WHERE o.IdStore = @IdStore",
                    new { IdStore = idStore }, transaction);

                // 3. CartItem
                await connection.ExecuteAsync(
                    "DELETE FROM CartItem WHERE IdStore = @IdStore",
                    new { IdStore = idStore }, transaction);

                // 4. Category
                await connection.ExecuteAsync(
                    "DELETE FROM Category WHERE IdStore = @IdStore",
                    new { IdStore = idStore }, transaction);

                // 5. Order
                await connection.ExecuteAsync(
                    "DELETE FROM [Order] WHERE IdStore = @IdStore",
                    new { IdStore = idStore }, transaction);

                // 6. Product
                await connection.ExecuteAsync(
                    "DELETE FROM Product WHERE IdStore = @IdStore",
                    new { IdStore = idStore }, transaction);

                // 7. StoreDocument
                await connection.ExecuteAsync(
                    "DELETE FROM StoreDocument WHERE IdStore = @IdStore",
                    new { IdStore = idStore }, transaction);

                // 8. StoreImage
                await connection.ExecuteAsync(
                    "DELETE FROM StoreImage WHERE IdStore = @IdStore",
                    new { IdStore = idStore }, transaction);

                // 9. المتجر نفسه
                await connection.ExecuteAsync(
                    "DELETE FROM Store WHERE IdStore = @IdStore",
                    new { IdStore = idStore }, transaction);

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        // ── تحديث حالة الاعتماد ──────────────────────────────
        public async Task UpdateVerificationAsync(int idStore, string status)
        {
            const string sql = @"
                UPDATE Store
                SET VerificationStatus = @Status
                WHERE IdStore = @IdStore";

            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(sql, new { IdStore = idStore, Status = status });
        }

        // ── جلب المتاجر للأدمن (كل المتاجر مرتبة بالأحدث) ──
        public async Task<IEnumerable<Store>> GetPendingStoresAsync()
        {
            const string sql = "SELECT * FROM Store ORDER BY CreatedAt DESC";
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<Store>(sql);
        }

        // ── ✅ جلب المتاجر المعتمدة للعرض العام ──────────────
        public async Task<IEnumerable<Store>> GetApprovedStoresAsync()
        {
            const string sql = @"
                SELECT *
                FROM   Store
                WHERE  VerificationStatus = 'Approved'
                ORDER  BY CreatedAt DESC";

            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<Store>(sql);
        }

        public async Task<int?> GetOwnerUserIdAsync(int storeId)
        {
            const string sql = "SELECT IdOwner FROM Store WHERE IdStore = @Id";
            using var conn = new SqlConnection(_connectionString);
            return await conn.QueryFirstOrDefaultAsync<int?>(sql, new { Id = storeId });
        }
    }
}