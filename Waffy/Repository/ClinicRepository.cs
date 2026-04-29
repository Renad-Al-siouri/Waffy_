using Dapper;
using Microsoft.Data.SqlClient;
using Waffy.Interfaces;
using Waffy.Models;

namespace Waffy.Repositories
{
    public class ClinicRepository : IClinicRepository
    {
        private readonly string _ConnectionString;

        public ClinicRepository(IConfiguration configuration)
        {
            _ConnectionString = configuration.GetConnectionString("WaffyConnection")!;
        }

        // ✅ أضف عيادة جديدة وأرجع IdClinic
        public async Task<int> AddAsync(Clinic clinic)
        {
            using var connection = new SqlConnection(_ConnectionString);
            var sql = @"
        INSERT INTO Clinic 
            (OwnerUserId, Name, ClinicType, Phone, Email, Description,
             City, Area, Address, Latitude, Longitude,
             DefaultOpenTime, DefaultCloseTime, HasEmergency,
             VerificationStatus, Rating, LogoUrl, CoverImageUrl, CreatedAt)
        VALUES 
            (@OwnerUserId, @Name, @ClinicType, @Phone, @Email, @Description,
             @City, @Area, @Address, @Latitude, @Longitude,
             @DefaultOpenTime, @DefaultCloseTime, @HasEmergency,
             @VerificationStatus, @Rating, @LogoUrl, @CoverImageUrl, @CreatedAt);
        SELECT CAST(SCOPE_IDENTITY() AS INT);";

            return await connection.ExecuteScalarAsync<int>(sql, clinic);
        }

        // ✅ جلب عيادة بالـ ID
        public async Task<Clinic?> GetByIdAsync(int idClinic)
        {
            using var connection = new SqlConnection(_ConnectionString);
            var sql = "SELECT * FROM Clinic WHERE IdClinic = @IdClinic";
            return await connection.QueryFirstOrDefaultAsync<Clinic>(sql, new { IdClinic = idClinic });
        }

        // ✅ جلب عيادة عن طريق OwnerUserId
        public async Task<Clinic?> GetByOwnerUserIdAsync(int ownerUserId)
        {
            using var connection = new SqlConnection(_ConnectionString);
            var sql = "SELECT * FROM Clinic WHERE OwnerUserId = @OwnerUserId";
            return await connection.QueryFirstOrDefaultAsync<Clinic>(sql, new { OwnerUserId = ownerUserId });
        }

        // ✅ تحديث بيانات العيادة
        public async Task UpdateAsync(Clinic clinic)
        {
            using var connection = new SqlConnection(_ConnectionString);
            var sql = @"
                UPDATE Clinic SET
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
                    LogoUrl = @LogoUrl,
                    CoverImageUrl = @CoverImageUrl
                WHERE IdClinic = @IdClinic";

            await connection.ExecuteAsync(sql, clinic);
        }

        // ✅ حذف عيادة
        public async Task DeleteAsync(int idClinic)
        {
            using var connection = new SqlConnection(_ConnectionString);
            var sql = "DELETE FROM Clinic WHERE IdClinic = @IdClinic";
            await connection.ExecuteAsync(sql, new { IdClinic = idClinic });
        }

        // ✅ جلب كل العيادات
        public async Task<IEnumerable<Clinic>> GetAllAsync()
        {
            const string sql = "SELECT * FROM Clinic ORDER BY CreatedAt DESC";
            using var connection = new SqlConnection(_ConnectionString);
            return await connection.QueryAsync<Clinic>(sql);
        }
    }
}