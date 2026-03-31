namespace Waffy.Repositories
{
    using Dapper;
    using Microsoft.Data.SqlClient;
    using System.Data;
    using Waffy.Models;
    using Waffy.Models.Waffy.Models;

    public class DoctorDocumentRepository
    {
        private readonly IConfiguration _config;

        public DoctorDocumentRepository(IConfiguration config)
        {
            _config = config;
        }

        private IDbConnection CreateConnection()
        {
            return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
        }

        public async Task<IEnumerable<DoctorDocument>> GetAllAsync()
        {
            using var db = CreateConnection();
            return await db.QueryAsync<DoctorDocument>("SELECT * FROM DoctorDocument ORDER BY Id DESC");
        }

        public async Task<DoctorDocument?> GetByIdAsync(int id)
        {
            using var db = CreateConnection();
            return await db.QueryFirstOrDefaultAsync<DoctorDocument>("SELECT * FROM DoctorDocument WHERE Id = @Id", new { Id = id });
        }

        public async Task<int> CreateAsync(DoctorDocument item)
        {
            using var db = CreateConnection();
            string sql = @"
        INSERT INTO DoctorDocument (DoctorId, DocumentType, FileUrl, Status, ExpiryDate, UploadedAt)
        VALUES (@DoctorId, @DocumentType, @FileUrl, @Status, @ExpiryDate, GETDATE())";
            return await db.ExecuteAsync(sql, item);
        }

        public async Task<int> UpdateAsync(DoctorDocument item)
        {
            using var db = CreateConnection();
            string sql = @"
        UPDATE DoctorDocument SET
            DoctorId = @DoctorId,
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
            return await db.ExecuteAsync("DELETE FROM DoctorDocument WHERE Id = @Id", new { Id = id });
        }
    }
}
