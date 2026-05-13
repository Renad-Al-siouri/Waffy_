using Dapper;
using Microsoft.Data.SqlClient;
using Waffy.Interfaces;
using Waffy.Models;

namespace Waffy.Repository
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly string _connectionString;

        public NotificationRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("WaffyConnection")
                ?? throw new Exception("WaffyConnection not found");
        }

        public async Task<IEnumerable<Notification>> GetByUserIdAsync(int userId)
        {
            const string sql = @"
                SELECT * FROM Notification
                WHERE IdUser = @UserId
                ORDER BY CreatedAt DESC";
            using var conn = new SqlConnection(_connectionString);
            return await conn.QueryAsync<Notification>(sql, new { UserId = userId });
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            const string sql = @"
                SELECT COUNT(*) FROM Notification
                WHERE IdUser = @UserId AND (IsRead = 0 OR IsRead IS NULL)";
            using var conn = new SqlConnection(_connectionString);
            return await conn.ExecuteScalarAsync<int>(sql, new { UserId = userId });
        }

        public async Task AddAsync(Notification notification)
        {
            const string sql = @"
                INSERT INTO Notification 
                    (Id, IdUser, Type, Title, Message, IsRead,
                     RelatedEntityType, RelatedEntityId, CreatedAt)
                VALUES (
                    ISNULL((SELECT MAX(Id) FROM Notification), 0) + 1,
                    @IdUser, @Type, @Title, @Message, 0,
                    @RelatedEntityType, @RelatedEntityId, @CreatedAt)";
            using var conn = new SqlConnection(_connectionString);
            await conn.ExecuteAsync(sql, notification);
        }

        public async Task MarkAsReadAsync(int notificationId)
        {
            const string sql = @"
                UPDATE Notification 
                SET IsRead = 1, ReadAt = GETUTCDATE()
                WHERE Id = @Id";
            using var conn = new SqlConnection(_connectionString);
            await conn.ExecuteAsync(sql, new { Id = notificationId });
        }

        public async Task MarkAllAsReadAsync(int userId)
        {
            const string sql = @"
                UPDATE Notification 
                SET IsRead = 1, ReadAt = GETUTCDATE()
                WHERE IdUser = @UserId AND (IsRead = 0 OR IsRead IS NULL)";
            using var conn = new SqlConnection(_connectionString);
            await conn.ExecuteAsync(sql, new { UserId = userId });
        }
    }
}