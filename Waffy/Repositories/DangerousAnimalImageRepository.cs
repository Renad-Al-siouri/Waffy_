namespace Waffy.Repositories
{
    using Dapper;
    using Microsoft.Data.SqlClient;
    using System.Data;
    using Waffy.Models;
    using Waffy.Models.Waffy.Models;

    public class DangerousAnimalImageRepository
    {
        private readonly IConfiguration _config;

        public DangerousAnimalImageRepository(IConfiguration config)
        {
            _config = config;
        }

        private IDbConnection CreateConnection()
        {
            return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
        }

        public async Task<IEnumerable<DangerousAnimalImage>> GetAllAsync()
        {
            using var db = CreateConnection();
            return await db.QueryAsync<DangerousAnimalImage>("SELECT * FROM DangerousAnimalImage ORDER BY Id DESC");
        }

        public async Task<DangerousAnimalImage?> GetByIdAsync(int id)
        {
            using var db = CreateConnection();
            return await db.QueryFirstOrDefaultAsync<DangerousAnimalImage>(
                "SELECT * FROM DangerousAnimalImage WHERE Id = @Id", new { Id = id });
        }

        public async Task<int> CreateAsync(DangerousAnimalImage item)
        {
            using var db = CreateConnection();
            string sql = @"INSERT INTO DangerousAnimalImage (ReportId, ImageUrl) VALUES (@ReportId, @ImageUrl)";
            return await db.ExecuteAsync(sql, item);
        }

        public async Task<int> UpdateAsync(DangerousAnimalImage item)
        {
            using var db = CreateConnection();
            string sql = @"UPDATE DangerousAnimalImage SET ReportId = @ReportId, ImageUrl = @ImageUrl WHERE Id = @Id";
            return await db.ExecuteAsync(sql, item);
        }

        public async Task<int> DeleteAsync(int id)
        {
            using var db = CreateConnection();
            return await db.ExecuteAsync("DELETE FROM DangerousAnimalImage WHERE Id = @Id", new { Id = id });
        }
    }
}
