namespace Waffy.Models
{
    using System;

    namespace Waffy.Models
    {
        public class Complaint
        {
            public int Id { get; set; }
            public int UserId { get; set; }
            public string TargetType { get; set; } = null!;
            public int? TargetId { get; set; }
            public string Title { get; set; } = null!;
            public string Description { get; set; } = null!;
            public string Status { get; set; } = "Open";
            public string Priority { get; set; } = "Medium";
            public DateTime? ResolvedAt { get; set; }
            public int? ResolvedByAdminId { get; set; }
            public DateTime CreatedAt { get; set; }
        }
    }
}
