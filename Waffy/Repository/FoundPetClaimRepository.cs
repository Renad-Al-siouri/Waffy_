using Dapper;
using Microsoft.Data.SqlClient;
using Waffy.Interfaces;
using Waffy.Models;

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

        public async Task<FoundPetClaim?> GetByIdAsync(int claimId)
        {
            const string sql = @"
                SELECT fc.*,
                       lpr.PetName,
                       lpr.RewardAmount,
                       lpr.IdUser AS OwnerId
                FROM FoundPetClaim fc
                INNER JOIN LostPetReport lpr ON fc.IdLostReport = lpr.IdLostReport
                WHERE fc.IdClaim = @ClaimId";

            using var conn = new SqlConnection(_connectionString);
            return await conn.QueryFirstOrDefaultAsync<FoundPetClaim>(sql, new { ClaimId = claimId });
        }

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
    }
}