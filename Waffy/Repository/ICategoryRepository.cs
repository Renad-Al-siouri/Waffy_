using Waffy.Models;
namespace Waffy.Interfaces
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<Category>> GetByStoreIdAsync(int storeId);
        Task<Category?> GetByIdAsync(int id);
        Task<int> AddAsync(Category category);
        Task UpdateAsync(Category category);
        Task DeleteAsync(int id);
    }
}