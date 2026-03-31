namespace Waffy.Repositories
{
    using Dapper;
    using Microsoft.Data.SqlClient;
    using System.Data;
    using Waffy.Models;
    using Waffy.Models.Waffy.Models;

    public class NotificationRepository
    {
        private readonly IConfiguration _config;

        public NotificationRepository(IConfiguration config)
        {
            _config = config;
        }

        private IDbConnection CreateConnection()
        {
            return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
        }

        public async Task<IEnumerable<Notification>> GetAllAsync()
        {
            using var db = CreateConnection();
            return await db.QueryAsync<Notification>("SELECT * FROM Notification ORDER BY Id DESC");
        }

        public async Task<Notification?> GetByIdAsync(int id)
        {
            using var db = CreateConnection();
            return await db.QueryFirstOrDefaultAsync<Notification>(
                "SELECT * FROM Notification WHERE Id = @Id", new { Id = id });
        }

        public async Task<int> CreateAsync(Notification item)
        {
            using var db = CreateConnection();
            string sql = @"
        INSERT INTO Notification
        (UserId, Type, Title, Message, IsRead, ReadAt, RelatedEntityType, RelatedEntityId, CreatedAt)
        VALUES
        (@UserId, @Type, @Title, @Message, @IsRead, @ReadAt, @RelatedEntityType, @RelatedEntityId, GETDATE())";
            return await db.ExecuteAsync(sql, item);
        }

        public async Task<int> UpdateAsync(Notification item)
        {
            using var db = CreateConnection();
            string sql = @"
        UPDATE Notification SET
            UserId = @UserId,
            Type = @Type,
            Title = @Title,
            Message = @Message,
            IsRead = @IsRead,
            ReadAt = @ReadAt,
            RelatedEntityType = @RelatedEntityType,
            RelatedEntityId = @RelatedEntityId
        WHERE Id = @Id";
            return await db.ExecuteAsync(sql, item);
        }

        public async Task<int> DeleteAsync(int id)
        {
            using var db = CreateConnection();
            return await db.ExecuteAsync("DELETE FROM Notification WHERE Id = @Id", new { Id = id });
        }
    }
}
