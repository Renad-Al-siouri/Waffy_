namespace Waffy.Repositories
{
    using Dapper;
    using Microsoft.Data.SqlClient;
    using System.Data;
    using Waffy.Models;
    using Waffy.Models.Waffy.Models;

    public class ClinicDocumentRepository
    {
        private readonly IConfiguration _config;

        public ClinicDocumentRepository(IConfiguration config)
        {
            _config = config;
        }

        private IDbConnection CreateConnection()
        {
            return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
        }

        public async Task<IEnumerable<ClinicDocument>> GetAllAsync()
        {
            using var db = CreateConnection();
            return await db.QueryAsync<ClinicDocument>("SELECT * FROM ClinicDocument ORDER BY Id DESC");
        }

        public async Task<ClinicDocument?> GetByIdAsync(int id)
        {
            using var db = CreateConnection();
            return await db.QueryFirstOrDefaultAsync<ClinicDocument>("SELECT * FROM ClinicDocument WHERE Id = @Id", new { Id = id });
        }

        public async Task<int> CreateAsync(ClinicDocument item)
        {
            using var db = CreateConnection();
            string sql = @"
        INSERT INTO ClinicDocument (ClinicId, DocumentType, FileUrl, Status, ExpiryDate, UploadedAt)
        VALUES (@ClinicId, @DocumentType, @FileUrl, @Status, @ExpiryDate, GETDATE())";
            return await db.ExecuteAsync(sql, item);
        }

        public async Task<int> UpdateAsync(ClinicDocument item)
        {
            using var db = CreateConnection();
            string sql = @"
        UPDATE ClinicDocument SET
            ClinicId = @ClinicId,
            DocumentType = @DocumentType,
            FileUrl = @FileUrl,
            Status = @Status,
            ExpiryDate = @ExpiryDate
        WHERE Id = @Id";
            return await db.ExecuteAsync(sql, item);
        }

        public async Task<int> DeleteAsync(int id)
        {
            using var db = CreateConnection();
            return await db.ExecuteAsync("DELETE FROM ClinicDocument WHERE Id = @Id", new { Id = id });
        }
    }
}
