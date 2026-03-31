namespace Waffy.Repositories
{
    using Dapper;
    using Microsoft.Data.SqlClient;
    using System.Data;
    using Waffy.Models;
    using Waffy.Models.Waffy.Models;

    public class ComplaintReplyRepository
    {
        private readonly IConfiguration _config;

        public ComplaintReplyRepository(IConfiguration config)
        {
            _config = config;
        }

        private IDbConnection CreateConnection()
        {
            return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
        }

        public async Task<IEnumerable<ComplaintReply>> GetAllAsync()
        {
            using var db = CreateConnection();
            return await db.QueryAsync<ComplaintReply>("SELECT * FROM ComplaintReply ORDER BY Id DESC");
        }

        public async Task<ComplaintReply?> GetByIdAsync(int id)
        {
            using var db = CreateConnection();
            return await db.QueryFirstOrDefaultAsync<ComplaintReply>("SELECT * FROM ComplaintReply WHERE Id = @Id", new { Id = id });
        }

        public async Task<int> CreateAsync(ComplaintReply item)
        {
            using var db = CreateConnection();
            string sql = @"
        INSERT INTO ComplaintReply (ComplaintId, AdminId, Message, CreatedAt)
        VALUES (@ComplaintId, @AdminId, @Message, GETDATE())";
            return await db.ExecuteAsync(sql, item);
        }

        public async Task<int> UpdateAsync(ComplaintReply item)
        {
            using var db = CreateConnection();
            string sql = @"
        UPDATE ComplaintReply SET
            ComplaintId = @ComplaintId,
            AdminId = @AdminId,
            Message = @Message
        WHERE Id = @Id";
            return await db.ExecuteAsync(sql, item);
        }

        public async Task<int> DeleteAsync(int id)
        {
            using var db = CreateConnection();
            return await db.ExecuteAsync("DELETE FROM ComplaintReply WHERE Id = @Id", new { Id = id });
        }
    }
}
