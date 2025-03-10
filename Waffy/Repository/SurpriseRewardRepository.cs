using Dapper;
using Microsoft.Data.SqlClient;
using Waffy.Interfaces;
using Waffy.Models;

namespace Waffy.Repository
{
    public class SurpriseRewardRepository : ISurpriseRewardRepository
    {
        private readonly string _connectionString;

        public SurpriseRewardRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("WaffyConnection")
                ?? throw new Exception("WaffyConnection not found");
        }

        public async Task<SurpriseRewardPool?> GetAvailableAsync()
        {
            const string sql = @"
                SELECT TOP 1 * FROM SurpriseRewardPool
                WHERE IsUsed = 0
                AND (ExpiryDate IS NULL OR ExpiryDate > GETUTCDATE())
                ORDER BY CreatedAt ASC";
            using var conn = new SqlConnection(_connectionString);
            return await conn.QueryFirstOrDefaultAsync<SurpriseRewardPool>(sql);
        }

        public async Task MarkAsUsedAsync(int rewardId, int userId)
        {
            const string sql = @"
                UPDATE SurpriseRewardPool
                SET IsUsed = 1, UsedByUserId = @UserId, UsedAt = GETUTCDATE()
                WHERE IdReward = @RewardId";
            using var conn = new SqlConnection(_connectionString);
            await conn.ExecuteAsync(sql, new { RewardId = rewardId, UserId = userId });
        }

        public async Task AddAsync(SurpriseRewardPool reward)
        {
            const string sql = @"
                INSERT INTO SurpriseRewardPool
                    (RewardType, Title, Description, Value, DiscountPercent,
                     PartnerName, CouponCode, ExpiryDate, CreatedAt)
                VALUES
                    (@RewardType, @Title, @Description, @Value, @DiscountPercent,
                     @PartnerName, @CouponCode, @ExpiryDate, GETUTCDATE())";
            using var conn = new SqlConnection(_connectionString);
            await conn.ExecuteAsync(sql, reward);
        }

        public async Task<IEnumerable<SurpriseRewardPool>> GetAllAsync()
        {
            const string sql = "SELECT * FROM SurpriseRewardPool ORDER BY CreatedAt DESC";
            using var conn = new SqlConnection(_connectionString);
            return await conn.QueryAsync<SurpriseRewardPool>(sql);
        }

        public async Task DeleteAsync(int rewardId)
        {
            const string sql = "DELETE FROM SurpriseRewardPool WHERE IdReward = @Id";
            using var conn = new SqlConnection(_connectionString);
            await conn.ExecuteAsync(sql, new { Id = rewardId });
        }
        public async Task<SurpriseRewardPool?> GetByIdAsync(int rewardId)
        {
            const string sql = "SELECT * FROM SurpriseRewardPool WHERE IdReward = @Id";
            using var conn = new SqlConnection(_connectionString);
            return await conn.QueryFirstOrDefaultAsync<SurpriseRewardPool>(sql, new { Id = rewardId });
        }
        public async Task UpdateAsync(SurpriseRewardPool reward)
        {
            const string sql = @"
        UPDATE SurpriseRewardPool SET
            RewardType     = @RewardType,
            Title          = @Title,
            Value          = @Value,
            DiscountPercent= @DiscountPercent,
            PartnerName    = @PartnerName,
            CouponCode     = @CouponCode,
            ExpiryDate     = @ExpiryDate
        WHERE IdReward = @IdReward
          AND IsUsed   = 0"; 
    using var conn = new SqlConnection(_connectionString);
            await conn.ExecuteAsync(sql, reward);
        }
    }
}