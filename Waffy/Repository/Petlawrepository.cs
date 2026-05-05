using Dapper;
using Microsoft.Data.SqlClient;
using Waffy.Models;
using Waffy.Repositories.Interfaces;

namespace Waffy.Repositories
{
    /// <summary>
    /// Dapper implementation of <see cref="IPetLawRepository"/>.
    /// All queries target [dbo].[PetLaws].
    /// </summary>
    public class PetLawRepository : IPetLawRepository
    {
        private readonly string _connectionString;

        public PetLawRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("WaffyConnection")
                ?? throw new Exception("WaffyConnection not found");
        }

        // ── helpers ─────────────────────────────────────────────────────────

        private SqlConnection CreateConnection() => new SqlConnection(_connectionString);

        // ── Read ────────────────────────────────────────────────────────────

        public async Task<IEnumerable<PetLaw>> GetAllAsync()
        {
            const string sql = @"
                SELECT  Id, Title, Category, CategoryLabel,
                        Summary, Source, LastUpdated,
                        IsActive, CreatedAt, UpdatedAt
                FROM    [dbo].[PetLaws]
                WHERE   IsActive = 1
                ORDER BY LastUpdated DESC;";

            using var conn = CreateConnection();
            return await conn.QueryAsync<PetLaw>(sql);
        }

        public async Task<IEnumerable<PetLaw>> GetByCategoryAsync(string category)
        {
            const string sql = @"
                SELECT  Id, Title, Category, CategoryLabel,
                        Summary, Source, LastUpdated,
                        IsActive, CreatedAt, UpdatedAt
                FROM    [dbo].[PetLaws]
                WHERE   IsActive  = 1
                  AND   Category  = @Category
                ORDER BY LastUpdated DESC;";

            using var conn = CreateConnection();
            return await conn.QueryAsync<PetLaw>(sql, new { Category = category });
        }

        public async Task<IEnumerable<PetLaw>> SearchAsync(string keyword)
        {
            const string sql = @"
                SELECT  Id, Title, Category, CategoryLabel,
                        Summary, Source, LastUpdated,
                        IsActive, CreatedAt, UpdatedAt
                FROM    [dbo].[PetLaws]
                WHERE   IsActive = 1
                  AND   (
                            Title   LIKE N'%' + @Keyword + N'%'
                         OR Summary LIKE N'%' + @Keyword + N'%'
                        )
                ORDER BY LastUpdated DESC;";

            using var conn = CreateConnection();
            return await conn.QueryAsync<PetLaw>(sql, new { Keyword = keyword });
        }

        public async Task<PetLaw?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT  Id, Title, Category, CategoryLabel,
                        Summary, Source, LastUpdated,
                        IsActive, CreatedAt, UpdatedAt
                FROM    [dbo].[PetLaws]
                WHERE   Id = @Id;";

            using var conn = CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<PetLaw>(sql, new { Id = id });
        }

        public async Task<int> GetTotalCountAsync()
        {
            const string sql = @"
                SELECT COUNT(1) FROM [dbo].[PetLaws] WHERE IsActive = 1;";

            using var conn = CreateConnection();
            return await conn.ExecuteScalarAsync<int>(sql);
        }

        // ── Write ────────────────────────────────────────────────────────────

        public async Task<int> AddAsync(PetLaw law)
        {
            const string sql = @"
                INSERT INTO [dbo].[PetLaws]
                    (Title, Category, CategoryLabel, Summary, Source, LastUpdated, IsActive, CreatedAt, UpdatedAt)
                VALUES
                    (@Title, @Category, @CategoryLabel, @Summary, @Source, @LastUpdated, 1, GETDATE(), GETDATE());

                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using var conn = CreateConnection();
            return await conn.ExecuteScalarAsync<int>(sql, law);
        }

        public async Task<bool> UpdateAsync(PetLaw law)
        {
            const string sql = @"
                UPDATE [dbo].[PetLaws]
                SET    Title         = @Title,
                       Category      = @Category,
                       CategoryLabel = @CategoryLabel,
                       Summary       = @Summary,
                       Source        = @Source,
                       LastUpdated   = @LastUpdated,
                       UpdatedAt     = GETDATE()
                WHERE  Id = @Id;";

            using var conn = CreateConnection();
            int rows = await conn.ExecuteAsync(sql, law);
            return rows > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            const string sql = @"
                UPDATE [dbo].[PetLaws]
                SET    IsActive  = 0,
                       UpdatedAt = GETDATE()
                WHERE  Id = @Id;";

            using var conn = CreateConnection();
            int rows = await conn.ExecuteAsync(sql, new { Id = id });
            return rows > 0;
        }
    }
}