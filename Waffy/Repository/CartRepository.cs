using Dapper;
using Microsoft.Data.SqlClient;
using Waffy.Interfaces;
using Waffy.Models;

namespace Waffy.Repository
{
    public class CartRepository : ICartRepository
    {
        private readonly string _connectionString;

        public CartRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("WaffyConnection")
                ?? throw new Exception("WaffyConnection not found");
        }

        // ── جلب أو إنشاء عربة ────────────────────────────────────
        public async Task<Cart> GetOrCreateCartAsync(int userId)
        {
            using var connection = new SqlConnection(_connectionString);

            // ✅ نحدد الأعمدة صراحةً بدل SELECT * لضمان الـ Mapping
            var existing = await connection.QueryFirstOrDefaultAsync<Cart>(@"
                SELECT IdCart, IdUser, CreatedAt, UpdatedAt
                FROM Cart
                WHERE IdUser = @UserId",
                new { UserId = userId });

            if (existing != null) return existing;

            // إنشاء cart جديدة
            var newId = await connection.ExecuteScalarAsync<int>(@"
                INSERT INTO Cart (IdUser, CreatedAt, UpdatedAt)
                OUTPUT INSERTED.IdCart
                VALUES (@UserId, @Now, @Now)",
                new { UserId = userId, Now = DateTime.UtcNow });

            return new Cart
            {
                IdCart = newId,
                IdUser = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        // ── جلب عناصر العربة مع بيانات المنتج والمتجر ────────────
        public async Task<IEnumerable<CartItem>> GetCartItemsAsync(int idCart)
        {
            using var conn = new SqlConnection(_connectionString);

            // ── 1. جلب العناصر الأساسية ──────────────────────────────
            var items = (await conn.QueryAsync<CartItem>(@"
        SELECT
            ci.Id, ci.IdCart, ci.IdProduct, ci.IdStore,
            ci.Quantity, ci.AddedAt,
            ISNULL(p.Name,  'Unknown')   AS ProductName,
            p.ImageUrl                   AS ProductImage,
            ISNULL(p.Price, 0)           AS UnitPrice,
            p.SalePrice,
            ISNULL(p.StockQuantity, 0)   AS StockQuantity,
            ISNULL(s.Name, 'Unknown')    AS StoreName
        FROM CartItem ci
        LEFT JOIN Product p ON p.Id      = ci.IdProduct
        LEFT JOIN Store   s ON s.IdStore = ci.IdStore
        WHERE ci.IdCart = @IdCart
        ORDER BY ci.AddedAt DESC",
                new { IdCart = idCart })).ToList();

            if (!items.Any()) return items;

            // ── 2. جلب صور المنتجات دفعة واحدة ──────────────────────
            var productIds = items.Select(i => i.IdProduct).Distinct().ToList();

            var allImages = (await conn.QueryAsync<(int IdProduct, string ImageUrl, bool IsMain, int SortOrder)>(@"
        SELECT IdProduct, ImageUrl, IsMain, SortOrder
        FROM   ProductImage
        WHERE  IdProduct IN @Ids
        ORDER  BY IdProduct, IsMain DESC, SortOrder ASC",
                new { Ids = productIds })).ToList();

            // ── 3. ربط الصور بكل عنصر ────────────────────────────────
            foreach (var item in items)
            {
                var imgs = allImages
                    .Where(i => i.IdProduct == item.IdProduct)
                    .Select(i => i.ImageUrl)
                    .ToList();

                if (imgs.Any())
                {
                    item.Images = imgs;
                    // أول صورة (main) هي الافتراضية
                    item.ProductImage = imgs.First();
                }
                else if (!string.IsNullOrEmpty(item.ProductImage))
                {
                    item.Images = new List<string> { item.ProductImage };
                }
            }

            return items;
        }
        // ── إضافة أو تعديل عنصر ──────────────────────────────────
        public async Task AddOrUpdateItemAsync(int idCart, int idProduct, int idStore, int quantity)
        {
            using var connection = new SqlConnection(_connectionString);

            // ✅ تحقق أن الـ idCart صحيح قبل الإدراج
            if (idCart <= 0)
                throw new Exception($"Invalid CartId: {idCart}");

            var sql = @"
                IF EXISTS (
                    SELECT 1 FROM CartItem 
                    WHERE IdCart = @IdCart AND IdProduct = @IdProduct
                )
                    UPDATE CartItem
                    SET    Quantity = Quantity + @Quantity
                    WHERE  IdCart = @IdCart AND IdProduct = @IdProduct
                ELSE
                    INSERT INTO CartItem (IdCart, IdProduct, IdStore, Quantity, AddedAt)
                    VALUES (@IdCart, @IdProduct, @IdStore, @Quantity, @Now)";

            await connection.ExecuteAsync(sql, new
            {
                IdCart = idCart,
                IdProduct = idProduct,
                IdStore = idStore,
                Quantity = quantity,
                Now = DateTime.UtcNow
            });

            await connection.ExecuteAsync(
                "UPDATE Cart SET UpdatedAt = @Now WHERE IdCart = @IdCart",
                new { IdCart = idCart, Now = DateTime.UtcNow });
        }

        // ── تعديل الكمية ─────────────────────────────────────────
        public async Task UpdateQuantityAsync(int itemId, int quantity)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(
                "UPDATE CartItem SET Quantity = @Quantity WHERE Id = @Id",
                new { Id = itemId, Quantity = quantity });
        }

        // ── حذف عنصر ─────────────────────────────────────────────
        public async Task RemoveItemAsync(int itemId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(
                "DELETE FROM CartItem WHERE Id = @Id",
                new { Id = itemId });
        }

        // ── تفريغ العربة ─────────────────────────────────────────
        public async Task ClearCartAsync(int idCart)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(
                "DELETE FROM CartItem WHERE IdCart = @IdCart",
                new { IdCart = idCart });
        }

        // ── عدد العناصر ──────────────────────────────────────────
        public async Task<int> GetCartCountAsync(int idCart)
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.ExecuteScalarAsync<int>(
                "SELECT ISNULL(SUM(Quantity), 0) FROM CartItem WHERE IdCart = @IdCart",
                new { IdCart = idCart });
        }

        public async Task ClearCartByStoreAsync(int idCart, int idStore)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.ExecuteAsync(
                "DELETE FROM CartItem WHERE IdCart = @IdCart AND IdStore = @IdStore",
                new { IdCart = idCart, IdStore = idStore });
        }

        public async Task RemoveOutOfStockItemsAsync(int idCart)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.ExecuteAsync(@"
        DELETE ci
        FROM CartItem ci
        INNER JOIN Product p ON p.Id = ci.IdProduct
        WHERE ci.IdCart = @IdCart
          AND p.StockQuantity <= 0",
                new { IdCart = idCart });
        }

        public async Task<int> GetItemQuantityAsync(int cartId, int productId)
        {
            using var conn = new SqlConnection(_connectionString);
            return await conn.ExecuteScalarAsync<int>(@"
        SELECT ISNULL(Quantity, 0) FROM CartItem 
        WHERE IdCart = @CartId AND IdProduct = @ProductId",
                new { CartId = cartId, ProductId = productId });
        }

        public async Task<CartItem?> GetItemByIdAsync(int itemId)
        {
            using var conn = new SqlConnection(_connectionString);
            return await conn.QueryFirstOrDefaultAsync<CartItem>(
                "SELECT * FROM CartItem WHERE Id = @Id",
                new { Id = itemId });
        }
    }
}