using Dapper;
using Microsoft.Data.SqlClient;
using Waffy.Interfaces;
using Waffy.Models;

namespace Waffy.Repository
{
    public class StoreDocumentRepository : IStoreDocumentRepository
    {
        private readonly string _connectionString;

        public StoreDocumentRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("WaffyConnection")
                ?? throw new Exception("Connection string not found");
        }

        public async Task<IEnumerable<StoreDocument>> GetByStoreIdAsync(int idStore)
        {
            var sql = @"SELECT * FROM StoreDocument 
                        WHERE IdStore = @IdStore 
                        ORDER BY UploadedAt DESC";
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<StoreDocument>(sql, new { IdStore = idStore });
        }

        public async Task<StoreDocument?> GetByIdAsync(int id)
        {
            var sql = "SELECT * FROM StoreDocument WHERE Id = @Id";
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<StoreDocument>(sql, new { Id = id });
        }

        public async Task AddAsync(StoreDocument document)
        {
            var sql = @"
                INSERT INTO StoreDocument
                    (IdStore, DocumentType, FileUrl, Status, ExpiryDate, UploadedAt)
                VALUES
                    (@IdStore, @DocumentType, @FileUrl, @Status, @ExpiryDate, @UploadedAt)";
            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(sql, document);
        }

        // ← محدّث: يحفظ AdminNote و ReviewedAt و ReviewedByAdminId
        public async Task UpdateStatusAsync(int id, string status, string? adminNote, int adminId)
        {
            var sql = @"
                UPDATE StoreDocument SET
                    Status              = @Status,
                    AdminNote           = @AdminNote,
                    ReviewedAt          = @ReviewedAt,
                    ReviewedByAdminId   = @AdminId
                WHERE Id = @Id";
            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(sql, new
            {
                Id = id,
                Status = status,
                AdminNote = adminNote,
                ReviewedAt = DateTime.UtcNow,
                AdminId = adminId
            });
        }

        public async Task DeleteAsync(int id)
        {
            var sql = "DELETE FROM StoreDocument WHERE Id = @Id";
            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(sql, new { Id = id });
        }
    }
}