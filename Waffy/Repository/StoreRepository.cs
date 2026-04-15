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

        public async Task<IEnumerable<Store>> GetAllAsync()
        {
            var sql = "SELECT * FROM Store";
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<Store>(sql);
        }

        public async Task<Store?> GetByIdAsync(int idStore)
        {
            var sql = "SELECT * FROM Store WHERE IdStore = @IdStore";
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<Store>(sql, new { IdStore = idStore });
        }

        public async Task<Store?> GetByOwnerUserIdAsync(int ownerUserId)
        {
            var sql = "SELECT * FROM Store WHERE OwnerUserId = @OwnerUserId";
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<Store>(sql, new { OwnerUserId = ownerUserId });
        }

        public async Task<int> AddAsync(Store store)
        {
            var sql = @"
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

        public async Task UpdateAsync(Store store)
        {
            var sql = @"
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

        public async Task DeleteAsync(int idStore)
        {
            var sql = "DELETE FROM Store WHERE IdStore = @IdStore";
            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(sql, new { IdStore = idStore });
        }
        public async Task UpdateVerificationAsync(int idStore, string status)
        {
            var sql = @"UPDATE Store SET VerificationStatus = @Status WHERE IdStore = @IdStore";
            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(sql, new { IdStore = idStore, Status = status });
        }

        public async Task<IEnumerable<Store>> GetPendingStoresAsync()
        {
            var sql = "SELECT * FROM Store ORDER BY CreatedAt DESC";
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<Store>(sql);
        }
    }
}