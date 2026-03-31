namespace Waffy.Repositories
{
    using Dapper;
    using Microsoft.Data.SqlClient;
    using System.Data;
    using Waffy.Models;
    using Waffy.Models.Waffy.Models;

    public class AdoptionImageRepository
    {
        private readonly IConfiguration _config;

        public AdoptionImageRepository(IConfiguration config)
        {
            _config = config;
        }

        private IDbConnection CreateConnection()
        {
            return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
        }

        public async Task<IEnumerable<AdoptionImage>> GetAllAsync()
        {
            using var db = CreateConnection();
            return await db.QueryAsync<AdoptionImage>("SELECT * FROM AdoptionImage ORDER BY Id DESC");
        }

        public async Task<AdoptionImage?> GetByIdAsync(int id)
        {
            using var db = CreateConnection();
            return await db.QueryFirstOrDefaultAsync<AdoptionImage>(
                "SELECT * FROM AdoptionImage WHERE Id = @Id", new { Id = id });
        }

        public async Task<int> CreateAsync(AdoptionImage item)
        {
            using var db = CreateConnection();
            string sql = @"INSERT INTO AdoptionImage (PostId, ImageUrl) VALUES (@PostId, @ImageUrl)";
            return await db.ExecuteAsync(sql, item);
        }

        public async Task<int> UpdateAsync(AdoptionImage item)
        {
            using var db = CreateConnection();
            string sql = @"UPDATE AdoptionImage SET PostId = @PostId, ImageUrl = @ImageUrl WHERE Id = @Id";
            return await db.ExecuteAsync(sql, item);
        }

        public async Task<int> DeleteAsync(int id)
        {
            using var db = CreateConnection();
            return await db.ExecuteAsync("DELETE FROM AdoptionImage WHERE Id = @Id", new { Id = id });
        }
    }
}
