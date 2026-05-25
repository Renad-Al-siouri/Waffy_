using Dapper;
using Microsoft.Data.SqlClient;
using Waffy.Interfaces;
using Waffy.Models;

namespace Waffy.Repositories
{
    public class ClinicDocumentRepository : IClinicDocumentRepository
    {
        private readonly string _ConnectionString;

        public ClinicDocumentRepository(IConfiguration configuration)
        {
            _ConnectionString = configuration.GetConnectionString("WaffyConnection")!;
        }

        // ✅ إضافة وثيقة جديدة
        public async Task AddAsync(ClinicDocument document)
        {
            using var connection = new SqlConnection(_ConnectionString);
            var sql = @"
                INSERT INTO ClinicDocument 
                    (IdClinic, DocumentType, FileUrl, Status, ExpiryDate, UploadedAt)
                VALUES 
                    (@IdClinic, @DocumentType, @FileUrl, @Status, @ExpiryDate, @UploadedAt)";

            await connection.ExecuteAsync(sql, document);
        }

        // ✅ جلب كل وثائق عيادة معينة
        public async Task<IEnumerable<ClinicDocument>> GetByClinicIdAsync(int idClinic)
        {
            using var connection = new SqlConnection(_ConnectionString);
            var sql = "SELECT * FROM ClinicDocument WHERE IdClinic = @IdClinic";
            return await connection.QueryAsync<ClinicDocument>(sql, new { IdClinic = idClinic });
        }

        // ✅ حذف وثيقة
        public async Task DeleteAsync(int id)
        {
            using var connection = new SqlConnection(_ConnectionString);
            var sql = "DELETE FROM ClinicDocument WHERE Id = @Id";
            await connection.ExecuteAsync(sql, new { Id = id });
        }

        // أضف هاي الـ method داخل الـ class الموجود

        public async Task UpdateStatusAsync(int docId, string status, string? adminNote, int reviewedByAdminId)
        {
            using var connection = new SqlConnection(_ConnectionString);
            var sql = @"
        UPDATE ClinicDocument
        SET Status            = @status,
            AdminNote         = @adminNote,
            ReviewedAt        = GETUTCDATE(),
            ReviewedByAdminId = @reviewedByAdminId
        WHERE Id = @docId";
            await connection.ExecuteAsync(sql, new { docId, status, adminNote, reviewedByAdminId });
        }
    }
}