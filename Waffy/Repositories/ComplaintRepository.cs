namespace Waffy.Repositories
{
    using Dapper;
    using Microsoft.Data.SqlClient;
    using System.Data;
    using Waffy.Models;
    using Waffy.Models.Waffy.Models;

    public class ComplaintRepository
    {
        private readonly IConfiguration _config;

        public ComplaintRepository(IConfiguration config)
        {
            _config = config;
        }

        private IDbConnection CreateConnection()
        {
            return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
        }

        public async Task<IEnumerable<Complaint>> GetAllAsync()
        {
            using var db = CreateConnection();
            return await db.QueryAsync<Complaint>("SELECT * FROM Complaint ORDER BY Id DESC");
        }

        public async Task<Complaint?> GetByIdAsync(int id)
        {
            using var db = CreateConnection();
            return await db.QueryFirstOrDefaultAsync<Complaint>("SELECT * FROM Complaint WHERE Id = @Id", new { Id = id });
        }

        public async Task<int> CreateAsync(Complaint item)
        {
            using var db = CreateConnection();
            string sql = @"
        INSERT INTO Complaint
        (UserId, TargetType, TargetId, Title, Description, Status, Priority, ResolvedAt, ResolvedByAdminId, CreatedAt)
        VALUES
        (@UserId, @TargetType, @TargetId, @Title, @Description, @Status, @Priority, @ResolvedAt, @ResolvedByAdminId, GETDATE())";
            return await db.ExecuteAsync(sql, item);
        }

        public async Task<int> UpdateAsync(Complaint item)
        {
            using var db = CreateConnection();
            string sql = @"
        UPDATE Complaint SET
            UserId = @UserId,
            TargetType = @TargetType,
            TargetId = @TargetId,
            Title = @Title,
            Description = @Description,
            Status = @Status,
            Priority = @Priority,
            ResolvedAt = @ResolvedAt,
            ResolvedByAdminId = @ResolvedByAdminId
        WHERE Id = @Id";
            return await db.ExecuteAsync(sql, item);
        }

        public async Task<int> DeleteAsync(int id)
        {
            using var db = CreateConnection();
            return await db.ExecuteAsync("DELETE FROM Complaint WHERE Id = @Id", new { Id = id });
        }
    }
}
