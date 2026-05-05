using Waffy.Models;
namespace Waffy.Interfaces
{
    public interface IProductImageRepository
    {
        Task<IEnumerable<ProductImage>> GetByProductIdAsync(int productId);
        Task<int> AddAsync(ProductImage image);
        Task DeleteAsync(int id);
        Task DeleteByProductIdAsync(int productId);
        Task SetMainImageAsync(int productId, int imageId);
    }
}