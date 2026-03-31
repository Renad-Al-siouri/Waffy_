namespace Waffy.Repositories
{
    using Dapper;
    using Microsoft.Data.SqlClient;
    using System.Data;
    using Waffy.Models;
    using Waffy.Models.Waffy.Models;

    public class ProductRepository
    {
        private readonly IConfiguration _config;

        public ProductRepository(IConfiguration config)
        {
            _config = config;
        }

        private IDbConnection CreateConnection()
        {
            return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            using var db = CreateConnection();
            return await db.QueryAsync<Product>("SELECT * FROM Product ORDER BY Id DESC");
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            using var db = CreateConnection();
            return await db.QueryFirstOrDefaultAsync<Product>("SELECT * FROM Product WHERE Id = @Id", new { Id = id });
        }

        public async Task<int> CreateAsync(Product item)
        {
            using var db = CreateConnection();
            string sql = @"
        INSERT INTO Product
        (StoreId, CategoryId, Name, Description, Price, SalePrice, StockQuantity, PetType, ImageUrl, IsVisible, CreatedAt)
        VALUES
        (@StoreId, @CategoryId, @Name, @Description, @Price, @SalePrice, @StockQuantity, @PetType, @ImageUrl, @IsVisible, GETDATE())";
            return await db.ExecuteAsync(sql, item);
        }

        public async Task<int> UpdateAsync(Product item)
        {
            using var db = CreateConnection();
            string sql = @"
        UPDATE Product SET
            StoreId = @StoreId,
            CategoryId = @CategoryId,
            Name = @Name,
            Description = @Description,
            Price = @Price,
            SalePrice = @SalePrice,
            StockQuantity = @StockQuantity,
            PetType = @PetType,
            ImageUrl = @ImageUrl,
            IsVisible = @IsVisible
        WHERE Id = @Id";
            return await db.ExecuteAsync(sql, item);
        }

        public async Task<int> DeleteAsync(int id)
        {
            using var db = CreateConnection();
            return await db.ExecuteAsync("DELETE FROM Product WHERE Id = @Id", new { Id = id });
        }
    }
}
