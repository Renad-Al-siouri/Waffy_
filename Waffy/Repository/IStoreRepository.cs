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

        // تحديث حالة الاعتماد
        Task UpdateVerificationAsync(int idStore, string status);

        // جلب المتاجر المنتظرة (للأدمن)
        Task<IEnumerable<Store>> GetPendingStoresAsync();

        // ✅ جديد — جلب المتاجر المعتمدة فقط (للعرض العام)
        Task<IEnumerable<Store>> GetApprovedStoresAsync();

        Task<int?> GetOwnerUserIdAsync(int storeId);
    }
}