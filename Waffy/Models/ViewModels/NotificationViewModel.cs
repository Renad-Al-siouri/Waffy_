namespace Waffy.Models.ViewModels
{
    public class NotificationViewModel
    {
        public IEnumerable<Waffy.Models.Notification> Notifications { get; set; }
            = new List<Waffy.Models.Notification>();
        public int UnreadCount { get; set; }
        public int TotalCount { get; set; }
    }
}