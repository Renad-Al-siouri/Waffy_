namespace Waffy.Repositories
{
    using Dapper;
    using Microsoft.Data.SqlClient;
    using System.Data;
    using Waffy.Models;
    using Waffy.Models.Waffy.Models;

    public class FoundPetImageRepository
    {
        private readonly IConfiguration _config;

        public FoundPetImageRepository(IConfiguration config)
        {
            _config = config;
        }

        private IDbConnection CreateConnection()
        {
            return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
        }

        public async Task<IEnumerable<FoundPetImage>> GetAllAsync()
        {
            using var db = CreateConnection();
            return await db.QueryAsync<FoundPetImage>("SELECT * FROM FoundPetImage ORDER BY Id DESC");
        }

        public async Task<FoundPetImage?> GetByIdAsync(int id)
        {
            using var db = CreateConnection();
            return await db.QueryFirstOrDefaultAsync<FoundPetImage>(
                "SELECT * FROM FoundPetImage WHERE Id = @Id", new { Id = id });
        }

        public async Task<int> CreateAsync(FoundPetImage item)
        {
            using var db = CreateConnection();
            string sql = @"INSERT INTO FoundPetImage (ClaimId, ImageUrl) VALUES (@ClaimId, @ImageUrl)";
            return await db.ExecuteAsync(sql, item);
        }

        public async Task<int> UpdateAsync(FoundPetImage item)
        {
            using var db = CreateConnection();
            string sql = @"UPDATE FoundPetImage SET ClaimId = @ClaimId, ImageUrl = @ImageUrl WHERE Id = @Id";
            return await db.ExecuteAsync(sql, item);
        }

        public async Task<int> DeleteAsync(int id)
        {
            using var db = CreateConnection();
            return await db.ExecuteAsync("DELETE FROM FoundPetImage WHERE Id = @Id", new { Id = id });
        }
    }
}
