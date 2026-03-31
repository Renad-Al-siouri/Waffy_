namespace Waffy.Repositories
{
    using Dapper;
    using Microsoft.Data.SqlClient;
    using System.Data;
    using Waffy.Models;
    using Waffy.Models.Waffy.Models;

    public class ClinicRepository
    {
        private readonly IConfiguration _config;

        public ClinicRepository(IConfiguration config)
        {
            _config = config;
        }

        private IDbConnection CreateConnection()
        {
            return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
        }

        public async Task<IEnumerable<Clinic>> GetAllAsync()
        {
            using var db = CreateConnection();
            return await db.QueryAsync<Clinic>("SELECT * FROM Clinic ORDER BY Id DESC");
        }

        public async Task<Clinic?> GetByIdAsync(int id)
        {
            using var db = CreateConnection();
            return await db.QueryFirstOrDefaultAsync<Clinic>("SELECT * FROM Clinic WHERE Id = @Id", new { Id = id });
        }

        public async Task<int> CreateAsync(Clinic clinic)
        {
            using var db = CreateConnection();
            string sql = @"
        INSERT INTO Clinic
        (OwnerUserId, Name, ClinicType, Phone, Email, Description, City, Area, Address,
         Latitude, Longitude, DefaultOpenTime, DefaultCloseTime, HasEmergency,
         VerificationStatus, Rating, LogoUrl, CoverImageUrl, CreatedAt)
        VALUES
        (@OwnerUserId, @Name, @ClinicType, @Phone, @Email, @Description, @City, @Area, @Address,
         @Latitude, @Longitude, @DefaultOpenTime, @DefaultCloseTime, @HasEmergency,
         @VerificationStatus, @Rating, @LogoUrl, @CoverImageUrl, GETDATE())";
            return await db.ExecuteAsync(sql, clinic);
        }

        public async Task<int> UpdateAsync(Clinic clinic)
        {
            using var db = CreateConnection();
            string sql = @"
        UPDATE Clinic SET
            OwnerUserId = @OwnerUserId,
            Name = @Name,
            ClinicType = @ClinicType,
            Phone = @Phone,
            Email = @Email,
            Description = @Description,
            City = @City,
            Area = @Area,
            Address = @Address,
            Latitude = @Latitude,
            Longitude = @Longitude,
            DefaultOpenTime = @DefaultOpenTime,
            DefaultCloseTime = @DefaultCloseTime,
            HasEmergency = @HasEmergency,
            VerificationStatus = @VerificationStatus,
            Rating = @Rating,
            LogoUrl = @LogoUrl,
            CoverImageUrl = @CoverImageUrl
        WHERE Id = @Id";
            return await db.ExecuteAsync(sql, clinic);
        }

        public async Task<int> DeleteAsync(int id)
        {
            using var db = CreateConnection();
            return await db.ExecuteAsync("DELETE FROM Clinic WHERE Id = @Id", new { Id = id });
        }
    }
}
