namespace Waffy.Models
{
    using System;

    namespace Waffy.Models
    {
        public class ComplaintReply
        {
            public int Id { get; set; }
            public int ComplaintId { get; set; }
            public int AdminId { get; set; }
            public string Message { get; set; } = null!;
            public DateTime CreatedAt { get; set; }
        }
    }
}
