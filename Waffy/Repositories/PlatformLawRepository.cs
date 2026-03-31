namespace Waffy.Repositories
{
    using Dapper;
    using Microsoft.Data.SqlClient;
    using System.Data;
    using Waffy.Models;
    using Waffy.Models.Waffy.Models;

    public class PlatformLawRepository
    {
        private readonly IConfiguration _config;

        public PlatformLawRepository(IConfiguration config)
        {
            _config = config;
        }

        private IDbConnection CreateConnection()
        {
            return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
        }

        public async Task<IEnumerable<PlatformLaw>> GetAllAsync()
        {
            using var db = CreateConnection();
            return await db.QueryAsync<PlatformLaw>("SELECT * FROM PlatformLaw ORDER BY Id DESC");
        }

        public async Task<PlatformLaw?> GetByIdAsync(int id)
        {
            using var db = CreateConnection();
            return await db.QueryFirstOrDefaultAsync<PlatformLaw>(
                "SELECT * FROM PlatformLaw WHERE Id = @Id", new { Id = id });
        }

        public async Task<int> CreateAsync(PlatformLaw item)
        {
            using var db = CreateConnection();
            string sql = @"
        INSERT INTO PlatformLaw
        (Title, Category, Content, Source, IssuedDate, LastUpdated, CreatedBy)
        VALUES
        (@Title, @Category, @Content, @Source, @IssuedDate, GETDATE(), @CreatedBy)";
            return await db.ExecuteAsync(sql, item);
        }

        public async Task<int> UpdateAsync(PlatformLaw item)
        {
            using var db = CreateConnection();
            string sql = @"
        UPDATE PlatformLaw SET
            Title = @Title,
            Category = @Category,
            Content = @Content,
            Source = @Source,
            IssuedDate = @IssuedDate,
            LastUpdated = GETDATE(),
            CreatedBy = @CreatedBy
        WHERE Id = @Id";
            return await db.ExecuteAsync(sql, item);
        }

        public async Task<int> DeleteAsync(int id)
        {
            using var db = CreateConnection();
            return await db.ExecuteAsync("DELETE FROM PlatformLaw WHERE Id = @Id", new { Id = id });
        }
    }
}
