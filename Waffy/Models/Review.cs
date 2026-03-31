namespace Waffy.Models
{
    using System;

    namespace Waffy.Models
    {
        public class Review
        {
            public int Id { get; set; }
            public int UserId { get; set; }
            public string TargetType { get; set; } = null!;
            public int TargetId { get; set; }
            public int Rating { get; set; }
            public string? Comment { get; set; }
            public string Status { get; set; } = "Visible";
            public DateTime? UpdatedAt { get; set; }
            public DateTime CreatedAt { get; set; }
        }
    }
}
