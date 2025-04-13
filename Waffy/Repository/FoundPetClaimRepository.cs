using Dapper;
using Microsoft.Data.SqlClient;
using Waffy.Interfaces;
using Waffy.Models;
using Waffy.Models.ViewModels;

namespace Waffy.Repository
{
    public class FoundPetClaimRepository : IFoundPetClaimRepository
    {
        private readonly string _connectionString;

        public FoundPetClaimRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("WaffyConnection")
                ?? throw new Exception("WaffyConnection not found");
        }

        public async Task<int> AddAsync(FoundPetClaim claim)
        {
            const string sql = @"
                INSERT INTO FoundPetClaim
                    (IdLostReport, IdUser, FinderName, FinderPhone,
                     Description, IsVerified, Status, CreatedAt)
                VALUES
                    (@IdLostReport, @IdUser, @FinderName, @FinderPhone,
                     @Description, @IsVerified, @Status, @CreatedAt);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using var conn = new SqlConnection(_connectionString);
            return await conn.ExecuteScalarAsync<int>(sql, claim);
        }

        // ✅ جلب كل الـ claims لتقرير معين — Verified أولاً
        public async Task<IEnumerable<FoundPetClaim>> GetByReportIdAsync(int reportId)
        {
            const string sql = @"
                SELECT fc.*,
                       lpr.PetName,
                       lpr.RewardAmount,
                       lpr.IdUser AS OwnerId
                FROM FoundPetClaim fc
                INNER JOIN LostPetReport lpr ON fc.IdLostReport = lpr.IdLostReport
                WHERE fc.IdLostReport = @ReportId
                ORDER BY fc.IsVerified DESC, fc.CreatedAt ASC";

            using var conn = new SqlConnection(_connectionString);
            return await conn.QueryAsync<FoundPetClaim>(sql, new { ReportId = reportId });
        }

        // ✅ جلب كل الـ claims التي قدّمها user معين (لصفحة التتبع)
        public async Task<IEnumerable<FoundPetClaim>> GetByUserIdAsync(int userId)
        {
            const string sql = @"
                SELECT fc.*,
                       lpr.PetName,
                       lpr.RewardAmount,
                       lpr.IdUser AS OwnerId
                FROM FoundPetClaim fc
                INNER JOIN LostPetReport lpr ON fc.IdLostReport = lpr.IdLostReport
                WHERE fc.IdUser = @UserId
                ORDER BY fc.CreatedAt DESC";

            using var conn = new SqlConnection(_connectionString);
            return await conn.QueryAsync<FoundPetClaim>(sql, new { UserId = userId });
        }

        // ── جلب claim بالـ ID ──────────────────────────────────
        public async Task<FoundPetClaim?> GetByIdAsync(int claimId)
        {
            const string sql = @"
                SELECT fc.*,
                       lpr.PetName, lpr.RewardAmount, lpr.RewardStatus,
                       lpr.RewardReserved, lpr.IdUser AS OwnerId
                FROM FoundPetClaim fc
                LEFT JOIN LostPetReport lpr ON fc.IdLostReport = lpr.IdLostReport
                WHERE fc.IdClaim = @ClaimId";   // ✅ IdClaim (fixed from IdFoundClaim)
            using var conn = new SqlConnection(_connectionString);
            return await conn.QueryFirstOrDefaultAsync<FoundPetClaim>(sql, new { ClaimId = claimId });
        }

        // ── القديمة (تستخدمها SuperAdminController) ────────────
        public async Task ConfirmAsync(int claimId)
        {
            const string sql = @"
                UPDATE FoundPetClaim SET Status = 'Confirmed'
                WHERE IdClaim = @ClaimId";
            using var conn = new SqlConnection(_connectionString);
            await conn.ExecuteAsync(sql, new { ClaimId = claimId });
        }

        public async Task RejectAsync(int claimId)
        {
            const string sql = @"
                UPDATE FoundPetClaim SET Status = 'Rejected'
                WHERE IdClaim = @ClaimId";
            using var conn = new SqlConnection(_connectionString);
            await conn.ExecuteAsync(sql, new { ClaimId = claimId });
        }

        // ── الجديدة (تستخدمها ProfileController) ────────────────
        public async Task ConfirmClaimAsync(int claimId)
        {
            const string sql = @"
                UPDATE FoundPetClaim
                SET Status    = 'Confirmed',
                    UpdatedAt = GETUTCDATE()
                WHERE IdClaim = @ClaimId";   // ✅ IdClaim (fixed from IdFoundClaim)
            using var conn = new SqlConnection(_connectionString);
            await conn.ExecuteAsync(sql, new { ClaimId = claimId });
        }

        public async Task RejectClaimAsync(int claimId)
        {
            const string sql = @"
                UPDATE FoundPetClaim
                SET Status    = 'Rejected',
                    UpdatedAt = GETUTCDATE()
                WHERE IdClaim = @ClaimId";   // ✅ IdClaim (fixed from IdFoundClaim)
            using var conn = new SqlConnection(_connectionString);
            await conn.ExecuteAsync(sql, new { ClaimId = claimId });
        }

        public async Task<IEnumerable<FoundClaimAdminDto>> GetAllWithDetailsAsync()
        {
            const string sql = @"
                SELECT
                    fc.IdClaim, fc.IdLostReport, fc.IdUser,
                    fc.FinderName, fc.FinderPhone, fc.IsVerified,
                    fc.Status, fc.CreatedAt,
                    lpr.PetName, lpr.AnimalType, lpr.City,
                    lpr.RewardAmount, lpr.RewardStatus,
                    ISNULL(ou.FullName, 'Unknown') AS OwnerName,
                    ISNULL(ou.Phone,    '')        AS OwnerPhone,
                    fu.Email                       AS FinderEmail
                FROM FoundPetClaim fc
                INNER JOIN LostPetReport lpr ON fc.IdLostReport = lpr.IdLostReport
                LEFT JOIN  Users ou ON lpr.IdUser = ou.UserId
                LEFT JOIN  Users fu ON fc.IdUser  = fu.UserId
                ORDER BY fc.CreatedAt DESC";
            using var conn = new SqlConnection(_connectionString);
            return await conn.QueryAsync<FoundClaimAdminDto>(sql);
        }

        public async Task<IEnumerable<TopFinderDto>> GetTopFindersAsync()
        {
            const string sql = @"
        SELECT TOP 3
            COALESCE(fc.IdUser, -ABS(CHECKSUM(fc.FinderPhone))) AS IdUser,
            MAX(fc.FinderName)               AS FinderName,
            fc.FinderPhone,
            ISNULL(MAX(u.Email), '')         AS FinderEmail,
            COUNT(*)                          AS ConfirmedCount,
            ISNULL(SUM(lpr.RewardAmount), 0) AS TotalRewardEarned
        FROM FoundPetClaim fc
        INNER JOIN LostPetReport lpr ON fc.IdLostReport = lpr.IdLostReport
        LEFT JOIN  Users u ON fc.IdUser = u.UserId
        WHERE fc.Status = 'Confirmed'
        AND fc.CreatedAt >= DATEADD(DAY, -30, GETUTCDATE())
          AND MONTH(fc.CreatedAt) = MONTH(GETUTCDATE())
          AND YEAR(fc.CreatedAt)  = YEAR(GETUTCDATE())
        GROUP BY COALESCE(fc.IdUser, -ABS(CHECKSUM(fc.FinderPhone))),
                 fc.FinderPhone
        ORDER BY COUNT(*) DESC";

            using var conn = new SqlConnection(_connectionString);
            return await conn.QueryAsync<TopFinderDto>(sql);
        }

        public async Task CancelRewardAsync(int claimId)
        {
            const string sql = @"
                UPDATE FoundPetClaim SET Status = 'Cancelled'
                WHERE IdClaim = @ClaimId";
            using var conn = new SqlConnection(_connectionString);
            await conn.ExecuteAsync(sql, new { ClaimId = claimId });
        }
    }
}