namespace Waffy.Models
{
    using System;

    namespace Waffy.Models
    {
        public class ComplaintReceiver
        {
            public int Id { get; set; }
            public int ComplaintId { get; set; }
            public int ReceiverUserId { get; set; }
            public string ReceiverRole { get; set; } = null!;
            public bool IsRead { get; set; }
            public DateTime? ReadAt { get; set; }
            public DateTime CreatedAt { get; set; }
        }
    }
}
