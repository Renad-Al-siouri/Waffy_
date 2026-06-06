using Waffy.Models;

namespace Waffy.Interfaces
{
    public interface INotificationRepository
    {
        Task<IEnumerable<Notification>> GetByUserIdAsync(int userId);
        Task<int> GetUnreadCountAsync(int userId);
        Task AddAsync(Notification notification);
        Task MarkAsReadAsync(int notificationId);
        Task MarkAllAsReadAsync(int userId);

        Task AddBulkAsync(IEnumerable<int> userIds, string type,
    string title, string message, string? relatedEntityType, int? relatedEntityId);

     
    }
}