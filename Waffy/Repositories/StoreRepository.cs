namespace Waffy.Repositories
{
    using Dapper;
    using Microsoft.Data.SqlClient;
    using System.Data;
    using Waffy.Models;
    using Waffy.Models.Waffy.Models;

    public class StoreRepository
    {
        private readonly IConfiguration _config;

        public StoreRepository(IConfiguration config)
        {
            _config = config;
        }

        private IDbConnection CreateConnection()
        {
            return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
        }

        public async Task<IEnumerable<Store>> GetAllAsync()
        {
            using var db = CreateConnection();
            return await db.QueryAsync<Store>("SELECT * FROM Store ORDER BY Id DESC");
        }

        public async Task<Store?> GetByIdAsync(int id)
        {
            using var db = CreateConnection();
            return await db.QueryFirstOrDefaultAsync<Store>("SELECT * FROM Store WHERE Id = @Id", new { Id = id });
        }

        public async Task<int> CreateAsync(Store item)
        {
            using var db = CreateConnection();
            string sql = @"
        INSERT INTO Store
        (OwnerUserId, Name, StoreType, Description, LogoUrl, CoverImageUrl, Email, Phone, WhatsApp,
         Country, City, Area, Address, Latitude, Longitude, DeliveryAvailable, DeliveryAreas,
         DeliveryFee, DeliveryTime, OpenTime, CloseTime, WorkingDays, AcceptedPayments,
         VerificationStatus, VerifiedAt, VerifiedByAdminId, Rating, CreatedAt)
        VALUES
        (@OwnerUserId, @Name, @StoreType, @Description, @LogoUrl, @CoverImageUrl, @Email, @Phone, @WhatsApp,
         @Country, @City, @Area, @Address, @Latitude, @Longitude, @DeliveryAvailable, @DeliveryAreas,
         @DeliveryFee, @DeliveryTime, @OpenTime, @CloseTime, @WorkingDays, @AcceptedPayments,
         @VerificationStatus, @VerifiedAt, @VerifiedByAdminId, @Rating, GETDATE())";
            return await db.ExecuteAsync(sql, item);
        }

        public async Task<int> UpdateAsync(Store item)
        {
            using var db = CreateConnection();
            string sql = @"
        UPDATE Store SET
            OwnerUserId = @OwnerUserId,
            Name = @Name,
            StoreType = @StoreType,
            Description = @Description,
            LogoUrl = @LogoUrl,
            CoverImageUrl = @CoverImageUrl,
            Email = @Email,
            Phone = @Phone,
            WhatsApp = @WhatsApp,
            Country = @Country,
            City = @City,
            Area = @Area,
            Address = @Address,
            Latitude = @Latitude,
            Longitude = @Longitude,
            DeliveryAvailable = @DeliveryAvailable,
            DeliveryAreas = @DeliveryAreas,
            DeliveryFee = @DeliveryFee,
            DeliveryTime = @DeliveryTime,
            OpenTime = @OpenTime,
            CloseTime = @CloseTime,
            WorkingDays = @WorkingDays,
            AcceptedPayments = @AcceptedPayments,
            VerificationStatus = @VerificationStatus,
            VerifiedAt = @VerifiedAt,
            VerifiedByAdminId = @VerifiedByAdminId,
            Rating = @Rating
        WHERE Id = @Id";
            return await db.ExecuteAsync(sql, item);
        }

        public async Task<int> DeleteAsync(int id)
        {
            using var db = CreateConnection();
            return await db.ExecuteAsync("DELETE FROM Store WHERE Id = @Id", new { Id = id });
        }
    }
}
