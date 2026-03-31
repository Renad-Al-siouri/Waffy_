namespace Waffy.Models
{
    using System;

    namespace Waffy.Models
    {
        public class ReportActionLog
        {
            public int Id { get; set; }
            public string EntityType { get; set; } = null!;
            public int EntityId { get; set; }
            public string ActionType { get; set; } = null!;
            public string? StatusAfterAction { get; set; }
            public string? Note { get; set; }
            public int AdminId { get; set; }
            public DateTime CreatedAt { get; set; }
        }
    }
}
