using Dapper;
using Microsoft.Data.SqlClient;
using Waffy.Interfaces;
using Waffy.Models;
using Waffy.Models.ViewModels;

namespace Waffy.Repository
{
    public class RewardLogRepository : IRewardLogRepository
    {
        private readonly string _connectionString;

        public RewardLogRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("WaffyConnection")
                ?? throw new Exception("WaffyConnection not found");
        }

        public async Task<int> AddAsync(RewardLog log)
        {
            const string sql = @"
                INSERT INTO RewardLog
                    (IdReward, IdRecipient, IdAdminGiver, IdLostReport, IdFoundClaim,
                     GiftType, ReasonCategory, ReasonNote, DeliveryStatus, CreatedAt)
                VALUES
                    (@IdReward, @IdRecipient, @IdAdminGiver, @IdLostReport, @IdFoundClaim,
                     @GiftType, @ReasonCategory, @ReasonNote, @DeliveryStatus, @CreatedAt);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using var conn = new SqlConnection(_connectionString);
            return await conn.ExecuteScalarAsync<int>(sql, log);
        }

        public async Task<IEnumerable<RewardLogDto>> GetAllAsync()
        {
            const string sql = @"
        SELECT
            rl.IdRewardLog,
            rl.IdReward,
            rl.IdRecipient,
            rl.IdAdminGiver,
            rl.IdLostReport,
            rl.IdFoundClaim,
            rl.GiftType,
            rl.ReasonCategory,
            rl.ReasonNote,
            rl.DeliveryStatus,
            rl.CreatedAt,
            -- Recipient
            ISNULL(ru.FullName, 'Unknown') AS RecipientName,
            ISNULL(ru.Email,    '')        AS RecipientEmail,
            -- Admin
            ISNULL(au.FullName, 'Admin')   AS AdminName,
            -- Reward
            srp.Title           AS RewardTitle,
            srp.RewardType,
            srp.Value           AS RewardValue,
            srp.DiscountPercent,
            srp.CouponCode,
            srp.PartnerName
        FROM RewardLog rl
        LEFT JOIN Users ru  ON rl.IdRecipient  = ru.UserId
        LEFT JOIN Users au  ON rl.IdAdminGiver = au.UserId
        LEFT JOIN SurpriseRewardPool srp ON rl.IdReward = srp.IdReward
        ORDER BY rl.CreatedAt DESC";

            using var conn = new SqlConnection(_connectionString);
            return await conn.QueryAsync<RewardLogDto>(sql);
        }

        public async Task<IEnumerable<RewardLogDto>> GetByUserIdAsync(int userId)
        {
            const string sql = @"
                SELECT
                    rl.IdRewardLog, rl.GiftType, rl.ReasonCategory,
                    rl.ReasonNote, rl.DeliveryStatus, rl.CreatedAt,
                    recipient.UserId  AS RecipientId,
                    recipient.FullName AS RecipientName,
                    recipient.Email    AS RecipientEmail,
                    ISNULL(recipient.Phone, '—') AS RecipientPhone,
                    ISNULL(recipient.City,  '—') AS RecipientCity,
                    admin.FullName     AS AdminName,
                    sp.Title           AS RewardTitle,
                    sp.RewardType,
                    sp.Value           AS RewardValue,
                    sp.DiscountPercent,
                    sp.CouponCode,
                    sp.PartnerName,
                    lpr.PetName
                FROM RewardLog rl
                JOIN Users           recipient ON rl.IdRecipient  = recipient.UserId
                JOIN Users           admin     ON rl.IdAdminGiver = admin.UserId
                JOIN SurpriseRewardPool sp     ON rl.IdReward     = sp.IdReward
                LEFT JOIN LostPetReport lpr    ON rl.IdLostReport = lpr.IdLostReport
                WHERE rl.IdRecipient = @UserId
                ORDER BY rl.CreatedAt DESC";

            using var conn = new SqlConnection(_connectionString);
            return await conn.QueryAsync<RewardLogDto>(sql, new { UserId = userId });
        }

        public async Task<bool> HasRecentRewardAsync(int userId, int days = 30)
        {
            const string sql = @"
                SELECT COUNT(*) FROM RewardLog
                WHERE IdRecipient = @UserId
                  AND CreatedAt >= DATEADD(DAY, -@Days, GETUTCDATE())";

            using var conn = new SqlConnection(_connectionString);
            return await conn.ExecuteScalarAsync<int>(sql,
                new { UserId = userId, Days = days }) > 0;
        }
        public async Task<HashSet<int>> GetRewardedClaimIdsAsync()
        {
            const string sql = @"
        SELECT DISTINCT IdFoundClaim
        FROM RewardLog
        WHERE IdFoundClaim IS NOT NULL";
            using var conn = new SqlConnection(_connectionString);
            var ids = await conn.QueryAsync<int>(sql);
            return ids.ToHashSet();
        }

    }
}
