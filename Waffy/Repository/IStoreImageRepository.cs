using Waffy.Models;
namespace Waffy.Interfaces
{
    public interface IStoreImageRepository
    {
        Task<IEnumerable<StoreImage>> GetByStoreIdAsync(int storeId);
        Task<IEnumerable<StoreImage>> GetByStoreIdAndTypeAsync(int storeId, string imageType);
        Task<int> AddAsync(StoreImage image);
        Task DeleteAsync(int id);
        Task DeleteByStoreIdAndTypeAsync(int storeId, string imageType);
    }
}