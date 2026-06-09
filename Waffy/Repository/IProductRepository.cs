using Waffy.Models;

namespace Waffy.Interfaces
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetByStoreIdAsync(int idStore);
        Task<Product?> GetByIdAsync(int id);
        Task<int> AddAsync(Product product);
        Task UpdateAsync(Product product);
        Task DeleteAsync(int id);
        Task UpdateVisibilityAsync(int id, bool isVisible);
        Task UpdateStockAsync(int id, int newQuantity);

        Task DeductStockAsync(int productId, int quantity);

        Task<IEnumerable<Product>> GetFilteredAsync(
    int storeId, string? search, int? categoryId, decimal? maxPrice);
        Task RestoreStockAsync(int idProduct, int quantity);
    }

}