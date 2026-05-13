using Waffy.Models;
using Waffy.Models.ViewModels;

namespace Waffy.Interfaces
{
    public interface IPostFlagRepository
    {
        Task<bool> HasFlaggedAsync(int postId, int userId);
        Task AddFlagAsync(PostFlag flag);
        Task<IEnumerable<FlaggedPostDto>> GetAllFlaggedPostsAsync();
        Task ClearFlagsAsync(int postId);
        Task<Dictionary<int, int>> GetFlagCountsForPostsAsync(IEnumerable<int> postIds);
        Task<IEnumerable<PostFlag>> GetByUserIdAsync(int userId);
        Task MarkAsReviewedAsync(int postId, int adminId, string action, string? note);
        Task<IEnumerable<FlagDetailDto>> GetFlagsByPostIdAsync(int postId);
        Task<IEnumerable<FlaggedPostDto>> GetFlagHistoryAsync();
        Task LogActionAsync(int postId, int adminId, string actionType,
    int? targetUserId, string? targetName, string? message);
        Task<IEnumerable<PostFlagActionDto>> GetActionsByPostIdAsync(int postId);
        Task RevertReviewAsync(int postId);
    }
}