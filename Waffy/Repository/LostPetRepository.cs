using Dapper;
using Microsoft.Data.SqlClient;
using Waffy.Interfaces;
using Waffy.Models;
using Waffy.Models.ViewModels;

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
             Status, ExpiryDate, ReportCode, CreatedAt,
             IsPaid, HasSurpriseReward, IsFreePost, AmountPaid, RewardReserved)
        VALUES
            (@IdUser, @IdPet, @PetName, @AnimalType, @Breed, @Color,
             @LastSeenLocation, @City, @Area, @Latitude, @Longitude,
             @DateLost, @TimeLost, @Description, @SpecialMarks, @MicrochipId,
             @SecretQuestion, @SecretAnswerHash, @RewardAmount, @RewardStatus,
             @Status, @ExpiryDate, @ReportCode, @CreatedAt,
             @IsPaid, @HasSurpriseReward, @IsFreePost, @AmountPaid, @RewardReserved);
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

        public async Task<IEnumerable<LostPetAdminDto>> GetAllAsync()
        {
            const string sql = @"
        SELECT
            lpr.IdLostReport, lpr.IdUser, lpr.PetName, lpr.AnimalType,
            lpr.Breed, lpr.Color, lpr.City, lpr.Area, lpr.Status,
            lpr.RewardAmount, lpr.ReportCode, lpr.CreatedAt, lpr.ExpiryDate,
            lpr.SecretQuestion, lpr.IsPaid, lpr.HasSurpriseReward, lpr.AmountPaid, lpr.IsFreePost,lpr.RewardReserved,
            ISNULL(u.FullName, 'Unknown') AS OwnerName,
            ISNULL(u.Phone,    '')        AS OwnerPhone,
             (SELECT COUNT(*) FROM FoundPetClaim fc
     WHERE fc.IdLostReport = lpr.IdLostReport) AS ClaimCount,
    (SELECT COUNT(*) FROM FoundPetClaim fc
     WHERE fc.IdLostReport = lpr.IdLostReport
       AND fc.Status = 'Confirmed') AS ConfirmedClaimCount
        FROM LostPetReport lpr
        LEFT JOIN Users u ON lpr.IdUser = u.UserId
        ORDER BY lpr.CreatedAt DESC";
            using var conn = new SqlConnection(_connectionString);
            return await conn.QueryAsync<LostPetAdminDto>(sql);
        }

        public async Task<int> GetUserPostCountAsync(int userId)
        {
            const string sql = @"
        SELECT COUNT(*) FROM LostPetReport
        WHERE IdUser = @UserId";
            using var conn = new SqlConnection(_connectionString);
            return await conn.ExecuteScalarAsync<int>(sql, new { UserId = userId });
        }

        public async Task ExpireOldReportsAsync()
        {
            const string sql = @"
        UPDATE LostPetReport
        SET Status = 'Expired'
        WHERE Status = 'Active'
        AND ExpiryDate IS NOT NULL
        AND ExpiryDate < GETUTCDATE()";
            using var conn = new SqlConnection(_connectionString);
            await conn.ExecuteAsync(sql);
        }

        public async Task<IEnumerable<LostPetReport>> GetExpiringSoonAsync()
        {
            const string sql = @"
        SELECT * FROM LostPetReport
        WHERE Status = 'Active'
        AND ExpiryDate IS NOT NULL
        AND ExpiryDate BETWEEN GETUTCDATE() AND DATEADD(DAY, 3, GETUTCDATE())
        AND ExpiryNotifiedAt IS NULL";
            using var conn = new SqlConnection(_connectionString);
            return await conn.QueryAsync<LostPetReport>(sql);
        }

        public async Task MarkExpiryNotifiedAsync(int reportId)
        {
            const string sql = @"
        UPDATE LostPetReport
        SET ExpiryNotifiedAt = GETUTCDATE()
        WHERE IdLostReport = @IdLostReport";
            using var conn = new SqlConnection(_connectionString);
            await conn.ExecuteAsync(sql, new { IdLostReport = reportId });
        }

        public async Task UpdatePaymentStatusAsync(int reportId, bool isPaid, int paymentId)
        {
            const string sql = @"
        UPDATE LostPetReport
        SET IsPaid = @IsPaid, PaymentId = @PaymentId,
            Status = CASE WHEN @IsPaid = 1 THEN 'Active' ELSE Status END
        WHERE IdLostReport = @ReportId";
            using var conn = new SqlConnection(_connectionString);
            await conn.ExecuteAsync(sql, new { ReportId = reportId, IsPaid = isPaid, PaymentId = paymentId });
        }
        public async Task<int> GetFreePostsCountAsync(int userId)
{
    const string sql = @"
        SELECT COUNT(*) FROM LostPetReport
        WHERE IdUser = @UserId AND IsFreePost = 1";
    using var conn = new SqlConnection(_connectionString);
    return await conn.ExecuteScalarAsync<int>(sql, new { UserId = userId });
}

public async Task<int> GetFreeNoRewardPostsCountAsync(int userId)
{
    const string sql = @"
        SELECT COUNT(*) FROM LostPetReport
        WHERE IdUser     = @UserId
          AND IsFreePost = 1
          AND (RewardAmount IS NULL OR RewardAmount = 0)
          AND HasSurpriseReward = 0";
    using var conn = new SqlConnection(_connectionString);
    return await conn.ExecuteScalarAsync<int>(sql, new { UserId = userId });
}

public async Task<bool> CanRenewAsync(int reportId)
{
    const string sql = @"
        SELECT COUNT(*) FROM LostPetReport
        WHERE IdLostReport   = @IdLostReport
          AND HasBeenRenewed = 0
          AND Status IN ('Expired', 'Active')";
    using var conn = new SqlConnection(_connectionString);
    return await conn.ExecuteScalarAsync<int>(
        sql, new { IdLostReport = reportId }) > 0;
}

public async Task RenewReportAsync(int reportId, DateTime newExpiryDate)
{
    const string sql = @"
        UPDATE LostPetReport SET
            Status          = 'Active',
            HasBeenRenewed  = 1,
            ExpiryDate      = @NewExpiryDate,
            ExpiryNotifiedAt = NULL
        WHERE IdLostReport = @IdLostReport";
    using var conn = new SqlConnection(_connectionString);
    await conn.ExecuteAsync(sql,
        new { IdLostReport = reportId, NewExpiryDate = newExpiryDate });
}

public async Task<bool> HasRenewedExpiredPostAsync(int userId)
{
    const string sql = @"
        SELECT COUNT(*) FROM LostPetReport
        WHERE IdUser        = @UserId
          AND HasBeenRenewed = 1
          AND Status IN ('Expired', 'Found', 'Closed')";
    using var conn = new SqlConnection(_connectionString);
    return await conn.ExecuteScalarAsync<int>(sql, new { UserId = userId }) > 0;
}


        public async Task UpdateAmountPaidAsync(int reportId, decimal amount)
        {
            const string sql = @"
        UPDATE LostPetReport
        SET AmountPaid = @Amount
        WHERE IdLostReport = @ReportId";
            using var conn = new SqlConnection(_connectionString);
            await conn.ExecuteAsync(sql, new { ReportId = reportId, Amount = amount });
        }
        public async Task MarkRewardReservedAsync(int reportId)
        {
            const string sql = @"
        UPDATE LostPetReport
        SET RewardReserved = 1
        WHERE IdLostReport = @IdLostReport";
            using var conn = new SqlConnection(_connectionString);
            await conn.ExecuteAsync(sql, new { IdLostReport = reportId });
        }

        public async Task UpdateRewardStatusAsync(int reportId, string status)
        {
            const string sql = @"
        UPDATE LostPetReport
        SET RewardStatus = @Status
        WHERE IdLostReport = @IdLostReport";
            using var conn = new SqlConnection(_connectionString);
            await conn.ExecuteAsync(sql, new { IdLostReport = reportId, Status = status });
        }

    }
}
