namespace Waffy.Repositories
{
    using Dapper;
    using Microsoft.Data.SqlClient;
    using System.Data;
    using Waffy.Models;
    using Waffy.Models.Waffy.Models;

    public class FoundPetClaimRepository
    {
        private readonly IConfiguration _config;

        public FoundPetClaimRepository(IConfiguration config)
        {
            _config = config;
        }

        private IDbConnection CreateConnection()
        {
            return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
        }

        public async Task<IEnumerable<FoundPetClaim>> GetAllAsync()
        {
            using var db = CreateConnection();
            return await db.QueryAsync<FoundPetClaim>("SELECT * FROM FoundPetClaim ORDER BY Id DESC");
        }

        public async Task<FoundPetClaim?> GetByIdAsync(int id)
        {
            using var db = CreateConnection();
            return await db.QueryFirstOrDefaultAsync<FoundPetClaim>(
                "SELECT * FROM FoundPetClaim WHERE Id = @Id", new { Id = id });
        }

        public async Task<int> CreateAsync(FoundPetClaim item)
        {
            using var db = CreateConnection();
            string sql = @"
        INSERT INTO FoundPetClaim
        (LostReportId, FinderId, FoundLocation, City, Area, Latitude, Longitude, FoundDate,
         FoundTime, Description, AnimalType, Color, SpecialMarks, MatchScore, AttemptsCount,
         FinderCode, OwnerCode, CodesExchanged, Status, ExpiresAt, CreatedAt)
        VALUES
        (@LostReportId, @FinderId, @FoundLocation, @City, @Area, @Latitude, @Longitude, @FoundDate,
         @FoundTime, @Description, @AnimalType, @Color, @SpecialMarks, @MatchScore, @AttemptsCount,
         @FinderCode, @OwnerCode, @CodesExchanged, @Status, @ExpiresAt, GETDATE())";
            return await db.ExecuteAsync(sql, item);
        }

        public async Task<int> UpdateAsync(FoundPetClaim item)
        {
            using var db = CreateConnection();
            string sql = @"
        UPDATE FoundPetClaim SET
            LostReportId = @LostReportId,
            FinderId = @FinderId,
            FoundLocation = @FoundLocation,
            City = @City,
            Area = @Area,
            Latitude = @Latitude,
            Longitude = @Longitude,
            FoundDate = @FoundDate,
            FoundTime = @FoundTime,
            Description = @Description,
            AnimalType = @AnimalType,
            Color = @Color,
            SpecialMarks = @SpecialMarks,
            MatchScore = @MatchScore,
            AttemptsCount = @AttemptsCount,
            FinderCode = @FinderCode,
            OwnerCode = @OwnerCode,
            CodesExchanged = @CodesExchanged,
            Status = @Status,
            ExpiresAt = @ExpiresAt
        WHERE Id = @Id";
            return await db.ExecuteAsync(sql, item);
        }

        public async Task<int> DeleteAsync(int id)
        {
            using var db = CreateConnection();
            return await db.ExecuteAsync("DELETE FROM FoundPetClaim WHERE Id = @Id", new { Id = id });
        }
    }
}
