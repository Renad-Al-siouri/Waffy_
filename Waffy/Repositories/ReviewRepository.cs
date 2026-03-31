namespace Waffy.Repositories
{
    using Dapper;
    using Microsoft.Data.SqlClient;
    using System.Data;
    using Waffy.Models;
    using Waffy.Models.Waffy.Models;

    public class ReviewRepository
    {
        private readonly IConfiguration _config;

        public ReviewRepository(IConfiguration config)
        {
            _config = config;
        }

        private IDbConnection CreateConnection()
        {
            return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
        }

        public async Task<IEnumerable<Review>> GetAllAsync()
        {
            using var db = CreateConnection();
            return await db.QueryAsync<Review>("SELECT * FROM Review ORDER BY Id DESC");
        }

        public async Task<Review?> GetByIdAsync(int id)
        {
            using var db = CreateConnection();
            return await db.QueryFirstOrDefaultAsync<Review>(
                "SELECT * FROM Review WHERE Id = @Id", new { Id = id });
        }

        public async Task<int> CreateAsync(Review item)
        {
            using var db = CreateConnection();
            string sql = @"
        INSERT INTO Review
        (UserId, TargetType, TargetId, Rating, Comment, Status, UpdatedAt, CreatedAt)
        VALUES
        (@UserId, @TargetType, @TargetId, @Rating, @Comment, @Status, @UpdatedAt, GETDATE())";
            return await db.ExecuteAsync(sql, item);
        }

        public async Task<int> UpdateAsync(Review item)
        {
            using var db = CreateConnection();
            string sql = @"
        UPDATE Review SET
            UserId = @UserId,
            TargetType = @TargetType,
            TargetId = @TargetId,
            Rating = @Rating,
            Comment = @Comment,
            Status = @Status,
            UpdatedAt = @UpdatedAt
        WHERE Id = @Id";
            return await db.ExecuteAsync(sql, item);
        }

        public async Task<int> DeleteAsync(int id)
        {
            using var db = CreateConnection();
            return await db.ExecuteAsync("DELETE FROM Review WHERE Id = @Id", new { Id = id });
        }
    }
}
