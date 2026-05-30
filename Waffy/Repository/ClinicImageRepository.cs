using Dapper;
using Microsoft.Data.SqlClient;
using Waffy.Interfaces;
using Waffy.Models;

namespace Waffy.Repositories
{
    public class ClinicImageRepository : IClinicImageRepository
    {
        private readonly string _ConnectionString;

        public ClinicImageRepository(IConfiguration config)
        {
            _ConnectionString = config.GetConnectionString("WaffyConnection")
                ?? throw new InvalidOperationException("WaffyConnection not found.");
        }

        public async Task<IEnumerable<ClinicImage>> GetByClinicIdAsync(int clinicId)
        {
            using var conn = new SqlConnection(_ConnectionString);
            return await conn.QueryAsync<ClinicImage>(
                "SELECT * FROM ClinicImage WHERE IdClinic = @IdClinic ORDER BY SortOrder",
                new { IdClinic = clinicId });
        }

        public async Task AddAsync(ClinicImage image)
        {
            using var conn = new SqlConnection(_ConnectionString);
            await conn.ExecuteAsync(@"
                INSERT INTO ClinicImage (IdClinic, ImageUrl, SortOrder, UploadedAt)
                VALUES (@IdClinic, @ImageUrl, @SortOrder, @UploadedAt)",
                image);
        }

        public async Task DeleteAsync(int id)
        {
            using var conn = new SqlConnection(_ConnectionString);
            await conn.ExecuteAsync(
                "DELETE FROM ClinicImage WHERE Id = @Id",
                new { Id = id });
        }

        public async Task<int> CountByClinicIdAsync(int clinicId)
        {
            using var conn = new SqlConnection(_ConnectionString);
            return await conn.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM ClinicImage WHERE IdClinic = @IdClinic",
                new { IdClinic = clinicId });
        }
    }
}