// ══════════════════════════════════════════════════════════
// Repository/ReviewRepository.cs
// ══════════════════════════════════════════════════════════
using Dapper;
using Microsoft.Data.SqlClient;
using Waffy.Interfaces;
using Waffy.Models;

namespace Waffy.Repository
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly string _connectionString;

        public ReviewRepository(IConfiguration config)
            => _connectionString = config.GetConnectionString("WaffyConnection")!;

        // ── جلب reviews لهدف معين ─────────────────────────
        public async Task<IEnumerable<Review>> GetByTargetAsync(
            string targetType, int targetId)
        {
            const string sql = @"
                SELECT  r.IdReview, r.IdUser, r.TargetType, r.TargetId,
                        r.Rating, r.Title, r.Comment, r.Status, r.CreatedAt,
                        u.FullName     AS UserFullName,
                        u.ProfileImage AS UserProfileImage
                FROM    Review r
                JOIN    Users  u ON u.UserId = r.IdUser
                WHERE   r.TargetType = @TargetType
                  AND   r.TargetId   = @TargetId
                  AND   r.Status     = 'Active'
                ORDER BY r.CreatedAt DESC";

            using var conn = new SqlConnection(_connectionString);
            return await conn.QueryAsync<Review>(sql,
                new { TargetType = targetType, TargetId = targetId });
        }

        // ── هل قيّم مسبقاً؟ ─────────────────────────────
        public async Task<bool> HasReviewedAsync(
            int userId, string targetType, int targetId)
        {
            const string sql = @"
                SELECT COUNT(1) FROM Review
                WHERE  IdUser     = @UserId
                  AND  TargetType = @TargetType
                  AND  TargetId   = @TargetId";

            using var conn = new SqlConnection(_connectionString);
            var count = await conn.ExecuteScalarAsync<int>(sql,
                new { UserId = userId, TargetType = targetType, TargetId = targetId });
            return count > 0;
        }

        // ── جلب review بالـ Id ──────────────────────────
        public async Task<Review?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT r.*, u.FullName     AS UserFullName,
                            u.ProfileImage AS UserProfileImage
                FROM   Review r
                JOIN   Users  u ON u.UserId = r.IdUser
                WHERE  r.IdReview = @Id";

            using var conn = new SqlConnection(_connectionString);
            return await conn.QueryFirstOrDefaultAsync<Review>(sql, new { Id = id });
        }

        // ── إضافة review + تحديث Rating ─────────────────
        public async Task<int> AddAsync(Review review)
        {
            const string sql = @"
                INSERT INTO Review (IdUser, TargetType, TargetId, Rating,
                                    Title, Comment, Status, CreatedAt)
                VALUES (@IdUser, @TargetType, @TargetId, @Rating,
                        @Title, @Comment, 'Active', @CreatedAt);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using var conn = new SqlConnection(_connectionString);
            var newId = await conn.ExecuteScalarAsync<int>(sql, review);

            // تحديث Rating في Clinic أو Doctor
            await conn.ExecuteAsync(
                "EXEC sp_UpdateTargetRating @TargetType, @TargetId",
                new { review.TargetType, review.TargetId });

            return newId;
        }

        // ── حذف (soft delete) + تحديث Rating ────────────
        public async Task DeleteAsync(int id)
        {
            const string getSql = @"
                SELECT TargetType, TargetId FROM Review WHERE IdReview = @Id";

            const string delSql = @"
                UPDATE Review SET Status = 'Deleted' WHERE IdReview = @Id";

            using var conn = new SqlConnection(_connectionString);
            var r = await conn.QueryFirstOrDefaultAsync<Review>(getSql, new { Id = id });
            if (r == null) return;

            await conn.ExecuteAsync(delSql, new { Id = id });

            // إعادة حساب Rating بعد الحذف
            await conn.ExecuteAsync(
                "EXEC sp_UpdateTargetRating @TargetType, @TargetId",
                new { r.TargetType, r.TargetId });
        }

        // ── جلب كل reviews (SuperAdmin) ──────────────────
        public async Task<IEnumerable<Review>> GetAllAsync(string? targetType = null)
        {
            var sql = @"
                SELECT  r.IdReview, r.IdUser, r.TargetType, r.TargetId,
                        r.Rating, r.Title, r.Comment, r.Status, r.CreatedAt,
                        u.FullName     AS UserFullName,
                        u.ProfileImage AS UserProfileImage
                FROM    Review r
                JOIN    Users  u ON u.UserId = r.IdUser
                WHERE   r.Status = 'Active'
                " + (targetType != null ? "AND r.TargetType = @TargetType" : "") + @"
                ORDER BY r.CreatedAt DESC";

            using var conn = new SqlConnection(_connectionString);
            return await conn.QueryAsync<Review>(sql,
                targetType != null ? new { TargetType = targetType } : null);
        }

        // ── عدد الـ reviews لهدف ─────────────────────────
        public async Task<int> CountByTargetAsync(string targetType, int targetId)
        {
            const string sql = @"
                SELECT COUNT(1) FROM Review
                WHERE  TargetType = @TargetType
                  AND  TargetId   = @TargetId
                  AND  Status     = 'Active'";

            using var conn = new SqlConnection(_connectionString);
            return await conn.ExecuteScalarAsync<int>(sql,
                new { TargetType = targetType, TargetId = targetId });
        }
    }
}