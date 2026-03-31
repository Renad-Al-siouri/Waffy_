namespace Waffy.Repositories
{
    using Dapper;
    using Microsoft.Data.SqlClient;
    using System.Data;
    using Waffy.Models;
    using Waffy.Models.Waffy.Models;

    public class ReportActionLogRepository
    {
        private readonly IConfiguration _config;

        public ReportActionLogRepository(IConfiguration config)
        {
            _config = config;
        }

        private IDbConnection CreateConnection()
        {
            return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
        }

        public async Task<IEnumerable<ReportActionLog>> GetAllAsync()
        {
            using var db = CreateConnection();
            return await db.QueryAsync<ReportActionLog>("SELECT * FROM ReportActionLog ORDER BY Id DESC");
        }

        public async Task<ReportActionLog?> GetByIdAsync(int id)
        {
            using var db = CreateConnection();
            return await db.QueryFirstOrDefaultAsync<ReportActionLog>(
                "SELECT * FROM ReportActionLog WHERE Id = @Id", new { Id = id });
        }

        public async Task<int> CreateAsync(ReportActionLog item)
        {
            using var db = CreateConnection();
            string sql = @"
        INSERT INTO ReportActionLog
        (EntityType, EntityId, ActionType, StatusAfterAction, Note, AdminId, CreatedAt)
        VALUES
        (@EntityType, @EntityId, @ActionType, @StatusAfterAction, @Note, @AdminId, GETDATE())";
            return await db.ExecuteAsync(sql, item);
        }

        public async Task<int> UpdateAsync(ReportActionLog item)
        {
            using var db = CreateConnection();
            string sql = @"
        UPDATE ReportActionLog SET
            EntityType = @EntityType,
            EntityId = @EntityId,
            ActionType = @ActionType,
            StatusAfterAction = @StatusAfterAction,
            Note = @Note,
            AdminId = @AdminId
        WHERE Id = @Id";
            return await db.ExecuteAsync(sql, item);
        }

        public async Task<int> DeleteAsync(int id)
        {
            using var db = CreateConnection();
            return await db.ExecuteAsync("DELETE FROM ReportActionLog WHERE Id = @Id", new { Id = id });
        }
    }
}
