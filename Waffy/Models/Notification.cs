namespace Waffy.Models
{
    using System;

    namespace Waffy.Models
    {
        public class Notification
        {
            public int Id { get; set; }
            public int UserId { get; set; }
            public string Type { get; set; } = null!;
            public string Title { get; set; } = null!;
            public string Message { get; set; } = null!;
            public bool IsRead { get; set; }
            public DateTime? ReadAt { get; set; }
            public string? RelatedEntityType { get; set; }
            public int? RelatedEntityId { get; set; }
            public DateTime CreatedAt { get; set; }
        }
    }
}
