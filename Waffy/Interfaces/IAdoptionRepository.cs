using Waffy.Models;
using Waffy.Models.ViewModels;

namespace Waffy.Interfaces
{
    public interface IAdoptionRepository
    {
        // Public listings (Approved + Available)
        Task<IEnumerable<AdoptionPost>> GetApprovedAsync();

        // User's own posts
        Task<IEnumerable<AdoptionPost>> GetByUserIdAsync(int userId);

        // Single post
        Task<AdoptionPost?> GetByIdAsync(int idPost);

        // CRUD
        Task<int> AddAsync(AdoptionPost post);
        Task UpdateAsync(AdoptionPost post);
        Task UpdateStatusAsync(int idPost, string adoptionStatus);
        Task DeleteAsync(int idPost);

        // Images
        Task AddImageAsync(int idPost, string imageUrl);
        Task<IEnumerable<AdoptionImage>> GetImagesByPostIdAsync(int idPost);
        Task<string?> GetFirstImageAsync(int idPost);

        Task UpdatePostAsync(AdoptionPost post);
        Task ClosePostAsync(int postId);
        Task<bool> HasLikedAsync(int postId, int userId);
        Task AddLikeAsync(int postId, int userId);
        Task RemoveLikeAsync(int postId, int userId);
        Task<int> GetLikesAsync(int postId);
        Task<IEnumerable<string>> GetAllImagesAsync(int postId);
        Task<IEnumerable<AdoptionPostAdminDto>> GetAllAsync();
        Task ApproveAsync(int id);
        Task RejectAsync(int id);
        // ← أضف هذين فقط
        Task<IEnumerable<AdoptionPost>> GetByStoreIdAsync(int storeId);
        Task ChangeAdoptionStatusAsync(int idPost, string adoptionStatus);
        Task<IEnumerable<AdoptionPost>> GetByClinicIdAsync(int clinicId);
    }
}
