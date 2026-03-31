namespace Waffy.Repositories
{
    using Dapper;
    using Microsoft.Data.SqlClient;
    using System.Data;
    using Waffy.Models;
    using Waffy.Models.Waffy.Models;

    public class DangerousAnimalReportRepository
    {
        private readonly IConfiguration _config;

        public DangerousAnimalReportRepository(IConfiguration config)
        {
            _config = config;
        }

        private IDbConnection CreateConnection()
        {
            return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
        }

        public async Task<IEnumerable<DangerousAnimalReport>> GetAllAsync()
        {
            using var db = CreateConnection();
            return await db.QueryAsync<DangerousAnimalReport>("SELECT * FROM DangerousAnimalReport ORDER BY Id DESC");
        }

        public async Task<DangerousAnimalReport?> GetByIdAsync(int id)
        {
            using var db = CreateConnection();
            return await db.QueryFirstOrDefaultAsync<DangerousAnimalReport>(
                "SELECT * FROM DangerousAnimalReport WHERE Id = @Id", new { Id = id });
        }

        public async Task<int> CreateAsync(DangerousAnimalReport item)
        {
            using var db = CreateConnection();
            string sql = @"
        INSERT INTO DangerousAnimalReport
        (ReporterId, AnimalType, ProblemType, Severity, Description, City, Area,
         LocationText, Latitude, Longitude, Status, ReportCode, CreatedAt)
        VALUES
        (@ReporterId, @AnimalType, @ProblemType, @Severity, @Description, @City, @Area,
         @LocationText, @Latitude, @Longitude, @Status, @ReportCode, GETDATE())";
            return await db.ExecuteAsync(sql, item);
        }

        public async Task<int> UpdateAsync(DangerousAnimalReport item)
        {
            using var db = CreateConnection();
            string sql = @"
        UPDATE DangerousAnimalReport SET
            ReporterId = @ReporterId,
            AnimalType = @AnimalType,
            ProblemType = @ProblemType,
            Severity = @Severity,
            Description = @Description,
            City = @City,
            Area = @Area,
            LocationText = @LocationText,
            Latitude = @Latitude,
            Longitude = @Longitude,
            Status = @Status,
            ReportCode = @ReportCode
        WHERE Id = @Id";
            return await db.ExecuteAsync(sql, item);
        }

        public async Task<int> DeleteAsync(int id)
        {
            using var db = CreateConnection();
            return await db.ExecuteAsync("DELETE FROM DangerousAnimalReport WHERE Id = @Id", new { Id = id });
        }
    }
}
