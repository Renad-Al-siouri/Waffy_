// ══════════════════════════════════════════════════════════
// Interfaces/IReviewRepository.cs
// ══════════════════════════════════════════════════════════
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Waffy.Models;

namespace Waffy.Interfaces
{
    public interface IReviewRepository
    {
        // جلب كل reviews لعيادة أو طبيب
        Task<IEnumerable<Review>> GetByTargetAsync(string targetType, int targetId);

        // هل قيّم هذا المستخدم مسبقاً؟
        Task<bool> HasReviewedAsync(int userId, string targetType, int targetId);

        // جلب review معين
        Task<Review?> GetByIdAsync(int id);

        // إضافة review جديد + تحديث Rating
        Task<int> AddAsync(Review review);

        // حذف (Status = Deleted) + تحديث Rating — للـ SuperAdmin
        Task DeleteAsync(int id);

        // جلب كل reviews (للـ SuperAdmin)
        Task<IEnumerable<Review>> GetAllAsync(string? targetType = null);

        // إجمالي عدد الـ reviews لهدف معين
        Task<int> CountByTargetAsync(string targetType, int targetId);
    }
}