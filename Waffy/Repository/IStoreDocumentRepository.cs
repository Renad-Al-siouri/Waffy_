using Waffy.Models;

namespace Waffy.Interfaces
{
    public interface IStoreDocumentRepository
    {
        // جلب جميع وثائق متجر معين
        Task<IEnumerable<StoreDocument>> GetByStoreIdAsync(int idStore);

        // جلب وثيقة واحدة بالـ ID
        Task<StoreDocument?> GetByIdAsync(int id);

        // إضافة وثيقة جديدة
        Task AddAsync(StoreDocument document);

        // تحديث حالة الوثيقة (مثلاً Approved / Rejected)
        Task UpdateStatusAsync(int id, string status);

        // حذف وثيقة
        Task DeleteAsync(int id);
    }
}