namespace Waffy.Repositories
{
    using Dapper;
    using Microsoft.Data.SqlClient;
    using System.Data;
    using Waffy.Models;
    using Waffy.Models.Waffy.Models;

    public class LostPetImageRepository
    {
        private readonly IConfiguration _config;

        public LostPetImageRepository(IConfiguration config)
        {
            _config = config;
        }

        private IDbConnection CreateConnection()
        {
            return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
        }

        public async Task<IEnumerable<LostPetImage>> GetAllAsync()
        {
            using var db = CreateConnection();
            return await db.QueryAsync<LostPetImage>("SELECT * FROM LostPetImage ORDER BY Id DESC");
        }

        public async Task<LostPetImage?> GetByIdAsync(int id)
        {
            using var db = CreateConnection();
            return await db.QueryFirstOrDefaultAsync<LostPetImage>(
                "SELECT * FROM LostPetImage WHERE Id = @Id", new { Id = id });
        }

        public async Task<int> CreateAsync(LostPetImage item)
        {
            using var db = CreateConnection();
            string sql = @"INSERT INTO LostPetImage (ReportId, ImageUrl) VALUES (@ReportId, @ImageUrl)";
            return await db.ExecuteAsync(sql, item);
        }

        public async Task<int> UpdateAsync(LostPetImage item)
        {
            using var db = CreateConnection();
            string sql = @"UPDATE LostPetImage SET ReportId = @ReportId, ImageUrl = @ImageUrl WHERE Id = @Id";
            return await db.ExecuteAsync(sql, item);
        }

        public async Task<int> DeleteAsync(int id)
        {
            using var db = CreateConnection();
            return await db.ExecuteAsync("DELETE FROM LostPetImage WHERE Id = @Id", new { Id = id });
        }
    }
}
