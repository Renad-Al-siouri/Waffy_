using Waffy.Models;

namespace Waffy.Interfaces
{
    public interface IPetRepository
    {
        // ══ Pet CRUD ══════════════════════════════════════════
        Task<IEnumerable<Pet>> GetByUserIdAsync(int userId);
        Task<Pet?> GetByIdAsync(int petId);
        Task<int> AddAsync(Pet pet);
        Task UpdateAsync(Pet pet);
        Task DeleteAsync(int petId);
        Task UpdateImageAsync(int petId, string imageUrl);
        Task TransferOwnershipAsync(int petId, int newOwnerId);

        // ══ PetImage ══════════════════════════════════════════

        /// <summary>إرجاع كل صور الحيوان مرتبة حسب SortOrder</summary>
        Task<IEnumerable<PetImage>> GetImagesAsync(int petId);

        /// <summary>
        /// إضافة صورة جديدة للحيوان.
        /// لو isMain = true تُعيَّن كصورة رئيسية وتُحدَّث Pet.ImageUrl
        /// </summary>
        Task<int> AddPetImageAsync(int petId, string imageUrl, bool isMain = false);

        /// <summary>حذف صورة بالـ Id</summary>
        Task DeletePetImageAsync(int imageId);

        /// <summary>
        /// تعيين صورة كـ Main:
        ///  1. يُزيل IsMain عن كل صور الحيوان
        ///  2. يضع IsMain على الصورة المحددة
        ///  3. يُحدِّث Pet.ImageUrl
        /// </summary>
        Task SetMainImageAsync(int petId, int imageId);

        /// <summary>
        /// مزامنة Pet.ImageUrl مع أول صورة رئيسية في PetImage.
        /// يُستدعى بعد الحذف أو الإضافة.
        /// </summary>
        Task SyncMainImageAsync(int petId);
    }
}