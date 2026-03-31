namespace Waffy.Repositories
{
    using Dapper;
    using Microsoft.Data.SqlClient;
    using System.Data;
    using Waffy.Models;
    using Waffy.Models.Waffy.Models;

    public class ClaimAttemptLogRepository
    {
        private readonly IConfiguration _config;

        public ClaimAttemptLogRepository(IConfiguration config)
        {
            _config = config;
        }

        private IDbConnection CreateConnection()
        {
            return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
        }

        public async Task<IEnumerable<ClaimAttemptLog>> GetAllAsync()
        {
            using var db = CreateConnection();
            return await db.QueryAsync<ClaimAttemptLog>("SELECT * FROM ClaimAttemptLog ORDER BY Id DESC");
        }

        public async Task<ClaimAttemptLog?> GetByIdAsync(int id)
        {
            using var db = CreateConnection();
            return await db.QueryFirstOrDefaultAsync<ClaimAttemptLog>(
                "SELECT * FROM ClaimAttemptLog WHERE Id = @Id", new { Id = id });
        }

        public async Task<int> CreateAsync(ClaimAttemptLog item)
        {
            using var db = CreateConnection();
            string sql = @"
        INSERT INTO ClaimAttemptLog (ClaimId, AttemptValue, IsCorrect, IPAddress, CreatedAt)
        VALUES (@ClaimId, @AttemptValue, @IsCorrect, @IPAddress, GETDATE())";
            return await db.ExecuteAsync(sql, item);
        }

        public async Task<int> UpdateAsync(ClaimAttemptLog item)
        {
            using var db = CreateConnection();
            string sql = @"
        UPDATE ClaimAttemptLog SET
            ClaimId = @ClaimId,
            AttemptValue = @AttemptValue,
            IsCorrect = @IsCorrect,
            IPAddress = @IPAddress
        WHERE Id = @Id";
            return await db.ExecuteAsync(sql, item);
        }

        public async Task<int> DeleteAsync(int id)
        {
            using var db = CreateConnection();
            return await db.ExecuteAsync("DELETE FROM ClaimAttemptLog WHERE Id = @Id", new { Id = id });
        }
    }
}
