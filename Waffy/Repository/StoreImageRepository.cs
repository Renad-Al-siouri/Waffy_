using Dapper;
using Microsoft.Data.SqlClient;
using Waffy.Interfaces;
using Waffy.Models;

namespace Waffy.Repository
{
    public class StoreImageRepository : IStoreImageRepository
    {
        private readonly string _connectionString;

        public StoreImageRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("WaffyConnection")
                ?? throw new Exception("WaffyConnection not found");
        }

        public async Task<IEnumerable<StoreImage>> GetByStoreIdAsync(int storeId)
        {
            var sql = @"SELECT * FROM StoreImage 
                        WHERE IdStore = @StoreId 
                        ORDER BY ImageType, SortOrder ASC";
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<StoreImage>(sql, new { StoreId = storeId });
        }

        public async Task<IEnumerable<StoreImage>> GetByStoreIdAndTypeAsync(int storeId, string imageType)
        {
            var sql = @"SELECT * FROM StoreImage 
                        WHERE IdStore = @StoreId AND ImageType = @ImageType 
                        ORDER BY SortOrder ASC";
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<StoreImage>(sql, new { StoreId = storeId, ImageType = imageType });
        }

        public async Task<int> AddAsync(StoreImage image)
        {
            var sql = @"INSERT INTO StoreImage 
                            (IdStore, ImageUrl, ImageType, SortOrder, UploadedAt)
                        OUTPUT INSERTED.Id
                        VALUES 
                            (@IdStore, @ImageUrl, @ImageType, @SortOrder, @UploadedAt)";
            using var connection = new SqlConnection(_connectionString);
            return await connection.ExecuteScalarAsync<int>(sql, new
            {
                image.IdStore,
                image.ImageUrl,
                image.ImageType,
                image.SortOrder,
                UploadedAt = DateTime.UtcNow
            });
        }

        public async Task DeleteAsync(int id)
        {
            var sql = "DELETE FROM StoreImage WHERE Id = @Id";
            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(sql, new { Id = id });
        }

        public async Task DeleteByStoreIdAndTypeAsync(int storeId, string imageType)
        {
            var sql = "DELETE FROM StoreImage WHERE IdStore = @StoreId AND ImageType = @ImageType";
            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(sql, new { StoreId = storeId, ImageType = imageType });
        }
    }
}
