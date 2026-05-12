using Dapper;
using Microsoft.Data.SqlClient;
using Waffy.Interfaces;
using Waffy.Models;

namespace Waffy.Repository
{
    public class LostPetRepository : ILostPetRepository
    {
        private readonly string _connectionString;

        public LostPetRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("WaffyConnection")
                ?? throw new Exception("WaffyConnection not found");
        }

        // ── Active lost reports ──────────────────────────────
        public async Task<IEnumerable<LostPetReport>> GetActiveAsync()
        {
            const string sql = @"
                SELECT * FROM LostPetReport
                WHERE Status = 'Active'
                ORDER BY CreatedAt DESC";

            using var conn = new SqlConnection(_connectionString);
            return await conn.QueryAsync<LostPetReport>(sql);
        }

        // ── User's own reports ───────────────────────────────
        public async Task<IEnumerable<LostPetReport>> GetByUserIdAsync(int userId)
        {
            const string sql = @"
                SELECT * FROM LostPetReport
                WHERE IdUser = @UserId
                ORDER BY CreatedAt DESC";

            using var conn = new SqlConnection(_connectionString);
            return await conn.QueryAsync<LostPetReport>(sql, new { UserId = userId });
        }

        // ── Single report ────────────────────────────────────
        public async Task<LostPetReport?> GetByIdAsync(int idLostReport)
        {
            const string sql = "SELECT * FROM LostPetReport WHERE IdLostReport = @IdLostReport";
            using var conn = new SqlConnection(_connectionString);
            return await conn.QueryFirstOrDefaultAsync<LostPetReport>(sql, new { IdLostReport = idLostReport });
        }

        // ── Get by ReportCode ────────────────────────────────
        public async Task<LostPetReport?> GetByCodeAsync(string reportCode)
        {
            const string sql = "SELECT * FROM LostPetReport WHERE ReportCode = @ReportCode";
            using var conn = new SqlConnection(_connectionString);
            return await conn.QueryFirstOrDefaultAsync<LostPetReport>(sql, new { ReportCode = reportCode });
        }

        // ── Add report → return IdLostReport ─────────────────
        public async Task<int> AddAsync(LostPetReport report)
        {
            const string sql = @"
                INSERT INTO LostPetReport
                    (IdUser, IdPet, PetName, AnimalType, Breed, Color,
                     LastSeenLocation, City, Area, Latitude, Longitude,
                     DateLost, TimeLost, Description, SpecialMarks, MicrochipId,
                     SecretQuestion, SecretAnswerHash, RewardAmount, RewardStatus,
                     Status, ExpiryDate, ReportCode, CreatedAt)
                VALUES
                    (@IdUser, @IdPet, @PetName, @AnimalType, @Breed, @Color,
                     @LastSeenLocation, @City, @Area, @Latitude, @Longitude,
                     @DateLost, @TimeLost, @Description, @SpecialMarks, @MicrochipId,
                     @SecretQuestion, @SecretAnswerHash, @RewardAmount, @RewardStatus,
                     @Status, @ExpiryDate, @ReportCode, @CreatedAt);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using var conn = new SqlConnection(_connectionString);
            return await conn.ExecuteScalarAsync<int>(sql, report);
        }

        // ── Update report ────────────────────────────────────
        public async Task UpdateAsync(LostPetReport report)
        {
            const string sql = @"
                UPDATE LostPetReport SET
                    LastSeenLocation = @LastSeenLocation,
                    City             = @City,
                    Area             = @Area,
                    Description      = @Description,
                    SpecialMarks     = @SpecialMarks,
                    RewardAmount     = @RewardAmount,
                    ExpiryDate       = @ExpiryDate
                WHERE IdLostReport = @IdLostReport";

            using var conn = new SqlConnection(_connectionString);
            await conn.ExecuteAsync(sql, report);
        }

        // ── Update Status only ───────────────────────────────
        public async Task UpdateStatusAsync(int idLostReport, string status)
        {
            const string sql = @"
                UPDATE LostPetReport
                SET Status = @Status
                WHERE IdLostReport = @IdLostReport";

            using var conn = new SqlConnection(_connectionString);
            await conn.ExecuteAsync(sql, new { IdLostReport = idLostReport, Status = status });
        }

        // ── Delete ───────────────────────────────────────────
        public async Task DeleteAsync(int idLostReport)
        {
            const string sql = "DELETE FROM LostPetReport WHERE IdLostReport = @IdLostReport";
            using var conn = new SqlConnection(_connectionString);
            await conn.ExecuteAsync(sql, new { IdLostReport = idLostReport });
        }

        // ── Add image ────────────────────────────────────────
        public async Task AddImageAsync(int idLostReport, string imageUrl)
        {
            const string sql = @"
                INSERT INTO LostPetImage (IdLostReport, ImageUrl)
                VALUES (@IdLostReport, @ImageUrl)";

            using var conn = new SqlConnection(_connectionString);
            await conn.ExecuteAsync(sql, new { IdLostReport = idLostReport, ImageUrl = imageUrl });
        }

        // ── Get images by report ─────────────────────────────
        public async Task<IEnumerable<LostPetImage>> GetImagesByReportIdAsync(int idLostReport)
        {
            const string sql = "SELECT * FROM LostPetImage WHERE IdLostReport = @IdLostReport";
            using var conn = new SqlConnection(_connectionString);
            return await conn.QueryAsync<LostPetImage>(sql, new { IdLostReport = idLostReport });
        }

        // ── Get first image ──────────────────────────────────
        public async Task<string?> GetFirstImageAsync(int idLostReport)
        {
            const string sql = @"
                SELECT TOP 1 ImageUrl FROM LostPetImage
                WHERE IdLostReport = @IdLostReport";

            using var conn = new SqlConnection(_connectionString);
            return await conn.QueryFirstOrDefaultAsync<string>(sql, new { IdLostReport = idLostReport });
        }

        public async Task<IEnumerable<LostPetReport>> GetActivePagedAsync(int page, int pageSize)
        {
            const string sql = @"
        SELECT lpr.*, u.Phone AS ContactPhone
        FROM LostPetReport lpr
        LEFT JOIN Users u ON lpr.IdUser = u.UserId
        WHERE lpr.Status = 'Active'
        ORDER BY lpr.CreatedAt DESC
        OFFSET @Offset ROWS
        FETCH NEXT @PageSize ROWS ONLY";

            using var conn = new SqlConnection(_connectionString);
            return await conn.QueryAsync<LostPetReport>(sql, new
            {
                Offset = (page - 1) * pageSize,
                PageSize = pageSize
            });
        }

        public async Task<int> GetActiveTotalCountAsync()
        {
            const string sql = @"
        SELECT COUNT(*) FROM LostPetReport
        WHERE Status = 'Active'";

            using var conn = new SqlConnection(_connectionString);
            return await conn.ExecuteScalarAsync<int>(sql);
        }

        public async Task CloseReportAsync(int reportId)
        {
            const string sql = @"
        UPDATE LostPetReport SET
            Status = 'Found'
        WHERE IdLostReport = @IdLostReport";

            using var conn = new SqlConnection(_connectionString);
            await conn.ExecuteAsync(sql, new { IdLostReport = reportId });
        }

        public async Task<IEnumerable<string>> GetAllImagesAsync(int reportId)
        {
            const string sql = @"
        SELECT ImageUrl
        FROM   LostPetImage
        WHERE  IdLostReport = @IdLostReport
          AND  ImageUrl IS NOT NULL
        ORDER BY Id ASC";

            using var conn = new SqlConnection(_connectionString);
            return await conn.QueryAsync<string>(sql, new { IdLostReport = reportId });
        }

    }
}
