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
            const string sql = "DELETE FROM Store WHERE IdStore = @IdStore";
            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(sql, new { IdStore = idStore });
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
    }
}