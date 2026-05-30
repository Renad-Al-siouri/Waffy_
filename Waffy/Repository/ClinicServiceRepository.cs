using Dapper;
using Microsoft.Data.SqlClient;
using Waffy.Interfaces;
using Waffy.Models;

namespace Waffy.Repositories
{
    public class ClinicServiceRepository : IClinicServiceRepository
    {
        private readonly string _ConnectionString;

        public ClinicServiceRepository(IConfiguration config)
        {
            _ConnectionString = config.GetConnectionString("WaffyConnection")
    ?? throw new InvalidOperationException("Connection string 'WaffyConnection' not found.");
        }

        public async Task<IEnumerable<ClinicService>> GetByClinicIdAsync(int clinicId)
        {
            using var conn = new SqlConnection(_ConnectionString);
            return await conn.QueryAsync<ClinicService>(
                "SELECT * FROM ClinicService WHERE IdClinic = @IdClinic",
                new { IdClinic = clinicId });
        }

        public async Task DeleteByClinicIdAsync(int clinicId)
        {
            using var conn = new SqlConnection(_ConnectionString);
            await conn.ExecuteAsync(
                "DELETE FROM ClinicService WHERE IdClinic = @IdClinic",
                new { IdClinic = clinicId });
        }

        public async Task InsertAsync(ClinicService service)
        {
            using var conn = new SqlConnection(_ConnectionString);
            await conn.ExecuteAsync(@"
                INSERT INTO ClinicService (Id, IdClinic, DocumentType, Status, UploadedAt)
                VALUES (@Id, @IdClinic, @DocumentType, @Status, @UploadedAt)",
                service);
        }

        public async Task<int> GetNextIdAsync()
        {
            using var conn = new SqlConnection(_ConnectionString);
            var max = await conn.ExecuteScalarAsync<int?>(
                "SELECT ISNULL(MAX(Id), 0) FROM ClinicService");
            return (max ?? 0) + 1;
        }
    }
}