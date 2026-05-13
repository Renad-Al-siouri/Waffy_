using Dapper;
using Microsoft.Data.SqlClient;
using Waffy.Interfaces;
using Waffy.Models;
using Waffy.Models.ViewModels;

namespace Waffy.Repository
{
    public class PostFlagRepository : IPostFlagRepository
    {
        private readonly string _connectionString;

        public PostFlagRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("WaffyConnection")
                ?? throw new Exception("WaffyConnection not found");
        }

        public async Task<bool> HasFlaggedAsync(int postId, int userId)
        {
            const string sql = @"
                SELECT COUNT(1) FROM PostFlag
                WHERE IdPost = @PostId AND IdUser = @UserId";
            using var conn = new SqlConnection(_connectionString);
            return await conn.ExecuteScalarAsync<int>(sql,
                new { PostId = postId, UserId = userId }) > 0;
        }

        public async Task AddFlagAsync(PostFlag flag)
        {
            const string sql = @"
                INSERT INTO PostFlag (IdPost, IdUser, Reason, CreatedAt)
                VALUES (@IdPost, @IdUser, @Reason, @CreatedAt)";
            using var conn = new SqlConnection(_connectionString);
            await conn.ExecuteAsync(sql, flag);
        }

        public async Task<IEnumerable<FlaggedPostDto>> GetAllFlaggedPostsAsync()
        {
            const string sql = @"
        SELECT 
            ap.IdPost,
            ap.PostedById,
            ap.PetName,
            ap.AnimalType,
            ap.PostedByType,
            ISNULL(u.FullName, 'Unknown') AS OwnerName,
            ap.City,
            ap.Status,
            COUNT(pf.IdFlag)  AS FlagCount,
            MAX(pf.Reason)    AS TopReason,
            MAX(pf.CreatedAt) AS LastFlagDate
        FROM PostFlag pf
        INNER JOIN AdoptionPost ap ON pf.IdPost  = ap.IdPost
        LEFT JOIN  Users u         ON ap.PostedById = u.UserId
        WHERE pf.IsReviewed = 0
        GROUP BY ap.IdPost, ap.PostedById, ap.PetName, ap.AnimalType,
                 ap.PostedByType, u.FullName, ap.City, ap.Status
        ORDER BY COUNT(pf.IdFlag) DESC";

            using var conn = new SqlConnection(_connectionString);
            return await conn.QueryAsync<FlaggedPostDto>(sql);
        }

        // أضيفي هذه الـ methods الجديدة:

        public async Task MarkAsReviewedAsync(
            int postId, int adminId, string action, string? note)
        {
            const string sql = @"
        UPDATE PostFlag
        SET IsReviewed        = 1,
            ReviewedAt        = GETUTCDATE(),
            ReviewedByAdminId = @AdminId,
            AdminAction       = @Action,
            AdminNote         = @Note
        WHERE IdPost = @PostId AND IsReviewed = 0";
            using var conn = new SqlConnection(_connectionString);
            await conn.ExecuteAsync(sql,
                new { PostId = postId, AdminId = adminId, Action = action, Note = note });
        }

        public async Task<IEnumerable<FlagDetailDto>> GetFlagsByPostIdAsync(int postId)
        {
            const string sql = @"
        SELECT 
            pf.IdFlag,
            pf.IdPost,
            pf.IdUser,
            ISNULL(u.FullName, 'Anonymous') AS ReporterName,
            ISNULL(u.Email,    '')           AS ReporterEmail,
            u.Phone                          AS ReporterPhone,
            pf.Reason,
            pf.CreatedAt,
            pf.IsReviewed,
            pf.ReviewedAt,
            pf.AdminAction,
            pf.AdminNote
        FROM PostFlag pf
        LEFT JOIN Users u ON pf.IdUser = u.UserId
        WHERE pf.IdPost = @PostId
        ORDER BY pf.CreatedAt DESC";
            using var conn = new SqlConnection(_connectionString);
            return await conn.QueryAsync<FlagDetailDto>(sql, new { PostId = postId });
        }

        public async Task<IEnumerable<FlaggedPostDto>> GetFlagHistoryAsync()
        {
            const string sql = @"
        SELECT 
            ap.IdPost,
            ap.PostedById,
            ap.PetName,
            ap.AnimalType,
            ap.PostedByType,
            ISNULL(u.FullName, 'Unknown') AS OwnerName,
            ap.City,
            ap.Status,
            COUNT(pf.IdFlag)  AS FlagCount,
            MAX(pf.Reason)    AS TopReason,
            MAX(pf.CreatedAt) AS LastFlagDate
        FROM PostFlag pf
        INNER JOIN AdoptionPost ap ON pf.IdPost  = ap.IdPost
        LEFT JOIN  Users u         ON ap.PostedById = u.UserId
        WHERE pf.IsReviewed = 1
        GROUP BY ap.IdPost, ap.PostedById, ap.PetName, ap.AnimalType,
                 ap.PostedByType, u.FullName, ap.City, ap.Status
        ORDER BY MAX(pf.ReviewedAt) DESC";
            using var conn = new SqlConnection(_connectionString);
            return await conn.QueryAsync<FlaggedPostDto>(sql);
        }

        public async Task ClearFlagsAsync(int postId)
        {
            const string sql = "DELETE FROM PostFlag WHERE IdPost = @PostId";
            using var conn = new SqlConnection(_connectionString);
            await conn.ExecuteAsync(sql, new { PostId = postId });
        }

        public async Task<Dictionary<int, int>> GetFlagCountsForPostsAsync(
            IEnumerable<int> postIds)
        {
            var ids = postIds.ToList();
            if (!ids.Any()) return new Dictionary<int, int>();

            const string sql = @"
                SELECT IdPost, COUNT(*) AS FlagCount
                FROM PostFlag
                WHERE IdPost IN @Ids
                GROUP BY IdPost";

            using var conn = new SqlConnection(_connectionString);
            var rows = await conn.QueryAsync<(int IdPost, int FlagCount)>(sql, new { Ids = ids });
            return rows.ToDictionary(r => r.IdPost, r => r.FlagCount);
        }

        public async Task<IEnumerable<PostFlag>> GetByUserIdAsync(int userId)
        {
            const string sql = @"
                SELECT * FROM PostFlag 
                WHERE IdUser = @UserId 
                ORDER BY CreatedAt DESC";
            using var conn = new SqlConnection(_connectionString);
            return await conn.QueryAsync<PostFlag>(sql, new { UserId = userId });
        }

        public async Task LogActionAsync(
    int postId, int adminId, string actionType,
    int? targetUserId, string? targetName, string? message)
        {
            const string sql = @"
        INSERT INTO PostFlagAction
            (IdPost, AdminId, ActionType, TargetUserId, TargetName, Message, CreatedAt)
        VALUES
            (@PostId, @AdminId, @ActionType, @TargetUserId, @TargetName, @Message, GETUTCDATE())";
            using var conn = new SqlConnection(_connectionString);
            await conn.ExecuteAsync(sql, new
            {
                PostId = postId,
                AdminId = adminId,
                ActionType = actionType,
                TargetUserId = targetUserId,
                TargetName = targetName,
                Message = message
            });
        }

        public async Task<IEnumerable<PostFlagActionDto>> GetActionsByPostIdAsync(int postId)
        {
            const string sql = @"
        SELECT * FROM PostFlagAction
        WHERE IdPost = @PostId
        ORDER BY CreatedAt ASC";
            using var conn = new SqlConnection(_connectionString);
            return await conn.QueryAsync<PostFlagActionDto>(sql, new { PostId = postId });
        }

        public async Task RevertReviewAsync(int postId)
        {
            const string sql = @"
        UPDATE PostFlag
        SET IsReviewed        = 0,
            ReviewedAt        = NULL,
            ReviewedByAdminId = NULL,
            AdminAction       = NULL,
            AdminNote         = NULL
        WHERE IdPost = @PostId";
            using var conn = new SqlConnection(_connectionString);
            await conn.ExecuteAsync(sql, new { PostId = postId });
        }

    }
}