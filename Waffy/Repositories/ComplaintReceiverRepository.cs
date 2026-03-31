namespace Waffy.Repositories
{
    using Dapper;
    using Microsoft.Data.SqlClient;
    using System.Data;
    using Waffy.Models;
    using Waffy.Models.Waffy.Models;

    public class ComplaintReceiverRepository
    {
        private readonly IConfiguration _config;

        public ComplaintReceiverRepository(IConfiguration config)
        {
            _config = config;
        }

        private IDbConnection CreateConnection()
        {
            return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
        }

        public async Task<IEnumerable<ComplaintReceiver>> GetAllAsync()
        {
            using var db = CreateConnection();
            return await db.QueryAsync<ComplaintReceiver>("SELECT * FROM ComplaintReceiver ORDER BY Id DESC");
        }

        public async Task<ComplaintReceiver?> GetByIdAsync(int id)
        {
            using var db = CreateConnection();
            return await db.QueryFirstOrDefaultAsync<ComplaintReceiver>("SELECT * FROM ComplaintReceiver WHERE Id = @Id", new { Id = id });
        }

        public async Task<int> CreateAsync(ComplaintReceiver item)
        {
            using var db = CreateConnection();
            string sql = @"
        INSERT INTO ComplaintReceiver
        (ComplaintId, ReceiverUserId, ReceiverRole, IsRead, ReadAt, CreatedAt)
        VALUES
        (@ComplaintId, @ReceiverUserId, @ReceiverRole, @IsRead, @ReadAt, GETDATE())";
            return await db.ExecuteAsync(sql, item);
        }

        public async Task<int> UpdateAsync(ComplaintReceiver item)
        {
            using var db = CreateConnection();
            string sql = @"
        UPDATE ComplaintReceiver SET
            ComplaintId = @ComplaintId,
            ReceiverUserId = @ReceiverUserId,
            ReceiverRole = @ReceiverRole,
            IsRead = @IsRead,
            ReadAt = @ReadAt
        WHERE Id = @Id";
            return await db.ExecuteAsync(sql, item);
        }

        public async Task<int> DeleteAsync(int id)
        {
            using var db = CreateConnection();
            return await db.ExecuteAsync("DELETE FROM ComplaintReceiver WHERE Id = @Id", new { Id = id });
        }
    }
}
