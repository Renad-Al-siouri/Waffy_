namespace Waffy.Models
{
    using System;

    namespace Waffy.Models
    {
        public class ClaimAttemptLog
        {
            public int Id { get; set; }
            public int ClaimId { get; set; }
            public string AttemptValue { get; set; } = null!;
            public bool IsCorrect { get; set; }
            public string? IPAddress { get; set; }
            public DateTime CreatedAt { get; set; }
        }
    }
}
