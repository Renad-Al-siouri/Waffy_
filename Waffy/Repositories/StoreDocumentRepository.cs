namespace Waffy.Repositories
{
    using Dapper;
    using Microsoft.Data.SqlClient;
    using System.Data;
    using Waffy.Models;
    using Waffy.Models.Waffy.Models;

    public class StoreDocumentRepository
    {
        private readonly IConfiguration _config;

        public StoreDocumentRepository(IConfiguration config)
        {
            _config = config;
        }

        private IDbConnection CreateConnection()
        {
            return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
        }

        public async Task<IEnumerable<StoreDocument>> GetAllAsync()
        {
            using var db = CreateConnection();
            return await db.QueryAsync<StoreDocument>("SELECT * FROM StoreDocument ORDER BY Id DESC");
        }

        public async Task<StoreDocument?> GetByIdAsync(int id)
        {
            using var db = CreateConnection();
            return await db.QueryFirstOrDefaultAsync<StoreDocument>(
                "SELECT * FROM StoreDocument WHERE Id = @Id", new { Id = id });
        }

        public async Task<IEnumerable<StoreDocument>> GetByStoreIdAsync(int storeId)
        {
            using var db = CreateConnection();
            return await db.QueryAsync<StoreDocument>(
                "SELECT * FROM StoreDocument WHERE StoreId = @StoreId ORDER BY Id DESC",
                new { StoreId = storeId });
        }

        public async Task<int> CreateAsync(StoreDocument item)
        {
            using var db = CreateConnection();
            string sql = @"
        INSERT INTO StoreDocument
        (StoreId, DocumentType, FileUrl, Status, ExpiryDate, UploadedAt)
        VALUES
        (@StoreId, @DocumentType, @FileUrl, @Status, @ExpiryDate, GETDATE())";
            return await db.ExecuteAsync(sql, item);
        }

        public async Task<int> UpdateAsync(StoreDocument item)
        {
            using var db = CreateConnection();
            string sql = @"
        UPDATE StoreDocument SET
            StoreId = @StoreId,
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
            return await db.ExecuteAsync("DELETE FROM StoreDocument WHERE Id = @Id", new { Id = id });
        }
    }
}
