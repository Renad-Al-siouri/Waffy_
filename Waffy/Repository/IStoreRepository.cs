using Waffy.Models;

namespace Waffy.Interfaces
{
    public interface IStoreRepository
    {
        // جلب جميع المتاجر
        Task<IEnumerable<Store>> GetAllAsync();

        // جلب متجر بالـ ID
        Task<Store?> GetByIdAsync(int idStore);

        // جلب متجر عن طريق OwnerUserId
        Task<Store?> GetByOwnerUserIdAsync(int ownerUserId);

        // إضافة متجر جديد — يرجع IdStore المُنشأ
        Task<int> AddAsync(Store store);

        // تحديث بيانات المتجر
        Task UpdateAsync(Store store);

        // حذف متجر
        Task DeleteAsync(int idStore);

        // أضف هذين السطرين في نهاية الـ interface
        Task UpdateVerificationAsync(int idStore, string status);
        Task<IEnumerable<Store>> GetPendingStoresAsync();
    }
}