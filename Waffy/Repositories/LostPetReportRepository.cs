namespace Waffy.Repositories
{
    using Dapper;
    using Microsoft.Data.SqlClient;
    using System.Data;
    using Waffy.Models;
    using Waffy.Models.Waffy.Models;

    public class LostPetReportRepository
    {
        private readonly IConfiguration _config;

        public LostPetReportRepository(IConfiguration config)
        {
            _config = config;
        }

        private IDbConnection CreateConnection()
        {
            return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
        }

        public async Task<IEnumerable<LostPetReport>> GetAllAsync()
        {
            using var db = CreateConnection();
            return await db.QueryAsync<LostPetReport>("SELECT * FROM LostPetReport ORDER BY Id DESC");
        }

        public async Task<LostPetReport?> GetByIdAsync(int id)
        {
            using var db = CreateConnection();
            return await db.QueryFirstOrDefaultAsync<LostPetReport>(
                "SELECT * FROM LostPetReport WHERE Id = @Id", new { Id = id });
        }

        public async Task<int> CreateAsync(LostPetReport item)
        {
            using var db = CreateConnection();
            string sql = @"
        INSERT INTO LostPetReport
        (UserId, PetId, PetName, AnimalType, Breed, Color, LastSeenLocation, City, Area,
         Latitude, Longitude, DateLost, TimeLost, Description, SpecialMarks, MicrochipId,
         SecretQuestion, SecretAnswerHash, RewardAmount, RewardStatus, Status, ExpiryDate,
         ReportCode, CreatedAt)
        VALUES
        (@UserId, @PetId, @PetName, @AnimalType, @Breed, @Color, @LastSeenLocation, @City, @Area,
         @Latitude, @Longitude, @DateLost, @TimeLost, @Description, @SpecialMarks, @MicrochipId,
         @SecretQuestion, @SecretAnswerHash, @RewardAmount, @RewardStatus, @Status, @ExpiryDate,
         @ReportCode, GETDATE())";
            return await db.ExecuteAsync(sql, item);
        }

        public async Task<int> UpdateAsync(LostPetReport item)
        {
            using var db = CreateConnection();
            string sql = @"
        UPDATE LostPetReport SET
            UserId = @UserId,
            PetId = @PetId,
            PetName = @PetName,
            AnimalType = @AnimalType,
            Breed = @Breed,
            Color = @Color,
            LastSeenLocation = @LastSeenLocation,
            City = @City,
            Area = @Area,
            Latitude = @Latitude,
            Longitude = @Longitude,
            DateLost = @DateLost,
            TimeLost = @TimeLost,
            Description = @Description,
            SpecialMarks = @SpecialMarks,
            MicrochipId = @MicrochipId,
            SecretQuestion = @SecretQuestion,
            SecretAnswerHash = @SecretAnswerHash,
            RewardAmount = @RewardAmount,
            RewardStatus = @RewardStatus,
            Status = @Status,
            ExpiryDate = @ExpiryDate,
            ReportCode = @ReportCode
        WHERE Id = @Id";
            return await db.ExecuteAsync(sql, item);
        }

        public async Task<int> DeleteAsync(int id)
        {
            using var db = CreateConnection();
            return await db.ExecuteAsync("DELETE FROM LostPetReport WHERE Id = @Id", new { Id = id });
        }
    }
}
