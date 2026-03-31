namespace Waffy.Models
{
    using System;

    namespace Waffy.Models
    {
        public class PlatformLaw
        {
            public int Id { get; set; }
            public string Title { get; set; } = null!;
            public string Category { get; set; } = null!;
            public string Content { get; set; } = null!;
            public string? Source { get; set; }
            public DateTime? IssuedDate { get; set; }
            public DateTime LastUpdated { get; set; }
            public int CreatedBy { get; set; }
        }
    }
}
