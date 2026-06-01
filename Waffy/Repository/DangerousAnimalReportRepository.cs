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

        private SqlConnection NewConn() => new(_connectionString);

        // ════════════════════════════════════════════════════════
        // CREATE — تم إصلاحه: الجدول صار IDENTITY لا نحتاج MAX+1
        // ════════════════════════════════════════════════════════
        public async Task<int> CreateAsync(DangerousAnimalReport r)
        {
            const string sql = @"
                INSERT INTO DangerousAnimalReport
                    (IdUser, AnimalType, ProblemType, Severity,
                     Description, City, Area, LocationText,
                     Latitude, Longitude, Status, ReportCode, CreatedAt)
                VALUES
                    (@IdUser, @AnimalType, @ProblemType, @Severity,
                     @Description, @City, @Area, @LocationText,
                     @Latitude, @Longitude, @Status, @ReportCode, @CreatedAt);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using var conn = NewConn();
            return await conn.ExecuteScalarAsync<int>(sql, new
            {
                r.IdUser,
                r.AnimalType,
                r.ProblemType,
                r.Severity,
                r.Description,
                r.City,
                r.Area,
                r.LocationText,
                r.Latitude,
                r.Longitude,
                r.Status,
                r.ReportCode,
                r.CreatedAt
            });
        }

        // ════════════════════════════════════════════════════════
        // GET ALL — للأدمن (مع اسم المبلّغ)
        // ════════════════════════════════════════════════════════
        public async Task<IEnumerable<DangerousAnimalReport>> GetAllAsync()
        {
            const string sql = @"
                SELECT d.*,
                       u.FullName AS ReporterName,
                       u.Phone    AS ReporterPhone
                FROM   DangerousAnimalReport d
                LEFT JOIN Users u ON u.UserId = d.IdUser
                ORDER BY d.CreatedAt DESC";

            using var conn = NewConn();
            return await conn.QueryAsync<DangerousAnimalReport>(sql);
        }

        // ════════════════════════════════════════════════════════
        // GET BY USER — للبروفايل (My Danger Reports)
        // ════════════════════════════════════════════════════════
        public async Task<IEnumerable<DangerousAnimalReport>> GetByUserIdAsync(int userId)
        {
            const string sql = @"
                SELECT * FROM DangerousAnimalReport
                WHERE  IdUser = @userId
                ORDER BY CreatedAt DESC";

            using var conn = NewConn();
            return await conn.QueryAsync<DangerousAnimalReport>(sql, new { userId });
        }

        // ════════════════════════════════════════════════════════
        // UPDATE STATUS
        // ════════════════════════════════════════════════════════
        public async Task UpdateStatusAsync(int id, string status)
        {
            const string sql = @"
                UPDATE DangerousAnimalReport
                SET    Status = @status
                WHERE  IdDangReport = @id";

            using var conn = NewConn();
            await conn.ExecuteAsync(sql, new { id, status });
        }

        // ════════════════════════════════════════════════════════
        // GET ACTIVE FOR MAP — فقط اللي عندهم إحداثيات وغير Resolved
        // ════════════════════════════════════════════════════════
        public async Task<IEnumerable<DangerousAnimalReport>> GetActiveForMapAsync()
        {
            const string sql = @"
                SELECT IdDangReport, AnimalType, ProblemType, Severity,
                       City, Area, Latitude, Longitude,
                       Status, ReportCode, CreatedAt
                FROM   DangerousAnimalReport
                WHERE  Status    != 'Resolved'
                  AND  Latitude  IS NOT NULL
                  AND  Longitude IS NOT NULL
                ORDER BY CreatedAt DESC";

            using var conn = NewConn();
            return await conn.QueryAsync<DangerousAnimalReport>(sql);
        }

        // ════════════════════════════════════════════════════════
        // GET BY ID — تفاصيل بلاغ واحد
        // ════════════════════════════════════════════════════════
        public async Task<DangerousAnimalReport?> GetByIdAsync(int idDangReport)
        {
            const string sql = @"
                SELECT d.*,
                       u.FullName AS ReporterName,
                       u.Phone    AS ReporterPhone
                FROM   DangerousAnimalReport d
                LEFT JOIN Users u ON u.UserId = d.IdUser
                WHERE  d.IdDangReport = @idDangReport";

            using var conn = NewConn();
            return await conn.QueryFirstOrDefaultAsync<DangerousAnimalReport>(
                sql, new { idDangReport });
        }

        // ════════════════════════════════════════════════════════
        // DELETE
        // ════════════════════════════════════════════════════════
        public async Task DeleteAsync(int idDangReport)
        {
            const string sql = @"
                DELETE FROM DangerousAnimalReport
                WHERE  IdDangReport = @idDangReport";

            using var conn = NewConn();
            await conn.ExecuteAsync(sql, new { idDangReport });
        }

        // ════════════════════════════════════════════════════════
        // ADD IMAGE
        // ════════════════════════════════════════════════════════
        public async Task AddImageAsync(DangerReportImage image)
        {
            const string sql = @"

        INSERT INTO DangerReportImage (IdDangReport, ImageUrl, SortOrder, CreatedAt)
        VALUES (@IdDangReport, @ImageUrl, @SortOrder, @CreatedAt)";

            using var conn = NewConn();
            await conn.ExecuteAsync(sql, image);
        }

        // ════════════════════════════════════════════════════════
        // GET IMAGES BY REPORT ID
        // ════════════════════════════════════════════════════════
        public async Task<IEnumerable<DangerReportImage>> GetImagesByReportIdAsync(int idDangReport)
        {
            const string sql = @"
        SELECT * FROM DangerReportImage
        WHERE IdDangReport = @idDangReport
        ORDER BY SortOrder ASC";

            using var conn = NewConn();
            return await conn.QueryAsync<DangerReportImage>(sql, new { idDangReport });
        }
    }
}