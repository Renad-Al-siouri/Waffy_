using Dapper;
using Microsoft.Data.SqlClient;
using Waffy.Interfaces;
using Waffy.Models;

namespace Waffy.Repository
{
    public class ProductRepository : IProductRepository
    {
        private readonly string _connectionString;

        public ProductRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("WaffyConnection")
                ?? throw new Exception("Connection string not found");
        }

        public async Task<IEnumerable<Product>> GetByStoreIdAsync(int idStore)
        {
            var sql = "SELECT * FROM Product WHERE IdStore = @IdStore ORDER BY CreatedAt DESC";
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<Product>(sql, new { IdStore = idStore });
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            var sql = "SELECT * FROM Product WHERE Id = @Id";
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<Product>(sql, new { Id = id });
        }

        public async Task<int> AddAsync(Product product)
        {
            var sql = @"
                INSERT INTO Product
                    (IdStore, IdCategory, Name, Description, Price, SalePrice,
                     StockQuantity, PetType, ImageUrl, IsVisible, CreatedAt)
                VALUES
                    (@IdStore, @IdCategory, @Name, @Description, @Price, @SalePrice,
                     @StockQuantity, @PetType, @ImageUrl, @IsVisible, @CreatedAt);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";
            using var connection = new SqlConnection(_connectionString);
            return await connection.ExecuteScalarAsync<int>(sql, product);
        }

        public async Task UpdateAsync(Product product)
        {
            var sql = @"
                UPDATE Product SET
                    IdCategory      = @IdCategory,
                    Name            = @Name,
                    Description     = @Description,
                    Price           = @Price,
                    SalePrice       = @SalePrice,
                    StockQuantity   = @StockQuantity,
                    PetType         = @PetType,
                    ImageUrl        = @ImageUrl,
                    IsVisible       = @IsVisible
                WHERE Id = @Id";
            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(sql, product);
        }

        public async Task DeleteAsync(int id)
        {
            var sql = "DELETE FROM Product WHERE Id = @Id";
            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(sql, new { Id = id });
        }

        public async Task UpdateVisibilityAsync(int id, bool isVisible)
        {
            var sql = "UPDATE Product SET IsVisible = @IsVisible WHERE Id = @Id";
            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(sql, new { Id = id, IsVisible = isVisible });
        }

        public async Task UpdateStockAsync(int id, int newQuantity)
        {
            var sql = "UPDATE Product SET StockQuantity = @StockQuantity WHERE Id = @Id";
            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(sql, new { Id = id, StockQuantity = newQuantity });
        }
        public async Task DeductStockAsync(int productId, int quantity)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.ExecuteAsync(@"
        UPDATE Product
        SET StockQuantity = CASE
            WHEN StockQuantity - @Quantity < 0 THEN 0
            ELSE StockQuantity - @Quantity
        END
        WHERE Id = @ProductId",
                new { ProductId = productId, Quantity = quantity });
        }
    }
}