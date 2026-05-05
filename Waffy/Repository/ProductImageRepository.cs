using Dapper;
using Microsoft.Data.SqlClient;
using Waffy.Interfaces;
using Waffy.Models;

namespace Waffy.Repository
{
    public class ProductImageRepository : IProductImageRepository
    {
        private readonly string _connectionString;

        public ProductImageRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("WaffyConnection")
                ?? throw new Exception("WaffyConnection not found");
        }

        public async Task<IEnumerable<ProductImage>> GetByProductIdAsync(int productId)
        {
            var sql = @"SELECT * FROM ProductImage 
                        WHERE IdProduct = @ProductId 
                        ORDER BY IsMain DESC, SortOrder ASC";
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<ProductImage>(sql, new { ProductId = productId });
        }

        public async Task<int> AddAsync(ProductImage image)
        {
            var sql = @"INSERT INTO ProductImage 
                            (IdProduct, ImageUrl, SortOrder, IsMain, UploadedAt)
                        OUTPUT INSERTED.Id
                        VALUES 
                            (@IdProduct, @ImageUrl, @SortOrder, @IsMain, @UploadedAt)";
            using var connection = new SqlConnection(_connectionString);
            return await connection.ExecuteScalarAsync<int>(sql, new
            {
                image.IdProduct,
                image.ImageUrl,
                image.SortOrder,
                image.IsMain,
                UploadedAt = DateTime.UtcNow
            });
        }

        public async Task DeleteAsync(int id)
        {
            var sql = "DELETE FROM ProductImage WHERE Id = @Id";
            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(sql, new { Id = id });
        }

        public async Task DeleteByProductIdAsync(int productId)
        {
            var sql = "DELETE FROM ProductImage WHERE IdProduct = @ProductId";
            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(sql, new { ProductId = productId });
        }

        public async Task SetMainImageAsync(int productId, int imageId)
        {
            var sql = @"UPDATE ProductImage SET IsMain = 0 WHERE IdProduct = @ProductId;
                        UPDATE ProductImage SET IsMain = 1 
                        WHERE Id = @ImageId AND IdProduct = @ProductId;";
            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(sql, new { ProductId = productId, ImageId = imageId });
        }
    }
}