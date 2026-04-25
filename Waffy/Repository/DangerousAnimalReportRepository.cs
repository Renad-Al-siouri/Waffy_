using Dapper;
using Microsoft.Data.SqlClient;
using Waffy.Interfaces;
using Waffy.Models;

namespace Waffy.Repository
{
    public class DangerousAnimalReportRepository : IDangerousAnimalReportRepository
    {
        private readonly string _connectionString;

        public DangerousAnimalReportRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("WaffyConnection")
                ?? throw new Exception("WaffyConnection not found");
        }

        // ✅ بدون IDENTITY → MAX(IdDangReport)+1
        public async Task<int> CreateAsync(DangerousAnimalReport report)
        {
            const string sql = @"
        INSERT INTO DangerousAnimalReport
            (IdDangReport, IdUser, AnimalType, ProblemType, Severity,
             Description, City, Area, LocationText, Latitude, Longitude,
             Status, ReportCode, CreatedAt)
        SELECT ISNULL(MAX(IdDangReport), 0) + 1,
               @IdUser, @AnimalType, @ProblemType, @Severity,
               @Description, @City, @Area, @LocationText, @Latitude, @Longitude,
               @Status, @ReportCode, @CreatedAt
        FROM DangerousAnimalReport;
        SELECT ISNULL(MAX(IdDangReport), 0) FROM DangerousAnimalReport;";

            using var conn = new SqlConnection(_connectionString);
            return await conn.ExecuteScalarAsync<int>(sql, new
            {
                report.IdUser,
                report.AnimalType,
                report.ProblemType,
                report.Severity,
                report.Description,
                report.City,
                report.Area,
                report.LocationText,
                report.Latitude,
                report.Longitude,
                report.Status,
                report.ReportCode,
                report.CreatedAt
            });
        }

        public async Task<IEnumerable<DangerousAnimalReport>> GetByUserIdAsync(int userId)
        {
            const string sql = @"
                SELECT * FROM DangerousAnimalReport
                WHERE IdUser = @UserId
                ORDER BY CreatedAt DESC";

            using var conn = new SqlConnection(_connectionString);
            return await conn.QueryAsync<DangerousAnimalReport>(sql, new { UserId = userId });
        }
    }
}